create or replace procedure public.cbi_active_stake_cache_update()
language plpgsql
as $$
declare
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
		      state = 'active' and query ilike '%public.cbi_active_stake_cache_update%'
		      and datname = (select current_database())
		  ) then 
		    raise exception 'cbi_active_stake_cache_update already running, exiting!';
		end if;
	
		--determine what needs doing, ie create or update with epoch delta 
		select max(no) into _current_epoch_no from epoch;
		select coalesce(last_processed_epoch_no, 0) into _last_active_stake_processed_epoch from _cbi_cache_handler_state where table_name = '_cbi_active_stake_cache_*';
	
		if _last_active_stake_processed_epoch is null then
			--select 0 into _last_active_stake_processed_epoch;
			truncate table _cbi_active_stake_cache_epoch;
			truncate table _cbi_active_stake_cache_pool;
			truncate table _cbi_active_stake_cache_account;
			raise notice 'cbi_active_stake_cache_update - building cache from scratch...';
		else
			if _current_epoch_no <= _last_active_stake_processed_epoch then
				raise notice 'cbi_active_stake_cache_update - no epoch to process, exiting!';
				return;
			else
				raise notice 'cbi_active_stake_cache_update - updating cache from epoch no % to epoch no %.', _last_active_stake_processed_epoch, _current_epoch_no;
			end if;
		end if;
	
		raise notice 'cbi_active_stake_cache_update - INFO - _current_epoch_no: %, _last_active_stake_processed_epoch: %', _current_epoch_no, _last_active_stake_processed_epoch;
	
		/* _cbi_active_stake_cache_epoch */
	    insert into _cbi_active_stake_cache_epoch
	      select
	        epoch_stake.epoch_no,
	        sum(epoch_stake.amount) as amount
	      from
	        epoch_stake
	      where
	        epoch_stake.epoch_no >= coalesce(_last_active_stake_processed_epoch,0) and
	        epoch_stake.epoch_no <= _current_epoch_no
	      group by
	        epoch_stake.epoch_no
	      on conflict (
	        epoch_no
	      ) do update
	        set amount = excluded.amount
	        where _cbi_active_stake_cache_epoch.amount is distinct from excluded.amount;
	       
		/* _cbi_active_stake_cache_pool */
	    insert into _cbi_active_stake_cache_pool
	      select
	        pool_hash.view as pool_id,
	        epoch_stake.epoch_no,
	        sum(epoch_stake.amount) as amount
	      from
	        epoch_stake
	        inner join pool_hash on pool_hash.id = epoch_stake.pool_id
	      where
	        epoch_stake.epoch_no >= coalesce(_last_active_stake_processed_epoch,0) and
	        epoch_stake.epoch_no <= _current_epoch_no
	      group by
	        pool_hash.view,
	        epoch_stake.epoch_no
	    on conflict (
	      pool_id,
	      epoch_no
	    ) do update
	      set amount = excluded.amount
	      where _cbi_active_stake_cache_pool.amount is distinct from excluded.amount;
	     
		/* _cbi_active_stake_cache_account */	     
		insert into _cbi_active_stake_cache_account
	      select
	        stake_address.id as stake_address_id,
	        pool_hash.id as pool_hash_id,
	        epoch_stake.epoch_no as epoch_no,
	        sum(epoch_stake.amount) as amount
	      from
	        epoch_stake
	        inner join pool_hash on pool_hash.id = epoch_stake.pool_id
	        inner join stake_address on stake_address.id = epoch_stake.addr_id
	      where
	        epoch_stake.epoch_no > coalesce(_last_active_stake_processed_epoch,0) and
	        epoch_stake.epoch_no <= _current_epoch_no
	      group by
	        stake_address.id,
	        pool_hash.id,
	        epoch_stake.epoch_no
	    on conflict (
	      stake_address_id,
	      pool_hash_id,
	      epoch_no
	    ) do update
	        set amount = excluded.amount;
	
	    -- only keep last 5 epochs
	    -- delete from _cbi_active_stake_cache_account
	    --   where epoch_no <= (_current_epoch_no - 4);
      
 		--update the handler table
		if _last_active_stake_processed_epoch is null then
			insert into _cbi_cache_handler_state(table_name, last_processed_epoch_no) values('_cbi_active_stake_cache_*', _current_epoch_no);
		else
			update _cbi_cache_handler_state set last_processed_epoch_no = _current_epoch_no
			where table_name = '_cbi_active_stake_cache_*';
		end if;
		
		raise notice 'cbi_active_stake_cache_update - COMPLETE';
	end;
$$;

call public.cbi_active_stake_cache_update();

select * from _cbi_cache_handler_state;
select * from _cbi_cache_handler_state where table_name = '_cbi_active_stake_cache_*';

delete from _cbi_cache_handler_state where table_name = '_cbi_active_stake_cache_*';

select max(no)  from epoch;

delete from _cbi_cache_handler_state where id = 1;

select * from _cbi_active_stake_cache_account limit 10;

---start from scratch
truncate table _cbi_active_stake_cache_epoch;
truncate table _cbi_active_stake_cache_pool;
truncate table _cbi_active_stake_cache_account;

delete from _cbi_cache_handler_state where table_name = '_cbi_active_stake_cache_*';


---query results
--mainnet

select sa.view, ph.view, casca.amount
from _cbi_active_stake_cache_account casca
inner join pool_hash ph on ph.id=casca.pool_hash_id
inner join stake_address sa on sa.id=casca.stake_address_id
where casca.epoch_no=(select max(no) from epoch)
and ph.view='pool1y24nj4qdkg35nvvnfawukauggsxrxuy74876cplmxsee29w5axc'
order by casca.amount desc;
