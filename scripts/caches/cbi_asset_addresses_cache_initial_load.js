// to daemonize this script using pm2
// pm2 start /home/cardano/data/cardanobi-backend-api/scripts/caches/cbi_asset_addresses_cache_initial_load.js --name "cbi_asset_addresses_cache_initial_load" --cwd /home/cardano/data/cardanobi-backend-api/scripts/caches/
// pm2 save
// pm2 stop cbi_asset_addresses_cache_initial_load

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

const stateFilePath = process.env.STATE_FILE_PATH || 'state.txt';
const logFilePath = process.env.LOG_FILE_PATH || 'log.txt';
const totalRecords = parseInt(process.env.LAST_MUTLI_ASSET_ID, 10);;
const startingRecord = parseInt(process.env.FIRST_MUTLI_ASSET_ID, 10);;
const batchSize = 20000;

const log = (message) => {
    console.log(message);
    fs.appendFileSync(logFilePath, message + '\n');
};

const processBatch = async () => {
    const startTime = Date.now();

    try {
        let currentLowerBound;
        try {
            currentLowerBound = parseInt(fs.readFileSync(stateFilePath, 'utf8'), 10);
        } catch (readError) {
            if (readError.code === 'ENOENT') { // File does not exist
                currentLowerBound = startingRecord;
                log(`State file not found. Starting from default lower bound: ${currentLowerBound}`);
            } else {
                throw readError; // Rethrow error if it's not 'ENOENT'
            }
        }

        const upperBound = currentLowerBound + batchSize;
        if (currentLowerBound > totalRecords) {
            log('\nProcessing complete.');
            return;
        }

        log(`\nProcessing batch: ${currentLowerBound} - ${upperBound}`);

        const query = `
            INSERT INTO _cbi_asset_addresses_cache (asset_id, address, quantity)
            SELECT
                ma.id AS asset_id,
                txo.address,
                SUM(mto.quantity) AS quantity
            FROM
                ma_tx_out mto
                INNER JOIN multi_asset ma ON ma.id = mto.ident
                INNER JOIN tx_out txo ON txo.id = mto.tx_out_id
                LEFT JOIN tx_in ON txo.tx_id = tx_in.tx_out_id
                    AND txo.index::smallint = tx_in.tx_out_index::smallint
            WHERE
                tx_in.tx_out_id IS NULL
                AND txo.tx_id < 84664110
                AND ma.id >= ${currentLowerBound} AND ma.id < ${upperBound}
            GROUP BY
                ma.id, txo.address;
        `;
        
        // const query = `select max(no) from epoch;`;

        await clientCBI.query(query);

        const duration = (Date.now() - startTime) / 60000; // Duration in minutes
        log(`Batch ${currentLowerBound} - ${upperBound} processed in ${duration.toFixed(2)} minutes`);

        // Log number of rows in the _cbi_asset_addresses_cache table
        const countResult = await clientCBI.query('SELECT COUNT(*) FROM _cbi_asset_addresses_cache');
        const count = countResult.rows[0].count;
        log(`Total rows in _cbi_asset_addresses_cache: ${count}`);

        // Estimate time left
        const batchesLeft = Math.ceil((totalRecords - upperBound) / batchSize);
        const estimatedTimeLeft = duration * batchesLeft;
        log(`Estimated time left: ${estimatedTimeLeft.toFixed(2)} minutes`);

        // Update the state file with the new lower bound
        fs.writeFileSync(stateFilePath, `${upperBound}`);
        currentLowerBound = upperBound;

        setTimeout(processBatch, 1000); // Wait for 1 second before starting the next batch
    } catch (err) {
        log('Error processing batch: ' + err);
        await clientCBI.end(); // Close the connection in case of an error
    }
};

// Connect once and start processing
clientCBI.connect().then(() => {
    processBatch();
}).catch(err => {
    log('Failed to connect to the database: ' + err);
});
