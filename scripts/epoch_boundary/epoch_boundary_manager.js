import Client from "pg/lib/client.js";
import * as dotenv from "dotenv";
import fs from "fs";
import child_process from "child_process";
import moment from "moment";


dotenv.config();

const boundaryProcessingDelayMs = process.env.CARDANOBI_EPOCH_BOUNDARY_MANAGER_DELAY_INTERVAL_MS || 60000;
const logFilePath =  process.env.CARDANOBI_EPOCH_BOUNDARY_MANAGER_LOG_FILE || "log.txt";
const startTime =  parseInt(process.env.CARDANOBI_NODE_CONFIG_START_TIME) || 1506203091;
const epochLength =  parseInt(process.env.CARDANOBI_NODE_CONFIG_EPOCH_LENGTH) || 432000;

const client_cbi = new Client({
    host: "127.0.0.1",
    user: process.env.CARDANOBI_ADMIN_USERNAME,
    database: "cardanobi",
    password: process.env.CARDANOBI_ADMIN_PASSWORD,
    port: 5432,
});
  
function logToFile(logText, data) {
    var logMessage = "";
  
    console.log(logText);
  
    if (data)
      logMessage = `${new Date().toISOString()} - ${logText}\n${JSON.stringify(
        data,
        null,
        2
      )}\n\n`;
    else logMessage = `${new Date().toISOString()} - ${logText}\n`;
  
    fs.appendFile(logFilePath, logMessage, "utf8", (error) => {
      if (error) {
        console.error("Error writing to log file:", error);
      }
    });
}

function getEpochParams() {
    let params = {};

    // Get the current time in Unix timestamp format (seconds since 1970-01-01 00:00:00 UTC).
    let current_time = moment().unix();
    let curr_time = moment.unix(current_time).format();

    // Calculate the number of epochs that have passed since the start of the Cardano epochs system.
    let epochs_passed = (current_time - startTime) / epochLength;
    let current_epoch = Math.floor(epochs_passed);
    let current_epoch_progress_pct = (epochs_passed - current_epoch) * 100;

    // Calculate the next epoch number by taking the ceiling of the number of epochs passed.
    let next_epoch = Math.ceil(epochs_passed);

    // Calculate the Unix timestamp for the start of the current & next epoch.
    let current_epoch_start = current_epoch * epochLength + startTime;
    let next_epoch_start = next_epoch * epochLength + startTime;

    // Convert this Unix timestamp back to a regular date/time.
    let next_epoch_start_time = moment.unix(next_epoch_start).format();

    params = {
      currentTime: current_time,
      currentTimeTs: curr_time,
      currentEpoch: {
        no: current_epoch,
        startTime: current_epoch_start,
        startTimeTs: moment.unix(current_epoch_start).format(),
        completionPct: current_epoch_progress_pct
      },
      nextEpoch: {
        no: next_epoch,
        startTime: next_epoch_start,
        startTimeTs: moment.unix(next_epoch_start).format()
      }
    };

  // console.log("getEpochParams: ", params);
  return params;
}

const epochTransitionQueryStep1 = `call public.cbi_active_stake_cache_update();`;
const epochTransitionQueryStep2 = `call public.cbi_stake_distribution_cache_update();`;
const epochTransitionQueryStep3 = `call public.cbi_asset_cache_update();`;
const epochTransitionQueryStep4 = `call public.cbi_asset_addresses_cache_update();`;
const epochTransitionQueryStep5 = `insert into _cbi_pool_params (pool_id, cold_vkey, vrf_key)
select ph.view as pool_id,
	encode(ph.hash_raw::bytea, 'hex') as cold_vkey,
encode(pu.vrf_key_hash::bytea, 'hex') as vrf_key 
from pool_hash ph
inner join pool_update pu on pu.hash_id = ph.id 
where pu.id=(select max(pu2.id) from pool_update pu2 where pu2.hash_id = ph.id)
and not exists (select * from "_cbi_pool_params" cpp where cpp.pool_id=ph.view)
and not exists (select * from pool_retire pr where pr.hash_id=ph.id and pr.retiring_epoch<(select max(no) from epoch));
`;
const epochTransitionQueryStep6 = `insert into _cbi_pool_stats(epoch_no, pool_hash_id,delegator_count,delegated_stakes)
select casca.epoch_no, ph.id, count(1) as delegators_count, coalesce(sum(casca.amount), 0) as delegated_stakes
from _cbi_active_stake_cache_account casca
inner join pool_hash ph on ph.view = casca.pool_id 
where casca.epoch_no = (select max(no) from epoch)
group by casca.epoch_no, ph.id;`;
  
async function processEpochTransition(client_cbi) {
  try {
    let now = moment().unix();
    const processResult1 = await client_cbi.query(epochTransitionQueryStep1);
    logToFile("cbi_active_stake_cache_update, completed, elapse time (m):", [(moment().unix() - now) / 60]);
    
    now = moment().unix();
    const processResult2 = await client_cbi.query(epochTransitionQueryStep2);
    logToFile("cbi_stake_distribution_cache_update, completed, elapse time (m):", [(moment().unix() - now) / 60]);

    now = moment().unix();
    const processResult3 = await client_cbi.query(epochTransitionQueryStep3);
    logToFile("cbi_asset_cache_update, completed, elapse time (m):", [(moment().unix() - now) / 60]);

    now = moment().unix();
    const processResult4 = await client_cbi.query(epochTransitionQueryStep4);
    logToFile("cbi_asset_addresses_cache_update, completed, elapse time (m):", [(moment().unix() - now) / 60]);

    now = moment().unix();
    const processResult5 = await client_cbi.query(epochTransitionQueryStep5);
    logToFile("_cbi_pool_params, completed, elapse time (m):", [(moment().unix() - now) / 60]);

    now = moment().unix();
    const processResult6 = await client_cbi.query(epochTransitionQueryStep6);
    logToFile("_cbi_pool_stats, completed, elapse time (m):", [(moment().unix() - now) / 60]);

    return true
  } catch (error) {
    console.error("Error processing epoch transition:", error);
    logToFile("processEpochTransition - error:", [error]);
  }

  return false;
}
  
async function main() {
  let initEpochParams = getEpochParams();
  logToFile("Epoch Boundary Manager - startup, initEpochParams: ", [initEpochParams]);

    while (true) {
      let lastEpochParams = getEpochParams();

      logToFile("Epoch Boundary Manager - loop check, lastEpochParams: ", [lastEpochParams]);

      lastEpochParams.currentEpoch.no = 84;

      if (initEpochParams.currentEpoch.no == lastEpochParams.currentEpoch.no) {
        //still in the same epoch, let's compute the time until next epoch and wait
        let timeUntilNextEpochStarts = lastEpochParams.nextEpoch.startTime - lastEpochParams.currentTime;
        logToFile("Waiting for next epoch transition in (seconds):", [timeUntilNextEpochStarts]);
        await new Promise((resolve) => setTimeout(resolve, timeUntilNextEpochStarts*1000)); 
      } else {
        //new epoch started, let's wait 3h until epoch_stake is ready
        logToFile("New epoch started - [Prev, New]: ", [lastEpochParams]);
        logToFile("Delaying processing of boundary by (ms): ", [boundaryProcessingDelayMs]);

        // await new Promise((resolve) => setTimeout(resolve, boundaryProcessingDelayMs)); 
        await new Promise((resolve) => setTimeout(resolve, 5000)); 

        logToFile("Processing epoch transition - START");

        try {
          await client_cbi.connect();
          logToFile("Connected to PostgreSQL database.");
      
          await processEpochTransition(client_cbi);
        } catch (error) {
          logToFile("Error connecting to the PostgreSQL database:", error);
        } finally {
          await client_cbi.end();
          logToFile("Disconnected from the PostgreSQL database.");
        }

        logToFile("Processing epoch transition - END");

        initEpochParams = lastEpochParams;
      }
    }
}
  
main();