create or replace procedure public.cbi_pool_stats_cache_update()
language plpgsql
as $$
declare
  _last_tx_id bigint;
  _last_processed_tx_id bigint;
	begin
		--avoid concurrent runs
		if (
		    select
		      count(pid) > 1
		    from
		      pg_stat_activity
		    where
		      state = 'active' and query ilike '%public.cbi_pool_stats_cache_update%'
		      and datname = (select current_database())
		  ) then 
		    raise exception 'cbi_pool_stats_cache_update already running, exiting!';
		end if;
	
		--determine what needs doing, ie create or update with epoch delta 
	    select max(tx.id) into _last_tx_id from tx;
		select coalesce(last_tx_id, 0) into _last_processed_tx_id from _cbi_cache_handler_state where table_name = '_cbi_pool_stats_cache';
	
		if _last_processed_tx_id is null then
			truncate table _cbi_pool_stats_cache;
			select 0 into _last_processed_tx_id;
			raise notice 'cbi_pool_stats_cache_update - building cache from scratch...';
		else
			if _last_tx_id <= _last_processed_tx_id then
				raise notice 'cbi_pool_stats_cache_update - no new transaction to process, exiting!';
				return;
			else
				raise notice 'cbi_pool_stats_cache_update - updating cache - last processed tx id % - latest known tx id %.', _last_processed_tx_id, _last_tx_id;
			end if;
		end if;
	
		raise notice 'cbi_pool_stats_cache_update - INFO - _last_processed_tx_id: %, _last_tx_id: %', _last_processed_tx_id, _last_tx_id;
	
        insert into _cbi_pool_stats_cache(epoch_no,pool_hash,tx_count,block_count)
        select block.epoch_no,
            pool_hash.view AS pool_hash,
            count(*) AS tx_count,
            COUNT(DISTINCT block.id) AS block_count
        from tx
            join block ON tx.block_id = block.id
            join slot_leader ON block.slot_leader_id = slot_leader.id
            join pool_hash ON pool_hash.id = slot_leader.pool_hash_id
        where tx.id > _last_processed_tx_id and tx.id <= _last_tx_id
        group by block.epoch_no, pool_hash.view
        on conflict on constraint _cbi_pool_stats_cache_unique do
            update
                set tx_count = _cbi_pool_stats_cache.tx_count + excluded.tx_count,
                    block_count = _cbi_pool_stats_cache.block_count + excluded.block_count;


 		--update the handler table
		if _last_processed_tx_id = 0 then
			insert into _cbi_cache_handler_state(table_name, last_tx_id) values('_cbi_pool_stats_cache', _last_tx_id);
		else
			update _cbi_cache_handler_state set last_tx_id = _last_tx_id
			where table_name = '_cbi_pool_stats_cache';
		end if;
		
		raise notice 'cbi_pool_stats_cache_update - COMPLETE';
	end;
$$;


call cbi_pool_stats_cache_update();

select * from _cbi_cache_handler_state;
select * from _cbi_cache_handler_state where table_name = '_cbi_pool_stats_cache';

delete from _cbi_cache_handler_state where table_name = '_cbi_pool_stats_cache';

truncate table _cbi_pool_stats_cache;
select count(*) from _cbi_pool_stats_cache;
select * from _cbi_pool_stats_cache limit 10;

select * from _cbi_pool_stats_cache 
where pool_hash='pool132jxjzyw4awr3s75ltcdx5tv5ecv6m042306l630wqjckhfm32r'
order by epoch_no desc;

select * from _cbi_pool_stats_cache;

drop table _cbi_pool_stats_cache;

-- _cbi_pool_stats_cache
create table if not exists public._cbi_pool_stats_cache (
  epoch_no word31type,
  pool_hash varchar,
  tx_count int8 default 0,
  block_count int8 default 0,
  CONSTRAINT "_cbi_pool_stats_cache_unique" UNIQUE (epoch_no, pool_hash)
);

CREATE INDEX _cbi_pool_stats_cache_1 ON public._cbi_pool_stats_cache USING btree (pool_hash);
CREATE INDEX _cbi_pool_stats_cache_2 ON public._cbi_pool_stats_cache USING btree (epoch_no, pool_hash);


-- Drop the existing unique index
DROP INDEX IF EXISTS _cbi_pool_stats_cache_1;

select * from _cbi_pool_stats_cache limit 10;




select block.epoch_no,
    pool_hash.view AS pool_hash,
    count(*) AS tx_count,
    COUNT(DISTINCT block.id) AS block_count
from tx
    join block ON tx.block_id = block.id
    join slot_leader ON block.slot_leader_id = slot_leader.id
    join pool_hash ON pool_hash.id = slot_leader.pool_hash_id
where pool_hash.view='pool132jxjzyw4awr3s75ltcdx5tv5ecv6m042306l630wqjckhfm32r'
group by block.epoch_no, pool_hash.view
order by block.epoch_no desc;