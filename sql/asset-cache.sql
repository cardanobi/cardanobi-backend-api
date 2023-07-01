create or replace procedure public.cbi_asset_cache_update()
 language plpgsql
as $$
declare
  _last_tx_id bigint;
  _handler_last_tx_id bigint;
  _asset_id_list bigint[];
	begin
		--avoid concurrent runs
		if (
		    select
		      count(pid) > 1
		    from
		      pg_stat_activity
		    where
		      state = 'active' and query ilike '%public.cbi_asset_cache_update%'
		      and datname = (select current_database())
		  ) then 
		    raise exception 'cbi_asset_cache_update already running, exiting!';
		end if;
	
		--determinie what needs doing, ie create or update with transaction delta 
		select max(id) into _last_tx_id from tx;
		select coalesce(last_tx_id, 0) into _handler_last_tx_id from _cbi_cache_handler_state where table_name = '_cbi_asset_cache';
		
		raise notice 'cbi_asset_cache_update - info - _last_tx_id: %, _handler_last_tx_id: %', _last_tx_id, _handler_last_tx_id;
	
		if _handler_last_tx_id is null then
			truncate table _cbi_asset_cache;
			raise notice 'cbi_asset_cache_update - building cache from scratch...';
		else
			select array_agg(distinct ident) into _asset_id_list from ma_tx_mint where tx_id > _handler_last_tx_id;
			if _asset_id_list is null then
				raise notice 'cbi_asset_cache_update - no new mint events to process, exiting!';
				return;
			else
				raise notice 'cbi_asset_cache_update - updating cache with % new mint events.', array_length(_asset_id_list, 1);
			end if;
		end if;
	
	
		--build universe of mint events
		with
			tx_mint_universe as (
				select
				  mtm.ident,
				  min(mtm.tx_id) as first_mint_tx_id, 
				  max(mtm.tx_id) as last_mint_tx_id
				from ma_tx_mint mtm
				where mtm.tx_id > 0
				  and mtm.quantity > 0
				group by mtm.ident
		    ),
		    
		    tx_mint_keys as (
				select
				  tmu.ident,
				  tmu.first_mint_tx_id,
				  coalesce(array_agg(tm.key) filter(where tm.tx_id = tmu.first_mint_tx_id),'{}') as first_mint_keys,
				  tmu.last_mint_tx_id,
				  coalesce(array_agg(tm.key) filter(where tm.tx_id = tmu.last_mint_tx_id),'{}') as last_mint_keys
				from
					tx_mint_universe tmu
					left join tx_metadata tm on tm.tx_id = tmu.first_mint_tx_id or tm.tx_id = tmu.last_mint_tx_id
				group by tmu.ident, tmu.first_mint_tx_id, tmu.last_mint_tx_id
			)
		    
    	insert into _cbi_asset_cache
		    select
		      ma.id,
		      min(b.time) as creation_time,
		      sum(mtm.quantity) as total_supply,
		      sum(case when mtm.quantity > 0 then 1 else 0 end) as mint_cnt,
		      sum(case when mtm.quantity < 0 then 1 else 0 end) as burn_cnt,
		      tmk.first_mint_tx_id,
		      encode(txf.hash ::bytea, 'hex') as first_mint_tx_hash,
		      tmk.first_mint_keys,
		      tmk.last_mint_tx_id,
		      encode(txl.hash ::bytea, 'hex') as last_mint_tx_hash,
		      tmk.last_mint_keys
		    from
		      multi_asset ma
		      inner join ma_tx_mint mtm on mtm.ident = ma.id
		      inner join tx on tx.id = mtm.tx_id
		      inner join block b on b.id = tx.block_id
		      inner join tx_mint_keys tmk on tmk.ident = ma.id
		      inner join tx txf on txf.id = tmk.first_mint_tx_id
		      inner join tx txl on txl.id = tmk.last_mint_tx_id
		    group by ma.id, tmk.first_mint_tx_id, txf.hash, tmk.first_mint_keys, tmk.last_mint_tx_id, txl.hash, tmk.last_mint_keys
		on conflict (asset_id)
		do update set
			creation_time     = excluded.creation_time,
			total_supply      = excluded.total_supply,
			mint_cnt          = excluded.mint_cnt,
			burn_cnt          = excluded.burn_cnt,
			last_mint_tx_id   = excluded.last_mint_tx_id,
			last_mint_tx_hash   = excluded.last_mint_tx_hash,
			last_mint_keys    = excluded.last_mint_keys;
				   
		   --update the handler table
			if _handler_last_tx_id is null then
				insert into _cbi_cache_handler_state(table_name, last_tx_id) values('_cbi_asset_cache', _last_tx_id);
			else
				update _cbi_cache_handler_state set last_tx_id = _last_tx_id
				where table_name = '_cbi_asset_cache';
			end if;
	
		raise notice 'cbi_asset_cache_update - complete';
	end;
$$;


select *
from _cbi_asset_cache
order by last_mint_tx_id  desc;

call public.cbi_asset_cache_update();
select * from _cbi_cache_handler_state;
select * from _cbi_asset_cache;
select count(*) from _cbi_asset_cache;

select * from ma_tx_mint mtm where mtm.tx_id =618796;

select * from "_cbi_asset_cache" cac where cac.asset_id =115459;
select * from multi_asset ma where ma.id=115459;

select array_agg(distinct ident) from ma_tx_mint where tx_id > 618881;
select array_agg(distinct ident) from ma_tx_mint where tx_id > (select last_tx_id from "_cbi_cache_handler_state" cchs where cchs.table_name='_cbi_asset_cache');