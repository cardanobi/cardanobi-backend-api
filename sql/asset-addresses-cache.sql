create or replace procedure public.cbi_asset_addresses_cache_update()
 language plpgsql
as $$
declare
  _last_tx_id bigint;
  _handler_last_tx_id bigint;
  _tx_id_list bigint[];
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
	
		--determinie what needs doing, ie create or update with transaction delta 
		select max(id) into _last_tx_id from tx;
		select coalesce(last_tx_id, 0) into _handler_last_tx_id from _cbi_cache_handler_state where table_name = '_cbi_asset_addresses_cache';
		
		raise notice 'cbi_asset_addresses_cache_update - info - _last_tx_id: %, _handler_last_tx_id: %', _last_tx_id, _handler_last_tx_id;
	
		if _handler_last_tx_id is null then
			truncate table _cbi_asset_addresses_cache;
            select 0 into _handler_last_tx_id;
			raise notice 'cbi_asset_addresses_cache_update - building cache from scratch, _handler_last_tx_id: %...', _handler_last_tx_id;
		else
			select array_agg(distinct id) into _tx_id_list from tx where id > _handler_last_tx_id;
			if _tx_id_list is null then
				raise notice 'cbi_asset_addresses_cache_update - no new multi-asset transactions to process, exiting!';
				return;
			else
				raise notice 'cbi_asset_cache_ucbi_asset_addresses_cache_updatepdate - updating cache with % new multi-asset transactions.', array_length(_tx_id_list, 1);
			end if;
		end if;
	
	
		--build universe of mint events
    	insert into _cbi_asset_addresses_cache
		select
	      x.id,
	      x.address,
	      sum(x.quantity)
	    from
	      (
	        select
	         ma.id,
	          txo.address,
	          mto.quantity
	        from
	          ma_tx_out mto
	          inner join multi_asset ma on ma.id = mto.ident 
	          inner join tx_out txo on txo.id = mto.tx_out_id
	          left join tx_in on txo.tx_id = tx_in.tx_out_id
	            and txo.index::smallint = tx_in.tx_out_index::smallint
	        where tx_in.tx_out_id is null and txo.tx_id > _handler_last_tx_id
	      ) x
	    group by
	      x.id, x.address
		on conflict (asset_id, address)
		do update set
			quantity     = excluded.quantity;
				   
	   --update the handler table
		if _handler_last_tx_id is null then
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

select max(id) from tx;
select coalesce(last_tx_id, 0) from _cbi_cache_handler_state where table_name = '_cbi_asset_addresses_cache';
select array_agg(distinct id) from tx where id > 626533;


select count(*) from _cbi_asset_addresses_cache;
select * from _cbi_asset_addresses_cache limit 20;


select * from multi_asset ma where ma.fingerprint = 'asset1gqp4wdmclgw2tqmkm3nq7jdstvqpesdj3agnel';


        
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
