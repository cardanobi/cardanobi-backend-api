// const shell = require('shelljs');
import shell from 'shelljs';
import Client from 'pg/lib/client.js';
import * as dotenv from 'dotenv';

dotenv.config();

const client = new Client({
    host: '127.0.0.1',
    user: process.env.CARDANOBI_ADMIN_USERNAME,
    database: 'cardanobi',
    password: process.env.CARDANOBI_ADMIN_PASSWORD,
    port: 5432,
});

async function runShellCmd(cmd) {
  return new Promise((resolve, reject) => {
    shell.exec(cmd, async (code, stdout, stderr) => {
      if (!code) {
        return resolve(stdout);
      }
      return reject(stderr);
    });
  });
}

function runShellCmdSync(cmd) {
    // return shell.exec(cmd, { timeout: 10000 }).stdout;
    // return shell.exec(cmd).stdout;

    // shell.exec(cmd, { shell: '/bin/bash' }, function(code, stdout, stderr) {
    //     // console.log('Exit code:', code);
    //     // console.log('Program output:', stdout);
    //     // console.log('Program stderr:', stderr);
    //     return stdout;
    //   });
    var output = shell.exec(cmd, { silent: false }).stdout;
    return output;
}

// { shell: '/bin/bash' }

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

const getPoolsToProcess = async () => {
    try {
        // var query = `select distinct encode(pu.vrf_key_hash::bytea, 'hex') as vrf_key_hash, ph."view" as pool_id
        //     from pool_update pu 
        //     inner join pool_hash ph on ph.id = pu.hash_id
        //     where not exists (select * from "_cbi_pool_params" cpp where cpp.vrf_key = encode(pu.vrf_key_hash::bytea, 'hex'))`;
        
        var query = `select distinct encode(pu.vrf_key_hash::bytea, 'hex') as vrf_key_hash, ph."view" as pool_id
            from pool_update pu 
            inner join pool_hash ph on ph.id = pu.hash_id
            where not exists (select * from "_cbi_pool_params" cpp where cpp.vrf_key = encode(pu.vrf_key_hash::bytea, 'hex'))
            and not exists (select * 
                            from pool_retire pr 
                            where pr.hash_id=ph.id and pr.retiring_epoch<(select max(id) from epoch)) limit 80;`;
        
        console.log(query);

        var result = await client.query(query);
        return result.rows;
    } catch (error) {
        console.error(error.stack);
        return false;
    }
};

const readPools = async () => {
    try {
        const query = `SELECT * from _cbi_pool_params;`;
        // const query = `SELECT * from _nginx_logs_staging WHERE hash = ('7e8337fff80d9092010646b5035a32f0c4f3ad343a5eaa33cc07f2c5759f6a1d');`;
        
        // console.log(query);

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


let magic = 1;

var pools = await getPoolsToProcess();
let poolCount = pools.length;
if (poolCount == 0) {
    console.log("No new pools to add to _cbi_pool_params. Bye for now...");
} else {
    let i = 1;
    pools.forEach(p => {
        console.log(`Processing pool ${i}/${poolCount}`);
        let cmd = `cardano-cli query pool-params --testnet-magic ${magic} --stake-pool-id ${p.pool_id} | jq -r '[.poolParams.publicKey, .poolParams.vrf] | @csv'`;

        console.log(cmd);

        let res = runShellCmdSync(cmd);
        let result = res.replaceAll("\n","").replaceAll("\"","").split(",")

        let pool = { pool_id: p.pool_id, vrf_key: p.vrf_key_hash, cold_vkey: result[0]};
        insertPoolParams(pool);

        i++;
    });
}

console.log("Done!");
// await client.end();