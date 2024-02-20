// deltaProcess.js
import Client from "pg/lib/client.js";
import * as dotenv from "dotenv";
import fs from "fs";

dotenv.config({ path: '/home/cardano/data/cardanobi-backend-api/scripts/caches/.env' });

const clientCBI = new Client({
    host: "127.0.0.1",
    user: process.env.CARDANOBI_ADMIN_USERNAME,
    database: "cardanobi",
    password: process.env.CARDANOBI_ADMIN_PASSWORD,
    port: 5432,
});

const logFilePath = process.env.DELTA_PROCESS_LOG_FILE_PATH || 'delta_update_log.txt';
const lockFilePath = process.env.DELTA_PROCESS_LOCK_FILE || 'delta_update_lockfile.lock';
const batchSize = process.env.DELTA_PROCESS_TX_BATCH_SIZE ? parseInt(process.env.DELTA_PROCESS_TX_BATCH_SIZE, 10) : null;

const log = (message) => {
    const timestamp = new Date().toISOString().replace(/T/, ' ').replace(/\..+/, '');
    const logMessage = `${timestamp} - ${message}`;
    console.log(logMessage);
    fs.appendFileSync(logFilePath, logMessage + '\n');
};


const acquireLock = () => {
    try {
        fs.writeFileSync(lockFilePath, 'lock');
        return true;
    } catch (err) {
        log('Error acquiring lock: ' + err);
        return false;
    }
};

const releaseLock = () => {
    try {
        fs.unlinkSync(lockFilePath);
    } catch (err) {
        log('Error releasing lock: ' + err);
    }
};

const isLocked = () => {
    return fs.existsSync(lockFilePath);
};

// Listen for notice events from the PostgreSQL server
clientCBI.on('notice', (notice) => {
    const prefix = 'cbi_asset_addresses_cache_update';
    if (notice.message.startsWith(prefix)) {
        log(`PSQL Notice: ${notice.message}`);
    }
});


const callProcedure = async () => {
    if (isLocked()) {
        log('Another instance is running. Exiting.');
        return;
    }

    if (!acquireLock()) {
        log('Failed to acquire lock. Exiting.');
        return;
    }

    const startTime = Date.now();

    try {
        log('CBI_ASSET_ADDRESSES_CACHE - Delta Update - START PROCESSING...');

        const query = batchSize ?
            `CALL public.cbi_asset_addresses_cache_update(${batchSize});` :
            'CALL public.cbi_asset_addresses_cache_update();';
        await clientCBI.query(query);

        // await new Promise(resolve => setTimeout(resolve, 20000));
        // log('Simulated procedure execution completed.');

        const duration = (Date.now() - startTime) / 60000; // Duration in minutes
        log(`Procedure executed in ${duration.toFixed(2)} minutes`);

        // Retrieve last_tx_id from handler state
        const handlerStateRes = await clientCBI.query(`SELECT coalesce(last_tx_id, 0) AS last_tx_id FROM _cbi_cache_handler_state WHERE table_name = '_cbi_asset_addresses_cache';`);
        const lastProcessedTxId = handlerStateRes.rows[0].last_tx_id;
        log(`Last processed transaction ID: ${lastProcessedTxId}`);

        // Retrieve current max transaction id from tx table
        const maxTxIdRes = await clientCBI.query('SELECT max(id) AS max_tx_id FROM tx;');
        const maxTxId = maxTxIdRes.rows[0].max_tx_id;
        log(`Current max transaction ID: ${maxTxId}`);

        // Compute the difference and estimated time left
        const txBacklog = maxTxId - lastProcessedTxId;
        log(`Number of transactions outstanding: ${txBacklog}`);

        // Assuming constant processing time per batch
        const estimatedBatchesLeft = txBacklog / batchSize;
        const estimatedTimeLeft = estimatedBatchesLeft * duration;
        log(`Estimated batches left to process: ${Math.ceil(estimatedBatchesLeft)}`);
        log(`Estimated time left to clear backlog: ${estimatedTimeLeft.toFixed(2)} minutes`);

        log('CBI_ASSET_ADDRESSES_CACHE - Delta Update - COMPLETE.');
    } catch (err) {
        log('Error calling procedure: ' + err);
    } finally {
        await clientCBI.end(); // Close the connection
        releaseLock();
    }
};

clientCBI.connect().then(() => {
    callProcedure();
}).catch(err => {
    log('Failed to connect to the database: ' + err);
});
