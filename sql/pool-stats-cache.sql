create or replace procedure public.cbi_pool_stats_cache_update()
language plpgsql
as $$
declare
  _last_block_no bigint;
  _last_processed_block_no bigint;
  _current_epoch_no bigint;
  _last_active_stake_processed_epoch integer;
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
        select max(block_no) into _last_block_no from block where block_no is not null;
        select coalesce(last_processed_block_no, 0) into _last_processed_block_no from _cbi_cache_handler_state where table_name = '_cbi_pool_stats_cache';
        select max(no) into _current_epoch_no from epoch;
		select coalesce(last_processed_epoch_no, 0) into _last_active_stake_processed_epoch from _cbi_cache_handler_state where table_name = '_cbi_pool_stats_cache';
	
		if _last_processed_block_no is null then
			truncate table _cbi_pool_stats_cache;
			select 0 into _last_processed_block_no;
			select 0 into _last_active_stake_processed_epoch;
			raise notice 'cbi_pool_stats_cache_update - building cache from scratch...';
		else
			if _last_block_no <= _last_processed_block_no then
				raise notice 'cbi_pool_stats_cache_update - no new transaction to process, exiting!';
				return;
			else
				raise notice 'cbi_pool_stats_cache_update - updating cache - last processed tx id % - latest known tx id %.', _last_processed_block_no, _last_block_no;
			end if;
		end if;
	
		raise notice 'cbi_pool_stats_cache_update - INFO - _last_processed_block_no: %, _last_block_no: %', _last_processed_block_no, _last_block_no;
	
        with blockdata as (
            select 
                block.epoch_no,
                pool_hash.id as pool_hash_id,
                sum(block.tx_count) as tx_count,
                count(distinct block.block_no) as block_count
            from 
                block
                join slot_leader on block.slot_leader_id = slot_leader.id
                join pool_hash on pool_hash.id = slot_leader.pool_hash_id
            where 
                block.block_no > _last_processed_block_no and block.block_no <= _last_block_no 
            group by 
                block.epoch_no, pool_hash.id
        ),
        stakedata as (
            select 
                casca.epoch_no, 
                casca.pool_hash_id, 
                count(1) as delegator_count, 
                coalesce(sum(casca.amount), 0) as delegated_stakes
            from 
                _cbi_active_stake_cache_account casca
            where
                casca.epoch_no >= coalesce(_last_active_stake_processed_epoch,0) and
                casca.epoch_no <= _current_epoch_no
            group by 
                casca.epoch_no, casca.pool_hash_id
        )
        insert into _cbi_pool_stats_cache (
            epoch_no,
            pool_hash_id,
            tx_count,
            block_count,
            delegator_count,
            delegated_stakes
        )
        select 
            coalesce(blockdata.epoch_no, stakedata.epoch_no) as epoch_no,
            coalesce(blockdata.pool_hash_id, stakedata.pool_hash_id) as pool_hash_id,
            coalesce(blockdata.tx_count, 0) as tx_count,
            coalesce(blockdata.block_count, 0) as block_count,
            coalesce(stakedata.delegator_count, 0) as delegator_count,
            coalesce(stakedata.delegated_stakes, 0) as delegated_stakes
        from 
            blockdata
        full join 
            stakedata 
        on 
            blockdata.epoch_no = stakedata.epoch_no and blockdata.pool_hash_id = stakedata.pool_hash_id
        on conflict on constraint _cbi_pool_stats_cache_unique 
        do update 
        set 
            tx_count = _cbi_pool_stats_cache.tx_count + excluded.tx_count,
            block_count = _cbi_pool_stats_cache.block_count + excluded.block_count,
            delegator_count = excluded.delegator_count,
            delegated_stakes = excluded.delegated_stakes;

 		--update the handler table
		if _last_processed_block_no = 0 then
			insert into _cbi_cache_handler_state(table_name, last_processed_block_no, last_processed_epoch_no) values('_cbi_pool_stats_cache', _last_block_no, _current_epoch_no);
		else
			update _cbi_cache_handler_state 
            set last_processed_block_no = _last_block_no,
                last_processed_epoch_no = _current_epoch_no
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

14123

---query ADACT preprod
select cpsc.* 
from _cbi_pool_stats_cache cpsc
inner join pool_hash ph on ph.id=cpsc.pool_hash_id
where ph.view='pool132jxjzyw4awr3s75ltcdx5tv5ecv6m042306l630wqjckhfm32r'
order by epoch_no desc;



select max(no) from epoch;

select * from _cbi_pool_stats_cache limit 10;

drop table _cbi_pool_stats_cache;


-- _cbi_pool_stats_cache
CREATE TABLE public."_cbi_pool_stats_cache" (
    epoch_no int8 NOT NULL,
    pool_hash_id int8 NOT NULL,
    delegator_count int8 DEFAULT 0,
    delegated_stakes int8 DEFAULT 0,
    tx_count int8 DEFAULT 0,
    block_count int8 DEFAULT 0,
    CONSTRAINT "_cbi_pool_stats_cache_unique" PRIMARY KEY (epoch_no, pool_hash_id)
);
CREATE INDEX idx_cbi_pool_stats_pool_hash_id ON public._cbi_pool_stats_cache USING btree (pool_hash_id);
