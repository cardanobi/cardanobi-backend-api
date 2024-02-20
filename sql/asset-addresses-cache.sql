create or replace procedure public.cbi_asset_addresses_cache_update(_batch_tx_count bigint default null)
 language plpgsql
as $$
declare
  _last_tx_id bigint;
  _handler_last_tx_id bigint;
  _tx_id_list bigint[];
  _changed_assets_count int;
  _recalculated_balances_count int;
	begin
		--avoid concurrent runs
		if (
		    select
		      count(pid) > 1
		    from
		      pg_stat_activity
		    where
		      state = 'active' and query ilike '%public.cbi_asset_addresses_cache_update%'
		      and datname = (select current_database())
		  ) then 
		    raise exception 'cbi_asset_addresses_cache_update already running, exiting!';
		end if;

		-- Drop temporary tables if they exist from a previous execution
		BEGIN
			EXECUTE 'DROP TABLE IF EXISTS temp_changes_assets, temp_recalculated_balances;';
		EXCEPTION
			WHEN OTHERS THEN
				-- Ignore errors in case tables do not exist
		END;

		select coalesce(last_tx_id, 0) into _handler_last_tx_id from _cbi_cache_handler_state where table_name = '_cbi_asset_addresses_cache';

		-- Determine the last tx id based on the passed parameter
		if _batch_tx_count is not null and _batch_tx_count > 0 then
			_last_tx_id := _handler_last_tx_id + _batch_tx_count;
		else
			select max(id) into _last_tx_id from tx;
		end if;
		
		raise notice 'cbi_asset_addresses_cache_update - info - _batch_tx_count: %, _last_tx_id: %, _handler_last_tx_id: %', _batch_tx_count, _last_tx_id, _handler_last_tx_id;
	
		if _handler_last_tx_id is null then
			truncate table _cbi_asset_addresses_cache;
            select 0 into _handler_last_tx_id;
			raise notice 'cbi_asset_addresses_cache_update - building cache from scratch, _handler_last_tx_id: %...', _handler_last_tx_id;
		else
			select array_agg(distinct id) into _tx_id_list from tx where id > _handler_last_tx_id and id <= _last_tx_id;
			if _tx_id_list is null then
				raise notice 'cbi_asset_addresses_cache_update - no new multi-asset transactions to process, exiting!';
				return;
			else
				raise notice 'cbi_asset_addresses_cache_update - updating cache with % new multi-asset transactions.', array_length(_tx_id_list, 1);
			end if;
		end if;
	
	
		if _handler_last_tx_id = 0 then
			--building cache from scratch (use cbi_asset_addresses_cache_initial_load process for mainnet as unpartitioned calcs would take too long)
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
				AND txo.tx_id <= _last_tx_id
			GROUP BY
				ma.id, txo.address;

		else
			--updating the cache by recomputing the (ma.id) that got new unspent utxos
			CREATE TEMP TABLE temp_changes_assets AS
			SELECT DISTINCT
				ma.id AS asset_id
			FROM
				ma_tx_out mto
				INNER JOIN multi_asset ma ON ma.id = mto.ident
				INNER JOIN tx_out txo ON txo.id = mto.tx_out_id
				left join tx_in on txo.tx_id = tx_in.tx_out_id
				and txo.index::smallint = tx_in.tx_out_index::smallint
			WHERE
				tx_in.tx_out_id is null
				AND txo.tx_id > _handler_last_tx_id
				AND txo.tx_id <= _last_tx_id;

			CREATE TEMP TABLE temp_recalculated_balances AS
			SELECT
				ca.asset_id,
				txo.address,
				COALESCE(SUM(mto.quantity), 0) AS new_quantity
			FROM
				temp_changes_assets ca
				INNER JOIN ma_tx_out mto ON ca.asset_id = mto.ident
				INNER JOIN tx_out txo ON mto.tx_out_id = txo.id
				LEFT JOIN tx_in ON txo.tx_id = tx_in.tx_out_id AND txo.index::smallint = tx_in.tx_out_index::smallint
			WHERE
				tx_in.tx_out_id IS NULL
				AND txo.tx_id <= _last_tx_id
			GROUP BY
				ca.asset_id, txo.address;


			-- Now, temp_recalculated_balances contains the data and can be used in subsequent logic
			SELECT COUNT(*) INTO _changed_assets_count FROM temp_changes_assets;
			SELECT COUNT(*) INTO _recalculated_balances_count FROM temp_recalculated_balances;

			INSERT INTO _cbi_asset_addresses_cache (asset_id, address, quantity)
			SELECT
				asset_id,
				address,
				new_quantity
			FROM
				temp_recalculated_balances
			ON CONFLICT (asset_id, address)
			DO UPDATE SET
				quantity = EXCLUDED.quantity;

			-- Set quantities to 0 for pairs not in recalculated_balances but present in _cbi_asset_addresses_cache
			UPDATE _cbi_asset_addresses_cache cac
			SET quantity = 0
			WHERE
				(cac.asset_id, cac.address) NOT IN (SELECT asset_id, address FROM temp_recalculated_balances)
				AND (cac.asset_id) IN (SELECT asset_id FROM temp_changes_assets);

			-- Log the counts
			RAISE NOTICE 'cbi_asset_addresses_cache_update - Number of entries in changed_pairs: %', _changed_assets_count;
			RAISE NOTICE 'cbi_asset_addresses_cache_update - Number of entries in recalculated_balances: %', _recalculated_balances_count;

			-- Ensure to clean up if you're using a temp table
			DROP TABLE temp_changes_assets;
			DROP TABLE temp_recalculated_balances;
		end if;


	   --update the handler table
		if _handler_last_tx_id is null or _handler_last_tx_id = 0 then
			insert into _cbi_cache_handler_state(table_name, last_tx_id) values('_cbi_asset_addresses_cache', _last_tx_id);
		else
			update _cbi_cache_handler_state set last_tx_id = _last_tx_id
			where table_name = '_cbi_asset_addresses_cache';
		end if;

		raise notice 'cbi_asset_addresses_cache_update - complete';
	end;
$$;


call cbi_asset_addresses_cache_update();

select * from _cbi_cache_handler_state;
select * from _cbi_cache_handler_state where table_name='_cbi_asset_addresses_cache';

insert into _cbi_cache_handler_state(table_name, last_tx_id) values('_cbi_asset_addresses_cache', 84664109);
update _cbi_cache_handler_state set last_tx_id=63473990 where table_name='_cbi_asset_addresses_cache';
delete from _cbi_cache_handler_state where table_name='_cbi_asset_addresses_cache';

select max(id) from tx;
select coalesce(last_tx_id, 0) from _cbi_cache_handler_state where table_name = '_cbi_asset_addresses_cache';
select array_agg(distinct id) from tx where id > 626533;

85423686
84664109

select count(1) from _cbi_asset_addresses_cache;
-- 16671323
-- 20679213
-- 20679215

select * from _cbi_asset_addresses_cache limit 20;
select * from _cbi_asset_addresses_cache where quantity=0 limit 20;


select * from multi_asset ma where ma.fingerprint = 'asset1gqp4wdmclgw2tqmkm3nq7jdstvqpesdj3agnel';

select max(no) from epoch;
        
select *
from tx
where tx.hash = '\xf07842b9f8502972204ac1f4b54085b8a0047bf60f3ea5734339fd38f3f49b5a';

select * from ma_tx_out mto where mto.


select mtm.id
from ma_tx_mint mtm
inner join multi_asset ma on ma.id = mtm.ident
where ma.fingerprint = 'asset1gqp4wdmclgw2tqmkm3nq7jdstvqpesdj3agnel'
order by mtm.id;

select * from "_cbi_asset_cache" cac where mint_cnt>2 and burn_cnt>0;
select * from multi_asset ma where ma.id=4366;


select
	max(txo.tx_id)
from
	ma_tx_out mto
	inner join multi_asset ma on ma.id = mto.ident 
	inner join tx_out txo on txo.id = mto.tx_out_id
	inner join tx on tx.id = txo.tx_id
	inner join block b on b.id = tx.block_id
where b.epoch_no = 100;

select * from epoch order by no desc limit 10;


select
	*
from
	pg_stat_activity
where
	state = 'active' and query ilike '%public.cbi_asset_addresses_cache_update%'
	and datname = (select current_database());

select * from temp_changes_assets;



    SELECT
        ma.id as asset_id,
        txo.address,
        SUM(mto.quantity) as quantity
    FROM
        ma_tx_out mto
        INNER JOIN multi_asset ma ON ma.id = mto.ident
        INNER JOIN tx_out txo ON txo.id = mto.tx_out_id
        LEFT JOIN tx_in ON txo.tx_id = tx_in.tx_out_id
            AND txo.index::smallint = tx_in.tx_out_index::smallint
    WHERE
        tx_in.tx_out_id IS NULL
        AND txo.tx_id > _handler_last_tx_id
        AND txo.tx_id <= _last_tx_id
    GROUP BY ma.id, txo.address


--analysis of true delta approach

select * from _cbi_cache_handler_state where table_name='_cbi_asset_addresses_cache';
84664109

--WITH new_unspent as (
    SELECT
        ma.id as asset_id,
        txo.address,
        SUM(mto.quantity) as quantity
    FROM
        ma_tx_out mto
        INNER JOIN multi_asset ma ON ma.id = mto.ident
        INNER JOIN tx_out txo ON txo.id = mto.tx_out_id
        LEFT JOIN tx_in ON txo.tx_id = tx_in.tx_out_id
            AND txo.index::smallint = tx_in.tx_out_index::smallint
    WHERE
        tx_in.tx_out_id IS NULL
        AND txo.tx_id > 84664109
        AND txo.tx_id <= 84664149
    GROUP BY ma.id, txo.address;



1	8177303	addr1q88rm02jjawpfpxwzefysgq36faffjvsxrcaqrvucm8kmdq79dc452gj84xkhtgr605xsapzu4gv38v66qdddn20urwsp7lcac	35500

1	3670122	addr1x8rjw3pawl0kelu4mj3c8x20fsczf5pl744s9mxz9v8n7efvjel5h55fgjcxgchp830r7h2l5msrlpt8262r3nvr8ekstg4qrx	1
2	3787876	addr1q94nlc2nzqv4wy06pq5k5c77ea83c9uxv83mtkrzhy39hgmskv63zyl9jxggf7xef3xqmx949m45asqjeazt5lzacy4sztvu9a	2271531682
3	6489687	addr1q94nlc2nzqv4wy06pq5k5c77ea83c9uxv83mtkrzhy39hgmskv63zyl9jxggf7xef3xqmx949m45asqjeazt5lzacy4sztvu9a	1
4	7789584	addr1q94nlc2nzqv4wy06pq5k5c77ea83c9uxv83mtkrzhy39hgmskv63zyl9jxggf7xef3xqmx949m45asqjeazt5lzacy4sztvu9a	500
5	8177303	addr1q88rm02jjawpfpxwzefysgq36faffjvsxrcaqrvucm8kmdq79dc452gj84xkhtgr605xsapzu4gv38v66qdddn20urwsp7lcac	355000
6	8177303	addr1q94nlc2nzqv4wy06pq5k5c77ea83c9uxv83mtkrzhy39hgmskv63zyl9jxggf7xef3xqmx949m45asqjeazt5lzacy4sztvu9a	104173
7	8183778	addr1q94nlc2nzqv4wy06pq5k5c77ea83c9uxv83mtkrzhy39hgmskv63zyl9jxggf7xef3xqmx949m45asqjeazt5lzacy4sztvu9a	30375220603
8	8533134	addr1q9sda2k92nndusflqkfr7c6sarphtf4fjvsj3wm8zp0vyh29ujzygy0jzu6pn5hq5gu787lgljn68rj6lm2pcqszrn9sqmyj7z	1
9	8866051	addr1q9sda2k92nndusflqkfr7c6sarphtf4fjvsj3wm8zp0vyh29ujzygy0jzu6pn5hq5gu787lgljn68rj6lm2pcqszrn9sqmyj7z	1
10	8866053	addr1q9sda2k92nndusflqkfr7c6sarphtf4fjvsj3wm8zp0vyh29ujzygy0jzu6pn5hq5gu787lgljn68rj6lm2pcqszrn9sqmyj7z	1
11	8868100	addr1q9sda2k92nndusflqkfr7c6sarphtf4fjvsj3wm8zp0vyh29ujzygy0jzu6pn5hq5gu787lgljn68rj6lm2pcqszrn9sqmyj7z	1
12	9379672	addr1q94nlc2nzqv4wy06pq5k5c77ea83c9uxv83mtkrzhy39hgmskv63zyl9jxggf7xef3xqmx949m45asqjeazt5lzacy4sztvu9a	1
13	9399030	addr1q94nlc2nzqv4wy06pq5k5c77ea83c9uxv83mtkrzhy39hgmskv63zyl9jxggf7xef3xqmx949m45asqjeazt5lzacy4sztvu9a	1

), spent as (

    SELECT
        ma.id as asset_id,
        txo.address,
        SUM(mto.quantity) as quantity
    FROM
        ma_tx_out mto
        INNER JOIN multi_asset ma ON ma.id = mto.ident
        INNER JOIN tx_out txo ON txo.id = mto.tx_out_id
        INNER JOIN tx_in ON txo.tx_id = tx_in.tx_out_id
            AND txo.index::smallint = tx_in.tx_out_index::smallint
    WHERE
        tx_in.tx_out_id IS NOT NULL
        AND txo.tx_id > 84664109
        AND txo.tx_id <= 84664149
		-- and ma.id=8866051
    GROUP BY ma.id, txo.address
	order by ma.id;



select * 
from _cbi_asset_addresses_cache caac
where 0=0 
-- and caac.asset_id = 8533134 
and caac.address in ('addr1qyqxm8pcwynlpqh6uw20ane658c3fhh7kqegy0g3lq6k4tva5qaft7n6d70esrkvezly8mv433hyd2ujf943x0u4u0qqqfhtrz',
	'',
	'',
	'');


42	8177303	addr1q8ljcy9n440n0kg4zd357n82pge7mkdsuur4g9qn66x2ahzhulqgcwas5crx26v8hnjgen5pz84rwup82dlvwev4v9qq6792kf	105107
43	8177303	addr1x8nz307k3sr60gu0e47cmajssy4fmld7u493a4xztjrll0aj764lvrxdayh2ux30fl0ktuh27csgmpevdu89jlxppvrswgxsta	2115141
44	8177303	addr1z8snz7c4974vzdpxu65ruphl3zjdvtxw8strf2c2tmqnxzgkfzj4qgs7k3f77pacnyh392326tsnh0v8qlgcyevxsslquxmw5a	1975466504


select * from _cbi_asset_cache cac where cac.asset_id=267;
select * from multi_asset where id=267;

---unspent new logic:

SELECT txo.tx_id, txo.index, txo.address, mto.quantity, mto.ident AS asset_id

SELECT
	mto.ident as asset_id,
	txo.address,
	SUM(mto.quantity) as quantity
FROM tx_out txo
JOIN ma_tx_out mto ON txo.id = mto.tx_out_id
LEFT JOIN tx_in tin ON txo.tx_id = tin.tx_out_id AND txo.index = tin.tx_out_index
WHERE tin.tx_in_id IS NULL
AND txo.tx_id > 84664109
AND txo.tx_id <= 84664149
-- AND txo.tx_id <= 85452672
-- and mto.ident = 267
-- and txo.address='addr1qyqxm8pcwynlpqh6uw20ane658c3fhh7kqegy0g3lq6k4tva5qaft7n6d70esrkvezly8mv433hyd2ujf943x0u4u0qqqfhtrz'
GROUP BY mto.ident, txo.address;

-- ORIGINAL
    SELECT
        ma.id as asset_id,
        txo.address,
        SUM(mto.quantity) as quantity
    FROM
        ma_tx_out mto
        INNER JOIN multi_asset ma ON ma.id = mto.ident
        INNER JOIN tx_out txo ON txo.id = mto.tx_out_id
        LEFT JOIN tx_in ON txo.tx_id = tx_in.tx_out_id
            AND txo.index::smallint = tx_in.tx_out_index::smallint
    WHERE
        tx_in.tx_out_id IS NULL
        AND txo.tx_id > 84664109
        AND txo.tx_id <= 84664149
    GROUP BY ma.id, txo.address;

tx_in.tx_out_id IS NULL

--Trace Inputs to Find Addresses with Decreased Balances
SELECT txo.tx_id, txo.index, txo.address, mto.quantity, mto.ident AS asset_id

SELECT
	mto.ident as asset_id,
	txo.address,
	SUM(mto.quantity) as quantity
FROM tx_in tin
JOIN tx_out txo ON tin.tx_out_id = txo.tx_id AND tin.tx_out_index = txo.index
JOIN ma_tx_out mto ON txo.id = mto.tx_out_id
WHERE 0=0  
-- AND tin.tx_in_id IS NOT NULL
AND tin.tx_out_id IS NOT NULL
AND txo.tx_id > 84664109
AND txo.tx_id <= 84664149
GROUP BY mto.ident, txo.address;

select max(tx_id) from tx_out;


--attempt 2 to  Find Addresses with Decreased Balances
SELECT
    tin_prev.tx_id AS spent_tx_id,
    txo_prev.address AS spent_address,
    mto_prev.quantity AS spent_quantity,
    mto_prev.ident AS asset_id
FROM tx_in tin
JOIN tx_out txo ON tin.tx_out_id = txo.tx_id AND tin.tx_out_index = txo.index -- Join to find the UTXO being spent
JOIN tx_in tin_prev ON txo.tx_id = tin_prev.tx_id -- Join on tx_in to find the transaction that provided the spent UTXO
JOIN tx_out txo_prev ON tin_prev.tx_out_id = txo_prev.tx_id AND tin_prev.tx_out_index = txo_prev.index -- Find the previous tx_out that was spent
JOIN ma_tx_out mto_prev ON txo_prev.id = mto_prev.tx_out_id -- Get the asset information from the spent output
WHERE txo.tx_id IN (
    SELECT tx_id
    FROM tx_out txo_new
    JOIN ma_tx_out mto_new ON txo_new.id = mto_new.tx_out_id
    LEFT JOIN tx_in tin_new ON txo_new.tx_id = tin_new.tx_out_id AND txo_new.index = tin_new.tx_out_index
    WHERE tin_new.tx_in_id IS NULL
    -- AND txo_new.tx_id > {last_processed_tx_id}
	AND txo_new.tx_id > 84664109
	AND txo_new.tx_id <= 84664149
)
AND txo_prev.tx_id <= 84664109;


SELECT
    mto.ident AS asset_id,
    txo_prev.address,
    SUM(mto.quantity) AS decreased_quantity
FROM tx_in
INNER JOIN tx_out txo_prev ON tx_in.tx_out_id = txo_prev.tx_id AND tx_in.tx_out_index = txo_prev.index
INNER JOIN ma_tx_out mto ON txo_prev.id = mto.tx_out_id
WHERE txo_prev.tx_id IN (
    SELECT DISTINCT tx_out.tx_id
    FROM tx_out
    JOIN ma_tx_out ON tx_out.id = ma_tx_out.tx_out_id
    LEFT JOIN tx_in ON tx_out.tx_id = tx_in.tx_out_id AND tx_out.index = tx_in.tx_out_index
    WHERE tx_in.id IS NULL -- Unspent UTXOs
    AND tx_out.tx_id > 84664109
	AND tx_out.tx_id <= 84664149
)
GROUP BY mto.ident, txo_prev.address;



--new unspent
	SELECT
		mto_new.ident as asset_id,
		txo_new.address,
		SUM(mto_new.quantity) as quantity
    FROM tx_out txo_new
    JOIN ma_tx_out mto_new ON txo_new.id = mto_new.tx_out_id
    LEFT JOIN tx_in tin_new ON txo_new.tx_id = tin_new.tx_out_id AND txo_new.index = tin_new.tx_out_index
    WHERE tin_new.tx_in_id IS NULL
    -- AND txo_new.tx_id > {last_processed_tx_id}
	AND txo_new.tx_id > 84664109
	AND txo_new.tx_id <= 84664149
	GROUP BY mto_new.ident, txo_new.address;






-----pay addr balance analysis - 2024.02.19

select * from tx where hash='\x2ea22a8fde4e2eafb740a105588b9b2ebdb72eac11845305958a072aa13a7373';
--85931334

select * from _cbi_asset_cache limit 10;
select * from multi_asset where policy = '\xa3931691f5c4e65d01c429e473d0dd24c51afdb6daf88e632a6c1e51' limit 10;


select to2.index as output_index, 
	to2.value,
	to2.address,
	false as is_collateral_output,
	encode(to2.data_hash::bytea, 'hex') as data_hash,
	to2."index",
	mto.quantity,
	encode(ma."policy" ::bytea, 'hex') as policy, 
	ma."name", 
	ma.fingerprint,
	encode(ma."name" ::bytea, 'escape') as clear_name,
	convert_from(ma."name" ::bytea, 'UTF-8') as clear_name
from tx_out to2 
left join ma_tx_out mto on mto.tx_out_id = to2.id 
left join multi_asset ma on ma.id = mto.ident 
where to2.tx_id = 85931334;


SELECT
	mto.ident as asset_id,
	txo.address,
	SUM(mto.quantity) as quantity,
	SUM(txo.value) as ada_value
FROM tx_out txo
JOIN ma_tx_out mto ON txo.id = mto.tx_out_id
LEFT JOIN tx_in txin ON txo.tx_id = txin.tx_out_id AND txo.index = txin.tx_out_index
WHERE txin.tx_in_id IS NULL
and txo.address='addr1qx83su0tl284szup8y6kd2ftj7jxfsuekv52rp5qyzz0t0k8m4lte2j35e2mza296llhhay4x6537tyzydalequcxc3qu3nx35'
GROUP BY mto.ident, txo.address;

SELECT
	mto.ident as asset_id,
	txo.address,
	SUM(mto.quantity) as quantity,
	SUM(txo.value) as ada_value
FROM tx_out txo
JOIN ma_tx_out mto ON txo.id = mto.tx_out_id and mto.ident=8809843
LEFT JOIN tx_in txin ON txo.tx_id = txin.tx_out_id AND txo.index = txin.tx_out_index
WHERE txin.tx_in_id IS NULL
and txo.address in ('addr1qx83su0tl284szup8y6kd2ftj7jxfsuekv52rp5qyzz0t0k8m4lte2j35e2mza296llhhay4x6537tyzydalequcxc3qu3nx35',
'addr1q80mmlz20t4gh67h48qgwvyr4t64mkxfmg0crur5vna4d92v3e480e3xdyp2j70wewt5u4ufc34qrg4ahn306kctx5kswu9xfe')
GROUP BY mto.ident, txo.address;


unspent_outputs AS (
    SELECT
        tx_out.address AS payment_address,
        SUM(tx_out.value) AS current_ada_value,
        COALESCE(SUM(ma_tx_out.quantity), 0) AS current_fact_quantity
    FROM
        tx_out
        LEFT JOIN ma_tx_out ON tx_out.id = ma_tx_out.tx_out_id AND ma_tx_out.ident = ${process.env.ORCFAX_MULTIASSET_ID}::bigint
    WHERE
        tx_out.address IN (SELECT payment_address FROM reservation_transactions)
        AND NOT EXISTS (
            -- Check if the tx_out is spent by looking for a corresponding tx_in
            SELECT 1
            FROM tx_in
            WHERE tx_in.tx_out_id = tx_out.id AND tx_in.tx_out_index = tx_out.index
        )
    GROUP BY tx_out.address
),


SELECT
	mto_new.ident as asset_id,
	txo_new.address,
	SUM(mto_new.quantity) as quantity
FROM tx_out txo_new
JOIN ma_tx_out mto_new ON txo_new.id = mto_new.tx_out_id
LEFT JOIN tx_in tin_new ON txo_new.tx_id = tin_new.tx_out_id AND txo_new.index = tin_new.tx_out_index
WHERE tin_new.tx_in_id IS NULL
and txo_new.address='addr1q80mmlz20t4gh67h48qgwvyr4t64mkxfmg0crur5vna4d92v3e480e3xdyp2j70wewt5u4ufc34qrg4ahn306kctx5kswu9xfe'
GROUP BY mto_new.ident, txo_new.address;