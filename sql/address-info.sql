create or replace procedure public.cbi_address_info_cache_update()
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
		      state = 'active' and query ilike '%public.cbi_address_info_cache_update%'
		      and datname = (select current_database())
		  ) then 
		    raise exception 'cbi_address_info_cache_update already running, exiting!';
		end if;
	
		--determine what needs doing, ie create or update with epoch delta 
	    select max(tx.id) into _last_tx_id from tx;
		select coalesce(last_tx_id, 0) into _last_processed_tx_id from _cbi_cache_handler_state where table_name = '_cbi_address_info_cache';
	
		if _last_processed_tx_id is null then
			truncate table _cbi_address_info_cache;
			select 0 into _last_processed_tx_id;
			raise notice 'cbi_address_info_cache_update - building cache from scratch...';
		else
			if _last_tx_id <= _last_processed_tx_id then
				raise notice 'cbi_address_info_cache_update - no new transaction to process, exiting!';
				return;
			else
				raise notice 'cbi_address_info_cache_update - updating cache - last processed tx id % - latest known tx id %.', _last_processed_tx_id, _last_tx_id;
			end if;
		end if;
	
		raise notice 'cbi_address_info_cache_update - INFO - _last_processed_tx_id: %, _last_tx_id: %', _last_processed_tx_id, _last_tx_id;
	

		insert into _cbi_address_info_cache(address,stake_address_id,stake_address,script_hash)
	    select 
            distinct tx_out.address, tx_out.stake_address_id, sa.view as stake_address, encode(sa.script_hash::bytea, 'hex') as script_hash 
        from tx_out 
        left join stake_address sa on tx_out.stake_address_id=sa.id
		where tx_out.tx_id>_last_processed_tx_id and  tx_out.tx_id<=_last_tx_id
		and length(tx_out.address)<255 --to not account for pre-shelley addresses with random lengths
		on conflict (address) do
	      update
	        set stake_address_id = excluded.stake_address_id,
	          stake_address = excluded.stake_address,
	          script_hash = excluded.script_hash;

 		--update the handler table
		if _last_processed_tx_id = 0 then
			insert into _cbi_cache_handler_state(table_name, last_tx_id) values('_cbi_address_info_cache', _last_tx_id);
		else
			update _cbi_cache_handler_state set last_tx_id = _last_tx_id
			where table_name = '_cbi_address_info_cache';
		end if;
		
		raise notice 'cbi_address_info_cache_update - COMPLETE';
	end;
$$;


select max(no) from epoch;
select * from _cbi_cache_handler_state;
select * from _cbi_cache_handler_state where table_name = '_cbi_address_info_cache'

delete from _cbi_cache_handler_state where table_name = '_cbi_address_info_cache';

call public.cbi_address_info_cache_update();

truncate table _cbi_address_info_cache;

select count(*) from _cbi_address_info_cache;
select count(*) from address_info_view;
select count(*) from stake_address;

268378

select * from "_cbi_address_info_cache" caic 
where caic.stake_address = 'stake1u8a9qstrmj4rvc3k5z8fems7f0j2vztz8det2klgakhfc8ce79fma';

select * from "_cbi_address_info_cache" caic 
where caic.address = 'addr1q8d0kcr09n3djm6l34wectcusyf4vehacq9uela3mmm0fq38wwd839avat62hm6fvvfafh9dyazzszcgygk2zufplvhsxa2f77';

select * from "_cbi_address_info_cache" caic 
where caic.stake_address = 'stake_test1uqpuy4l076l65h02mccw38dfa8dstgl93aed2u9723wxw3ch5a6g3';

select * from (
select caic.stake_address_id, count(1) as ct from "_cbi_address_info_cache" caic 
group by caic.stake_address_id 
) a
where a.ct>1 limit 10;

select * from stake_address sa where sa.id=145;


drop table _cbi_address_info_cache;


-- _cbi_address_info_cache	
create table if not exists public._cbi_address_info_cache (
  address varchar,
  stake_address_id int8,
  stake_address varchar,
  script_hash text
);

CREATE UNIQUE INDEX _cbi_address_info_cache_1 ON public._cbi_address_info_cache USING btree (address);
CREATE INDEX _cbi_address_info_cache_2 ON public._cbi_address_info_cache USING btree (stake_address);
CREATE INDEX _cbi_address_info_cache_3 ON public._cbi_address_info_cache USING btree (stake_address_id);




			


