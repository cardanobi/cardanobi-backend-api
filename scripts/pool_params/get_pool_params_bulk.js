// const shell = require('shelljs');
import shell from 'shelljs';
import Client from 'pg/lib/client.js';
import * as dotenv from 'dotenv';
import fs from 'fs';

dotenv.config();

const client = new Client({
    host: '127.0.0.1',
    user: process.env.CARDANOBI_ADMIN_USERNAME,
    database: 'cardanobi',
    password: process.env.CARDANOBI_ADMIN_PASSWORD,
    port: 5432,
});

function runShellCmdSync(cmd) {
    var output = shell.exec(cmd, { silent: false }).stdout;
    return output;
}

const createTable = async () => {
    try {
        await client.query(
            `CREATE TABLE IF NOT EXISTS "_cbi_pool_params" (
                "id" SERIAL PRIMARY KEY,
	            "pool_id" VARCHAR(64) NOT NULL UNIQUE,
	            "cold_vkey" VARCHAR(64) NOT NULL,
	            "vrf_key" VARCHAR(64) NOT NULL
            );`);
        return true;
    } catch (error) {
        console.error("createLogTable error:",error.stack);
        return false;
    }
};

const checkTableExists = async (tableName) => {
    try {
        var result = await client.query(
            `SELECT COUNT(1) FROM 
                pg_tables
             WHERE 
             schemaname = 'public' AND 
             tablename  = $1;`, [tableName]);
        return result.rows;
    } catch (error) {
        console.error("checkTableExists error:",error.stack);
        return false;
    }
};

const insertPoolParams = (pool) => {
    try {
        let query = `INSERT INTO "_cbi_pool_params" ("pool_id", "cold_vkey", "vrf_key") 
        VALUES ('${pool.pool_id}',
            '${pool.cold_vkey}', 
            '${pool.vrf_key}')`;
    
        // console.log(query);

        client.query(query, (err, res) => {
            if (err) console.log("insertLog error: ",err);
            // if (res) console.log("insertLog res: ",res);
        });
        return true;

        // return new Promise(function (resolve, reject) {
        //     client.query(query, (err, res) => {
        //         if (err) {
        //             console.log("insertLog error: ", err);
        //             return reject(err);
        //         } else {
        //             if (res.rowCount > 0) {
        //                 console.log("insertLog, ok: ", res);
        //                 return resolve(true);
        //             } 
        //             return resolve(false);
        //         }
        //     });
        // });
    } catch (error) {
        console.error(error.stack);
        return false;
    }
};

const getPoolId = async (vrfKey) => {
    try {      
        var query = `select distinct encode(pu.vrf_key_hash::bytea, 'hex') as vrf, ph."view" as pool_id
        from pool_update pu 
        inner join pool_hash ph on ph.id = pu.hash_id
        where not exists (select * from "_cbi_pool_params" cpp where cpp.vrf_key = encode(pu.vrf_key_hash::bytea, 'hex'))
        and pu.vrf_key_hash='\\x${vrfKey}';`;
        
        console.log(query);

        var result = await client.query(query);
        if (result.rows.length>0)
            return result.rows[0].pool_id;
        return undefined
    } catch (error) {
        console.error(error.stack);
        return false;
    }
};

const readPools = async () => {
    try {
        const query = `SELECT * from _cbi_pool_params;`;
        var result = await client.query(query);
        return result.rows;
    } catch (error) {
        console.error(error.stack);
        return false;
    }
};

// open connection 
await client.connect();

// create log table if required
var tableExists = await checkTableExists("_cbi_pool_params");
if (tableExists[0].count == 0) {
    await createTable().then(result => {
        if (result) {
            console.log("Table created");
        } else {
            console.log("Table not created");
        }
    });
} else {
    console.log("Table already exists!");
}

// Note: to prepare the pool_params.json file please run the following
// cardano-cli query ledger-state --mainnet > mydump.json
// cat mydump.json | jq '.stateBefore.esLState.delegationState.pstate."pParams pState"' >pool_params.json

let magic = 1;
var poolParamsFile = "/home/cardano/pool_params.json"
let poolParams = fs.readFileSync(poolParamsFile);
let ppJson = JSON.parse(poolParams);
let coldKeys = Object.keys(ppJson);
let poolCount = coldKeys.length;
// let coldVkey = "";
let vrfKkey = "";
let poolId = "";
let i = 1;

coldKeys.forEach(async key => {
    poolId = await getPoolId(ppJson[key].vrf);

    if (poolId != undefined) {
        console.log(`Processing pool ${i}/${poolCount}: ${poolId}`);
        let pool = { pool_id: poolId, vrf_key: ppJson[key].vrf, cold_vkey: key};
        insertPoolParams(pool);
    } else {
        console.log(`Already processed or unknown vrf: ${ppJson[key].vrf}`);
    }
    i++;
});

console.log("Done!");
// await client.end();