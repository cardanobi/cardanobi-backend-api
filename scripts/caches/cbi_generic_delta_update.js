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

// Listen for notice events from the PostgreSQL server
clientCBI.on('notice', (notice) => {
    const prefixes = [
        'cbi_address_info_cache_update', // Name of the first function
        'cbi_address_stats_cache_update', // Name of the second function
        'cbi_asset_cache_update' // Name of the third function
    ];

    // Check if the notice message starts with any of the specified prefixes
    const relevantNotice = prefixes.some(prefix => notice.message.startsWith(prefix));

    if (relevantNotice) {
        log(`PSQL Notice: ${notice.message}`);
    }
});


clientCBI.on("readyForQuery", () => {
    log("Client is connected and ready for queries.");
});

clientCBI.on("end", () => {
    log("Client is disconnected from PostgreSQL server.");
});

clientCBI.on("error", (err) => {
    log("An error occurred:", err);
});


const logFilePath = process.env.CBI_GENERIC_DELTA_UPDATE_LOG_FILE_PATH || 'cbi_generic_delta_update_process.log';

const log = (message, ...additionalArgs) => {
    const timestamp = new Date().toISOString().replace(/T/, ' ').replace(/\..+/, '');
    // Process additional arguments if they exist, turning objects into strings
    const additionalMessages = additionalArgs.map(arg =>
        typeof arg === 'object' ? JSON.stringify(arg) : arg
    ).join(' ');

    // Combine the primary message with any additional messages
    const logMessage = `${timestamp} - ${message} ${additionalMessages}`;
    console.log(logMessage);
    fs.appendFileSync(logFilePath, logMessage + '\n');
};

const processDeltaUpdates = async () => {
    const startTime = Date.now();

    try {
        log("DELTA UPDATE - START");

        // Begin execution of sequence of queries
        log("Starting sequence of PSQL queries");

        // 1. Call public.cbi_address_info_cache_update();
        log("Executing cbi_address_info_cache_update");
        await clientCBI.query("call public.cbi_address_info_cache_update();");
        log("Finished executing cbi_address_info_cache_update");

        // 2. Call public.cbi_address_stats_cache_update();
        log("Executing cbi_address_stats_cache_update");
        await clientCBI.query("call public.cbi_address_stats_cache_update();");
        log("Finished executing cbi_address_stats_cache_update");

        // 3. Call public.cbi_asset_cache_update();
        log("Executing cbi_asset_cache_update");
        await clientCBI.query("call public.cbi_asset_cache_update();");
        log("Finished executing cbi_asset_cache_update");

        // 4. Insert into _cbi_pool_params
        log("Executing insert into _cbi_pool_params");
        await clientCBI.query(`
            insert into _cbi_pool_params (pool_id, cold_vkey, vrf_key)
            select ph.view as pool_id,
                encode(ph.hash_raw::bytea, 'hex') as cold_vkey,
                encode(pu.vrf_key_hash::bytea, 'hex') as vrf_key 
            from pool_hash ph
            inner join pool_update pu on pu.hash_id = ph.id 
            where pu.id=(select max(pu2.id) from pool_update pu2 where pu2.hash_id = ph.id)
            and not exists (select * from "_cbi_pool_params" cpp where cpp.pool_id=ph.view)
            and not exists (select * from pool_retire pr where pr.hash_id=ph.id and pr.retiring_epoch<(select max(no) from epoch));
        `);
        log("Finished executing insert into _cbi_pool_params");

        // End of sequence of queries
        const endTime = Date.now();
        const timeTaken = (endTime - startTime) / 1000; // Convert time taken to minutes
        log(`Finished sequence of DELTA queries. Time taken: ${timeTaken.toFixed(2)} seconds`);

        log("DELTA UPDATE - END");
        let sleepSeconds = parseInt(process.env.CBI_GENERIC_DELTA_UPDATE_FREQENCY_SECONDS);
        log("Sleeping for " + sleepSeconds + " seconds.");
        setTimeout(processDeltaUpdates, sleepSeconds*1000); // go for a sleep until next run
    } catch (err) {
        log('Error processing batch: ' + err);
        await clientCBI.end(); // Close the connection in case of an error
    }
};

// Connect once and start processing
clientCBI.connect().then(() => {
    processDeltaUpdates();
}).catch(err => {
    log("Failed to connect to the database: ", err);
});


// to setup this script to run every 5 minutes using pm2
// pm2 start /home/cardano/data/cardanobi-backend-api/scripts/caches/cbi_generic_delta_update.js --name "cbi_generic_delta_update" --cwd /home/cardano/data/cardanobi-backend-api/scripts/caches

// pm2 tips:

// monitor execution
// pm2 list
// pm2 show cbi_generic_delta_update
// more /home/cardano/.pm2/logs/cbi_generic_delta_update-out.log

// restart following a change in the script
// pm2 stop cbi_generic_delta_update
// pm2 restart cbi_generic_delta_update


// [PM2] Freeze a process list on reboot via:
// $ pm2 save
// $ pm2 startup

// [PM2] Remove init script via:
// $ pm2 unstartup systemd


// To unshedule a pm2 App
// pm2 stop myApp
// pm2 list
// pm2 delete myApp
// pm2 save

// Logs
// pm2 logs cbi_generic_delta_update
// pm2 flush

// select *
// from _cbi_cache_handler_state
// where table_name in ('_cbi_address_info_cache',
// '_cbi_address_stats_cache',
// '_cbi_asset_cache');

