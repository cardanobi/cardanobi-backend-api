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


const logFilePath = process.env.CBI_EPOCH_BOUNDARY_UPDATE_LOG_FILE_PATH || 'cbi_epoch_boundary_update_process.log';

const log = (message, ...additionalArgs) => {
    const timestamp = new Date().toISOString().replace(/T/, ' ').replace(/\..+/, '');

    // Check if the message is an object and stringify it if so
    const formattedMessage = typeof message === 'object' ? JSON.stringify(message, null, 2) : message;

    // Process additional arguments if they exist, turning objects into strings
    const additionalMessages = additionalArgs.map(arg =>
        typeof arg === 'object' ? JSON.stringify(arg, null, 2) : arg
    ).join(' ');

    // Combine the primary message with any additional messages
    const logMessage = `${timestamp} - ${formattedMessage} ${additionalMessages}`;
    console.log(logMessage);
    fs.appendFileSync(logFilePath, logMessage + '\n');
};


// Constants for epoch lengths in seconds
const EPOCH_LENGTHS = {
    mainnet: parseInt(process.env.MAINNET_EPOCH_LENGTH),
    preprod: parseInt(process.env.PREPROD_EPOCH_LENGTH),
    preview: parseInt(process.env.PREVIEW_EPOCH_LENGTH),
};

// Constants for genesis epoch start times (as UNIX timestamps)
const GENESIS_START_TIMES = {
    mainnet: parseInt(process.env.MAINNET_START_TIME),
    preprod: parseInt(process.env.PREPROD_START_TIME),
    preview: parseInt(process.env.PREVIEW_START_TIME),
};

/**
 * Calculate seconds until the end of the current Cardano epoch.
 * @param {string} environment - The Cardano environment ('mainnet', 'preprod', 'preview').
 * @returns {number} Seconds until the end of the current epoch.
 */
function secondsUntilEpochEnd(environment) {
    const now = Math.floor(Date.now() / 1000); // Current time in seconds

    const genesisStartTime = GENESIS_START_TIMES[environment];
    const epochLength = EPOCH_LENGTHS[environment];

    if (!genesisStartTime || !epochLength) {
        throw new Error("Invalid environment specified.");
    }

    const timeSinceGenesis = now - genesisStartTime;
    const currentEpoch = Math.floor(timeSinceGenesis / epochLength);
    const nextEpochStartTime = genesisStartTime + ((currentEpoch + 1) * epochLength);
    const secondsUntilNextEpoch = nextEpochStartTime - now;

    // Convert nextEpochStartTime to YYYY/MM/DD HH:MM:SS format
    const startTimeNextEpoch = new Date(nextEpochStartTime * 1000).toISOString().replace(/T/, ' ').replace(/\..+/, '').replace(/-/g, '/');

    return {
        secondsUntilNextEpoch: secondsUntilNextEpoch,
        startTimeNextEpoch: startTimeNextEpoch
    };
}


// Example usage
const environment = 'mainnet';

// console.log(EPOCH_LENGTHS);
// console.log(GENESIS_START_TIMES);
// log(secondsUntilEpochEnd(environment));


const processEpochBoundary = async () => {
    try {
        log("EPOCH BOUNDARY UPDATE - START");

        // Get the current epoch number
        const currentEpochResult = await clientCBI.query("select max(no) as no from epoch;");
        const currentEpochNo = currentEpochResult.rows[0].no;
        const nextEpochNo = currentEpochNo + 1;
        log(`Current epoch number is ${currentEpochNo}`);

        // Wait for the new epoch to start
        let nextEpochInfo = secondsUntilEpochEnd(environment);
        log(`Next epoch start time: ${nextEpochInfo.startTimeNextEpoch}`);
        log(`Waiting for the next epoch (#${nextEpochNo}) to start in ${nextEpochInfo.secondsUntilNextEpoch} seconds`);

        // 1.1 Wait for the new epoch to start
        await new Promise(resolve => setTimeout(resolve, nextEpochInfo.secondsUntilNextEpoch * 1000));
        let previousEpochCount = 0;
        let newEpochCount = 0;
        let isDataComplete = false;

        // 1.2 Safety step: Check if the new epoch has been created in the `epoch` table
        let newEpochCreated = false;
        do {
            const checkNewEpochResult = await clientCBI.query("select max(no) as no from epoch;");
            const newEpochNo = checkNewEpochResult.rows[0].no;

            if (newEpochNo === nextEpochNo) {
                newEpochCreated = true;
                log(`New epoch ${newEpochNo} has started.`);
            } else {
                log(`Waiting for the new epoch to be reflected in the database.`);
                await new Promise(resolve => setTimeout(resolve, 10 * 60 * 1000)); // Wait for 10 minutes
            }
        } while (!newEpochCreated);

        // 1.3 Check for the completion of epoch boundary processing
        // Compute the row count for the previous epoch once, before the loop
        const previousEpochResult = await clientCBI.query(`select count(*) from epoch_stake where epoch_no=${currentEpochNo}`);
        previousEpochCount = parseInt(previousEpochResult.rows[0].count);

        let stabilityTime = 0; // Track stability duration
        let lastNewEpochCount = -1; // Initialize to -1 to ensure it's different in the first iteration

        do {
            // Count rows for the new epoch
            const newEpochResult = await clientCBI.query(`select count(*) from epoch_stake where epoch_no=${currentEpochNo + 1}`);
            const newEpochCount = parseInt(newEpochResult.rows[0].count);

            log(`EPOCH_STAKE row count for previous epoch: ${previousEpochCount}, for new epoch: ${newEpochCount}`);
            log(`isDataComplete: ${isDataComplete}, stabilityTime: ${stabilityTime}`);

            if (newEpochCount > previousEpochCount) {
                isDataComplete = true;
            } else if (newEpochCount === lastNewEpochCount) {
                // If the row count has not changed, increment the stability time
                stabilityTime += 10;
            } else {
                // If the row count has changed, reset the stability time counter
                stabilityTime = 0;
            }

            lastNewEpochCount = newEpochCount; // Update lastNewEpochCount for the next iteration

            // Check if the data has been stable for 20 minutes
            if (stabilityTime >= 20) {
                isDataComplete = true;
            } else if (!isDataComplete) {
                log("Waiting 10 more minutes for epoch boundary data to stabilize.");
                await new Promise(resolve => setTimeout(resolve, 10 * 60 * 1000)); // Wait for 10 minutes
            }
        } while (!isDataComplete);


        // 1.4 Ensure that the epoch_stake table's row count for the current epoch has remained constant for 10 minutes
        let lastRowCount = 0;
        let rowCount = 0;
        let rowCountStableFor = 0; // Track how long the row count has been stable
        do {
            const rowCountResult = await clientCBI.query(`select count(*) from epoch_stake where epoch_no=${nextEpochNo}`);
            rowCount = parseInt(rowCountResult.rows[0].count);

            if (rowCount === lastRowCount) {
                // Row count has not changed, increment the stable time counter
                rowCountStableFor += 10; // We check every 10 minutes
                log(`EPOCH_STAKE row count for epoch ${nextEpochNo} has been stable for ${rowCountStableFor} minutes.`);
            } else {
                // Row count has changed, reset the stable time counter
                rowCountStableFor = 0;
            }

            lastRowCount = rowCount;

            // Wait for 10 minutes before the next check if row count has not been stable for at least 10 minutes
            if (rowCountStableFor < 10) {
                log("Waiting 10 minutes before the next row count check.");
                await new Promise(resolve => setTimeout(resolve, 10 * 60 * 1000)); // Wait for 10 minutes
            }
        } while (rowCountStableFor < 10);

        const startTime = Date.now();

        // Begin execution of sequence of queries
        log("Executing cbi_active_stake_cache_update");
        const activeStakeStartTime = Date.now();
        await clientCBI.query("call public.cbi_active_stake_cache_update();");
        const activeStakeEndTime = Date.now();
        const activeStakeTimeTaken = (activeStakeEndTime - activeStakeStartTime) / 60000; // Convert time taken to minutes
        log(`Finished executing cbi_active_stake_cache_update. Time taken: ${activeStakeTimeTaken.toFixed(2)} minutes`);

        log("Executing cbi_stake_distribution_cache_update");
        const stakeDistStartTime = Date.now();
        await clientCBI.query("call public.cbi_stake_distribution_cache_update();");
        const stakeDistEndTime = Date.now();
        const stakeDistTimeTaken = (stakeDistEndTime - stakeDistStartTime) / 60000; // Convert time taken to minutes
        log(`Finished executing cbi_stake_distribution_cache_update. Time taken: ${stakeDistTimeTaken.toFixed(2)} minutes`);

        log("Executing cbi_pool_stats_cache_update");
        const poolStatsStartTime = Date.now();
        await clientCBI.query("call public.cbi_pool_stats_cache_update();");
        const poolStatsEndTime = Date.now();
        const poolStatsTimeTaken = (poolStatsEndTime - poolStatsStartTime) / 60000; // Convert time taken to minutes
        log(`Finished executing cbi_pool_stats_cache_update. Time taken: ${poolStatsTimeTaken.toFixed(2)} minutes`);

        // End of sequence of queries
        const endTime = Date.now();
        const totalSequenceTimeTaken = (endTime - startTime) / 60000; // Convert total time taken to minutes
        log(`Finished sequence of EPOCH BOUNDARY queries. Total time taken: ${totalSequenceTimeTaken.toFixed(2)} minutes`);


        log("EPOCH BOUNDARY UPDATE - END");

        log("Sleeping for 60 seconds.");
        setTimeout(processEpochBoundary, 60000); // Go for a sleep until next run
    } catch (err) {
        log('Error processing batch: ' + err);
        await clientCBI.end(); // Close the connection in case of an error
    }
};


// Connect once and start processing
clientCBI.connect().then(() => {
    processEpochBoundary();
}).catch(err => {
    log("Failed to connect to the database: ", err);
});


// to setup this script to run every 5 minutes using pm2
// pm2 start /home/cardano/data/cardanobi-backend-api/scripts/caches/cbi_epoch_boundary_update.js --name "cbi_epoch_boundary_update" --cwd /home/cardano/data/cardanobi-backend-api/scripts/caches

// pm2 tips:

// monitor execution
// pm2 list
// pm2 show cbi_epoch_boundary_update
// more /home/cardano/.pm2/logs/cbi_epoch_boundary_update-out.log

// restart following a change in the script
// pm2 stop cbi_epoch_boundary_update
// pm2 restart cbi_epoch_boundary_update


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
// pm2 logs cbi_epoch_boundary_update
// pm2 flush

// select *
// from _cbi_cache_handler_state
// where table_name in ('_cbi_address_info_cache',
// '_cbi_address_stats_cache',
// '_cbi_asset_cache');

