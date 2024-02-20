create or replace procedure public.cbi_address_stats_cache_update()
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
		      state = 'active' and query ilike '%public.cbi_address_stats_cache_update%'
		      and datname = (select current_database())
		  ) then 
		    raise exception 'cbi_address_stats_cache_update already running, exiting!';
		end if;
	
		--determine what needs doing, ie create or update with epoch delta 
	    select max(tx.id) into _last_tx_id from tx;
		select coalesce(last_tx_id, 0) into _last_processed_tx_id from _cbi_cache_handler_state where table_name = '_cbi_address_stats_cache';
	
		if _last_processed_tx_id is null then
			truncate table _cbi_address_stats_cache;
			select 0 into _last_processed_tx_id;
			raise notice 'cbi_address_stats_cache_update - building cache from scratch...';
		else
			if _last_tx_id <= _last_processed_tx_id then
				raise notice 'cbi_address_stats_cache_update - no new transaction to process, exiting!';
				return;
			else
				raise notice 'cbi_address_stats_cache_update - updating cache - last processed tx id % - latest known tx id %.', _last_processed_tx_id, _last_tx_id;
			end if;
		end if;
	
		raise notice 'cbi_address_stats_cache_update - INFO - _last_processed_tx_id: %, _last_tx_id: %', _last_processed_tx_id, _last_tx_id;
	
        insert into _cbi_address_stats_cache(epoch_no,address,stake_address_id,tx_count)
        select block.epoch_no, tx_out.address, coalesce(sa.id,0) as stake_address_id, count(*) as tx_count
        from tx
        inner join block on tx.block_id = block.id
        inner join tx_out on tx.id = tx_out.tx_id
        left join stake_address sa on tx_out.stake_address_id = sa.id 
        where tx.id > _last_processed_tx_id and tx.id <= _last_tx_id
		and length(tx_out.address)<255 --to not account for pre-shelley addresses with random lengths
        group by block.epoch_no, tx_out.address, sa.id
        on conflict on constraint _cbi_address_stats_cache_unique do
            update
                set tx_count = _cbi_address_stats_cache.tx_count + excluded.tx_count;


 		--update the handler table
		if _last_processed_tx_id = 0 then
			insert into _cbi_cache_handler_state(table_name, last_tx_id) values('_cbi_address_stats_cache', _last_tx_id);
		else
			update _cbi_cache_handler_state set last_tx_id = _last_tx_id
			where table_name = '_cbi_address_stats_cache';
		end if;
		
		raise notice 'cbi_address_stats_cache_update - COMPLETE';
	end;
$$;


call cbi_address_stats_cache_update();

select * from _cbi_cache_handler_state;
select * from _cbi_cache_handler_state where table_name = '_cbi_address_stats_cache';
--75808739	
--86029649

delete from _cbi_cache_handler_state where table_name = '_cbi_address_stats_cache';

truncate table _cbi_address_stats_cache;
select count(*) from _cbi_address_stats_cache;
select * from _cbi_address_stats_cache limit 10;

select * from _cbi_address_stats_cache;



select tx_out.*
from tx 
inner join tx_out on tx.id=tx_out.tx_id
where tx.id>1493693;

1493693
1493696

select count(*)
from tx 
inner join tx_out on tx.id=tx_out.tx_id
where tx.id>1493693
and tx_out.address='addr_test1wz8wsmsrh9j8x9kqszehtgypu6zutn9c6a0clyzzsxqtjscecq035';

--- find staking address with multiple payment address
select * from (
select sa.id, count(1) as ct
from _cbi_address_stats_cache casc
inner join stake_address sa on sa.id = casc.stake_address_id
group by sa.id
) a where a.ct>50;

--- find enterprise address
select * from (
select casc.address, sum(casc.tx_count) as ct from _cbi_address_stats_cache casc
where casc.stake_address_id=0
group by casc.address
) a order by a.ctesc;

select casc.address, sum(casc.tx_count) as ct 
from _cbi_address_stats_cache casc
where casc.stake_address_id=0 and casc.epoch_no >=90
group by casc.address
order by ct desc;

select max(no) from epoch;

--- preprod tests
--enterprise addresses
select * from _cbi_address_stats_cache where address='addr_test1wz8wsmsrh9j8x9kqszehtgypu6zutn9c6a0clyzzsxqtjscecq035';
select * from _cbi_address_stats_cache where address='addr_test1wpg6eael9tk4jzrnugcn000u9ej65wwm0c86uxcsugud89qdjvl6t';

--staking address: stake_test1urmpqeeexrkyjna48lmsx30a8auy4s0p40u2n9ugx8hdrfgfl3r44
select * from _cbi_address_stats_cache where stake_address_id=1203 order by epoch_no desc, address desc;
select * from _cbi_address_stats_cache where stake_address_id=1203 order by epoch_no asc, address asc;
select * from stake_address where id=1203;


update _cbi_address_stats_cache
set tx_count=4
where epoch_no=93 and address='addr_test1wrv9l2du900ajl27hk79u07xda68vgfugrppkua5zftlp8g0l9djk';

insert into _cbi_address_stats_cache(epoch_no,address,stake_address_id,tx_count)
select 93, 'addr_test1wrv9l2du900ajl27hk79u07xda68vgfugrppkua5zftlp8g0l9djk', 0, 10
on conflict on constraint _cbi_address_stats_cache_unique do
    update
        set tx_count = _cbi_address_stats_cache.tx_count + excluded.tx_count;

select * from _cbi_address_stats_cache where stake_address_id=61493 order by epoch_no desc;

95	61493	552

select * from _cbi_address_info_cache where address='addr_test1wz8wsmsrh9j8x9kqszehtgypu6zutn9c6a0clyzzsxqtjscecq035';

select max(no) from epoch;
select * from stake_address limit 10;
select * from epoch limit 10;

drop table _cbi_address_stats_cache;

-- _cbi_address_stats_cache
create table if not exists public._cbi_address_stats_cache (
  epoch_no word31type,
  address varchar,
  stake_address_id int8 DEFAULT 0,
  tx_count int8,
  CONSTRAINT "_cbi_address_stats_cache_unique" UNIQUE (epoch_no, address, stake_address_id)
);

CREATE INDEX _cbi_address_stats_cache_1 ON public._cbi_address_stats_cache USING btree (address);
CREATE INDEX _cbi_address_stats_cache_2 ON public._cbi_address_stats_cache USING btree (stake_address_id);
CREATE INDEX _cbi_address_stats_cache_3 ON public._cbi_address_stats_cache USING btree (epoch_no, address);
CREATE INDEX _cbi_address_stats_cache_4 ON public._cbi_address_stats_cache USING btree (epoch_no, stake_address_id);


-- Drop the existing unique index
DROP INDEX IF EXISTS _cbi_address_stats_cache_1;




