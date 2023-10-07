--Building procedure to create and update stake distribution cache table
create or replace procedure public.cbi_stake_distribution_cache_update()
language plpgsql
as $$
declare
  _current_epoch_no bigint;
  _current_active_stake_epoch_no bigint;
  _last_block_no bigint;
  _last_active_stake_tx_id bigint;
  _last_processed_block_no bigint;
 _cbi_active_stake_cache_last_processed_epoch_no bigint;
	begin
		--avoid concurrent runs
		if (
		    select
		      count(pid) > 1
		    from
		      pg_stat_activity
		    where
		      state = 'active' and query ilike '%public.cbi_stake_distribution_cache_update%'
		      and datname = (select current_database())
		  ) then 
		    raise exception 'cbi_stake_distribution_cache_update already running, exiting!';
		end if;
	
		--determine what needs doing, ie create or update with epoch delta 
		select max(no) into _current_epoch_no from epoch;
	  	select (_current_epoch_no::integer - 2)::integer into _current_active_stake_epoch_no;
	  	select max(block_no) into _last_block_no from block where block_no is not null;
	    select max(tx.id) into _last_active_stake_tx_id from tx inner join block b on b.id = tx.block_id where b.epoch_no <= _current_active_stake_epoch_no and b.block_no is not null and b.tx_count != 0;
		select coalesce(last_processed_block_no, 0) into _last_processed_block_no from _cbi_cache_handler_state where table_name = '_cbi_stake_distribution_cache';
		select coalesce(last_processed_epoch_no, 0) into _cbi_active_stake_cache_last_processed_epoch_no from _cbi_cache_handler_state where table_name = '_cbi_active_stake_cache_*';
	
		if _last_processed_block_no is null then
			truncate table _cbi_stake_distribution_cache;
			select 0 into _last_processed_block_no;
			raise notice 'cbi_stake_distribution_cache_update - building cache from scratch...';
		else
			if _last_block_no <= _last_processed_block_no then
				raise notice 'cbi_stake_distribution_cache_update - no new block to process, exiting!';
				return;
			else
				if _cbi_active_stake_cache_last_processed_epoch_no < _current_epoch_no then
					raise notice 'cbi_stake_distribution_cache_update - _cbi_active_stake_cache is not up to date (last processed epoch no: % vs current epoch no: %), exiting!', _cbi_active_stake_cache_last_processed_epoch_no, _current_epoch_no;
					return;
				end if;
				raise notice 'cbi_stake_distribution_cache_update - updating cache - last processed block no % - latest block no %.', _last_processed_block_no, _last_block_no;
			end if;
		end if;
	
		raise notice 'cbi_stake_distribution_cache_update - INFO - _current_epoch_no: %, _current_active_stake_epoch_no: %', _current_epoch_no, _current_active_stake_epoch_no;
		raise notice 'cbi_stake_distribution_cache_update - INFO - _last_block_no: %, _last_active_stake_tx_id: %', _last_block_no, _last_active_stake_tx_id;
		raise notice 'cbi_stake_distribution_cache_update - INFO - _last_processed_block_no: %', _last_processed_block_no;
	
		with 
			latest_reg_dereg as (
				select
					addr_id,
					max(case when reg_dereg = 'registration' then rd_tx_id end) as max_reg_tx_id,
					max(case when reg_dereg = 'deregistration' then rd_tx_id end) as max_dereg_tx_id,
					max(rd_tx_id) as max_reg_dereg_tx_id,
					max(epoch_no) as max_reg_dereg_eopch_no,
					max(case when reg_dereg = 'registration' then epoch_no end) as max_reg_epoch_no,
					max(case when reg_dereg = 'deregistration' then epoch_no end) as max_dereg_epoch_no
				from (
					select
						addr_id,
						tx.id as rd_tx_id,
						epoch_no,
						'registration' as reg_dereg
					from stake_registration sr
					join tx on sr.tx_id = tx.id
					union all
					select
						addr_id,
						tx.id as rd_tx_id,
						epoch_no,
						'deregistration' as reg_dereg
					from stake_deregistration sdr
					join tx on sdr.tx_id = tx.id
				) reg_dereg_data
				group by addr_id
			),
			latest_delegation as (
				select
					d.addr_id,
					d.pool_hash_id,
					d.active_epoch_no,
					d.tx_id
				from
					delegation d
					join (
					select
						addr_id,
						max(tx_id) as latest_deleg_tx_id
					from
						delegation
					group by
						addr_id
					) ld on d.addr_id = ld.addr_id and d.tx_id = ld.latest_deleg_tx_id
			),
			accounts_info as (
				select 
					sa.id as stake_address_id,
					-- sa.view as stake_address,
					coalesce(lrd.max_reg_tx_id, 0) >= coalesce(lrd.max_dereg_tx_id, 0) as is_registered,
					encode(last_reg_derep_tx.hash::bytea, 'hex') as last_reg_dereg_tx,
					max_reg_dereg_eopch_no as last_reg_dereg_epoch_no,
					-- ph.view as delegated_pool_bech32,
					-- ph.id as pool_hash_id,
					ld.pool_hash_id,
					encode(last_deleg_tx.hash::bytea, 'hex') as last_deleg_tx,
					ld.active_epoch_no as delegated_since_epoch_no
				from stake_address sa
					left join latest_reg_dereg lrd on sa.id = lrd.addr_id
					left join tx last_reg_derep_tx on last_reg_derep_tx.id = lrd.max_reg_dereg_tx_id
					left join latest_delegation ld on sa.id = ld.addr_id and ld.active_epoch_no >= coalesce(lrd.max_dereg_epoch_no, 0)
					-- left join pool_hash ph on ld.pool_hash_id = ph.id
					left join tx last_deleg_tx on last_deleg_tx.id = ld.tx_id
			),
		    account_active_stake as (
		      select ai.stake_address_id, casca.amount
		      from "_cbi_active_stake_cache_account" casca 
		        inner join accounts_info ai on ai.stake_address_id = casca.stake_address_id
		        where casca.epoch_no = _current_epoch_no
		    ),
		    account_delta_tx_ins as (
		      select ai.stake_address_id, tx_in.tx_out_id as txoid, tx_in.tx_out_index as txoidx
		      from tx_in
		        left join tx_out on tx_in.tx_out_id = tx_out.tx_id and tx_in.tx_out_index::smallint = tx_out.index::smallint
		        inner join accounts_info ai on ai.stake_address_id = tx_out.stake_address_id
		        where tx_in.tx_in_id > _last_active_stake_tx_id
		    ),
		    account_delta_input as (
		      select tx_out.stake_address_id, coalesce(sum(tx_out.value), 0) as amount
		      from account_delta_tx_ins
		        left join tx_out on account_delta_tx_ins.txoid=tx_out.tx_id and account_delta_tx_ins.txoidx = tx_out.index
		        inner join accounts_info ai on ai.stake_address_id = tx_out.stake_address_id
		        group by tx_out.stake_address_id
		    ),
		    account_delta_output as (
		      select ai.stake_address_id, coalesce(sum(tx_out.value), 0) as amount
		      from tx_out
		        inner join accounts_info ai on ai.stake_address_id = tx_out.stake_address_id
		      where tx_out.tx_id > _last_active_stake_tx_id
		      group by ai.stake_address_id
		    ),
		    account_delta_rewards as (
		      select ai.stake_address_id, coalesce(sum(reward.amount), 0) as rewards
		      from reward
		        inner join accounts_info ai on ai.stake_address_id = reward.addr_id
		      where
		        ( reward.spendable_epoch >= _current_epoch_no and reward.spendable_epoch <= _current_epoch_no )
		        or ( reward.type = 'refund' and reward.spendable_epoch >= (_current_active_stake_epoch_no + 1) and reward.spendable_epoch <= _current_epoch_no )
		      group by ai.stake_address_id
		    ),
		    account_delta_withdrawals as (
		      select ai.stake_address_id, coalesce(sum(withdrawal.amount), 0) as withdrawals
		      from withdrawal
		        inner join accounts_info ai on ai.stake_address_id = withdrawal.addr_id
		      where withdrawal.tx_id > _last_active_stake_tx_id
		      group by ai.stake_address_id
		    ),
		    account_total_rewards as (
		      select ai.stake_address_id,
		        coalesce(sum(reward.amount), 0) as rewards
		      from reward
		        inner join accounts_info ai on ai.stake_address_id = reward.addr_id
		      where reward.spendable_epoch <= _current_epoch_no
		      group by ai.stake_address_id
		    ),
		    account_total_withdrawals as (
		      select ai.stake_address_id,
		        coalesce(sum(withdrawal.amount), 0) as withdrawals
		      from withdrawal
		        inner join accounts_info ai on ai.stake_address_id = withdrawal.addr_id
		      group by ai.stake_address_id
		    ),
			latest_withdrawal_txs as (
				select distinct on (addr_id)
					addr_id,
					tx_id
				from withdrawal w
					inner join accounts_info ai on ai.stake_address_id = w.addr_id
				order by addr_id, tx_id desc
			),
			latest_withdrawal_epochs as (
				select
					lwt.addr_id,
					b.epoch_no
				from block b 
					inner join tx on tx.block_id = b.id
					inner join latest_withdrawal_txs lwt on tx.id = lwt.tx_id
			)			
	
		-- insert into _cbi_stake_distribution_cache(stake_address, stake_id, is_registered, last_reg_dereg_tx, last_reg_dereg_epoch_no, pool_id, delegated_since_epoch_no, last_deleg_tx, total_balance, utxo, rewards, withdrawals, rewards_available)
		insert into _cbi_stake_distribution_cache(stake_address_id, is_registered, last_reg_dereg_tx, last_reg_dereg_epoch_no, pool_hash_id, delegated_since_epoch_no, last_deleg_tx, total_balance, utxo, rewards, withdrawals, rewards_available)
		--let's now handle accounts delegated to a pool
	    select
	    --   ai.stake_address,
	      ai.stake_address_id,
          ai.is_registered,
		  ai.last_reg_dereg_tx,
		  ai.last_reg_dereg_epoch_no,
	    --   ai.delegated_pool_bech32 as pool_id,
		  ai.pool_hash_id,
		  ai.delegated_since_epoch_no,
		  ai.last_deleg_tx,
	      coalesce(aas.amount, 0) + coalesce(ado.amount, 0) - coalesce(adi.amount, 0) + coalesce(adr.rewards, 0) - coalesce(adw.withdrawals, 0) as total_balance,
	      case
	        when ( coalesce(atrew.rewards, 0) - coalesce(atw.withdrawals, 0) ) <= 0 then
	          coalesce(aas.amount, 0) + coalesce(ado.amount, 0) - coalesce(adi.amount, 0) + coalesce(adr.rewards, 0) - coalesce(adw.withdrawals, 0)
	        else
	          coalesce(aas.amount, 0) + coalesce(ado.amount, 0) - coalesce(adi.amount, 0) + coalesce(adr.rewards, 0) - coalesce(adw.withdrawals, 0) - (coalesce(atrew.rewards, 0) - coalesce(atw.withdrawals, 0))
	      end as utxo,
	      coalesce(atrew.rewards, 0) as rewards,
	      coalesce(atw.withdrawals, 0) as withdrawals,
	      case
	        when ( coalesce(atrew.rewards, 0) - coalesce(atw.withdrawals, 0) ) <= 0 then 0
	        else coalesce(atrew.rewards, 0) - coalesce(atw.withdrawals, 0)
	      end as rewards_available
	    from accounts_info ai
	      left join account_active_stake aas on aas.stake_address_id = ai.stake_address_id
	      left join account_total_rewards atrew on atrew.stake_address_id = ai.stake_address_id
	      left join account_total_withdrawals atw on atw.stake_address_id = ai.stake_address_id
	      left join account_delta_input adi on adi.stake_address_id = ai.stake_address_id
	      left join account_delta_output ado on ado.stake_address_id = ai.stake_address_id
	      left join account_delta_rewards adr on adr.stake_address_id = ai.stake_address_id
	      left join account_delta_withdrawals adw on adw.stake_address_id = ai.stake_address_id
		where ai.pool_hash_id is not null
		union
		--let's now handle all other accounts (newly created, not delegated, unregistered)
		select
			-- ai.stake_address,
			ai.stake_address_id,
			ai.is_registered,
			ai.last_reg_dereg_tx,
			ai.last_reg_dereg_epoch_no,
			-- ai.delegated_pool_bech32 as pool_id,
			ai.pool_hash_id,
			ai.delegated_since_epoch_no,
			ai.last_deleg_tx,
			case when (coalesce(rewards_t.rewards, 0) - coalesce(withdrawals_t.withdrawals, 0)) < 0 then
				(coalesce(utxo_t.utxo, 0) + coalesce(rewards_t.rewards, 0) - coalesce(withdrawals_t.withdrawals, 0) + coalesce(reserves_t.reserves, 0) + coalesce(treasury_t.treasury, 0) - (coalesce(rewards_t.rewards, 0) - coalesce(withdrawals_t.withdrawals, 0)))
			else
				(coalesce(utxo_t.utxo, 0) + coalesce(rewards_t.rewards, 0) - coalesce(withdrawals_t.withdrawals, 0) + coalesce(reserves_t.reserves, 0) + coalesce(treasury_t.treasury, 0))
			end as total_balance,
			coalesce(utxo_t.utxo, 0) as utxo,
			coalesce(rewards_t.rewards, 0) as rewards,
			coalesce(withdrawals_t.withdrawals, 0) as withdrawals,
			case when (coalesce(rewards_t.rewards, 0) - coalesce(withdrawals_t.withdrawals, 0)) <= 0 then
				'0'
			else
				(coalesce(rewards_t.rewards, 0) - coalesce(withdrawals_t.withdrawals, 0))
			end as rewards_available
			-- coalesce(reserves_t.reserves, 0)::text as reserves,
			-- coalesce(treasury_t.treasury, 0)::text as treasury
		from accounts_info ai
		left join (
			select
				tx_out.stake_address_id,
				coalesce(sum(value), 0) as utxo
			from tx_out
			left join tx_in on tx_out.tx_id = tx_in.tx_out_id
				and tx_out.index::smallint = tx_in.tx_out_index::smallint
			where tx_in.tx_out_id is null
			group by tx_out.stake_address_id
		) utxo_t on utxo_t.stake_address_id = ai.stake_address_id
		left join (
			select
				reward.addr_id,
				coalesce(sum(reward.amount), 0) as rewards
			from reward
			where reward.spendable_epoch <= _current_epoch_no
			group by reward.addr_id
		) rewards_t on rewards_t.addr_id = ai.stake_address_id
		left join (
			select
				withdrawal.addr_id,
				coalesce(sum(withdrawal.amount), 0) as withdrawals
			from withdrawal
			group by withdrawal.addr_id
		) withdrawals_t on withdrawals_t.addr_id = ai.stake_address_id
		left join (
			select
				reserve.addr_id,
				coalesce(sum(reserve.amount), 0) as reserves
			from reserve
			inner join tx on tx.id = reserve.tx_id
			inner join block on block.id = tx.block_id
			inner join latest_withdrawal_epochs lwe on lwe.addr_id = reserve.addr_id
			where block.epoch_no >= lwe.epoch_no
			group by reserve.addr_id
		) reserves_t on reserves_t.addr_id = ai.stake_address_id
		left join (
			select
				treasury.addr_id,
				coalesce(sum(treasury.amount), 0) as treasury
			from treasury
			inner join tx on tx.id = treasury.tx_id
			inner join block on block.id = tx.block_id
			inner join latest_withdrawal_epochs lwe on lwe.addr_id = treasury.addr_id
			where block.epoch_no >= lwe.epoch_no
			group by treasury.addr_id
		) treasury_t on treasury_t.addr_id = ai.stake_address_id
		where ai.pool_hash_id is null
		on conflict (stake_address_id) do
	      update
	        set is_registered = excluded.is_registered,
				last_reg_dereg_tx = excluded.last_reg_dereg_tx,
				last_reg_dereg_epoch_no = excluded.last_reg_dereg_epoch_no,
				pool_hash_id = excluded.pool_hash_id,
				delegated_since_epoch_no = excluded.delegated_since_epoch_no,
				last_deleg_tx = excluded.last_deleg_tx,
				total_balance = excluded.total_balance,
				utxo = excluded.utxo,
				rewards = excluded.rewards,
				withdrawals = excluded.withdrawals,
				rewards_available = excluded.rewards_available;

 		--update the handler table
		if _last_processed_block_no = 0 then
			insert into _cbi_cache_handler_state(table_name, last_processed_block_no) values('_cbi_stake_distribution_cache', _last_block_no);
		else
			update _cbi_cache_handler_state set last_processed_block_no = _last_block_no
			where table_name = '_cbi_stake_distribution_cache';
		end if;
		
		raise notice 'cbi_stake_distribution_cache_update - COMPLETE';
	end;
$$;


call public.cbi_active_stake_cache_update();

call public.cbi_stake_distribution_cache_update();

select * from _cbi_cache_handler_state where table_name='_cbi_stake_distribution_cache';

delete from _cbi_cache_handler_state where table_name='_cbi_stake_distribution_cache';

select count(*) from _cbi_stake_distribution_cache;
select count(*) from stake_address;

SELECT query
FROM pg_stat_activity
WHERE pid = 528359;

select * from _cbi_stake_distribution_cache limit 10;

select * from _cbi_cache_handler_state;

delete from _cbi_cache_handler_state
where id in (9);

--rec
select a.stake_address,a.stake_id,a.total_balance,b.total_balance
from _cbi_stake_distribution_cache a
inner join _cbi_stake_distribution_cache b on b.stake_id = a.stake_id
where a.total_balance != b.total_balance;


truncate table _cbi_stake_distribution_cache;

select * from _cbi_stake_distribution_cache limit 20;

select * from _cbi_stake_distribution_cache
where is_registered is false and registered_since_epoch_no is not null;

/*mainnet*/
select * from _cbi_stake_distribution_cache
where pool_id = 'pool1y24nj4qdkg35nvvnfawukauggsxrxuy74876cplmxsee29w5axc';

select * from _cbi_active_stake_cache_pool
where pool_id = 'pool1y24nj4qdkg35nvvnfawukauggsxrxuy74876cplmxsee29w5axc'
order by epoch_no desc limit 10;

/*preprod*/
select csdc.* 
from _cbi_stake_distribution_cache csdc
inner join pool_hash ph on ph.id=csdc.pool_hash_id
where ph.view = 'pool132jxjzyw4awr3s75ltcdx5tv5ecv6m042306l630wqjckhfm32r'
order by csdc.total_balance desc;

select count(1)
from _cbi_stake_distribution_cache csdc
inner join pool_hash ph on ph.id=csdc.pool_hash_id
where ph.view = 'pool132jxjzyw4awr3s75ltcdx5tv5ecv6m042306l630wqjckhfm32r';

select * from _cbi_stake_distribution_cache
where pool_id = 'pool132jxjzyw4awr3s75ltcdx5tv5ecv6m042306l630wqjckhfm32r'
and stake_address = 'stake_test1uq86p033vvv7mxxl29w8p35ardqhu0tl2gl2ym5xzlqpxzsrar5z7';

select * from _cbi_stake_distribution_cache
where is_registered is true and pool_id is not null
and last_deleg_tx != last_reg_dereg_tx;

select * from _cbi_stake_distribution_cache
where pool_id is null and delegated_since_epoch_no is not null;


select * from _cbi_stake_distribution_cache order by stake_id;
select * from _cbi_stake_distribution_cache where stake_address = 'stake_test1upxue2rk4tp0e3tp7l0nmfmj6ar7y9yvngzu0vn7fxs9ags2apttt';
select * from _cbi_stake_distribution_cache where stake_address = 'stake_test1upjyj7s4vfmsyw429vy397gxd34v3qxa90lvuc639zaj5xcmvvyjm';
select * from _cbi_stake_distribution_cache where stake_address = 'stake_test1uztg6yppa0t30rslkrneva5c9qju40rhndjnuy356kxw83s6n95nu';
select * from _cbi_stake_distribution_cache where stake_address = 'stake_test1uqh4cqczjpcjgnd3vhntldk9utmc3754tyrxy9seghptzwc6zayzz';

select * from _cbi_active_stake_cache_account where stake_address = 'stake_test1uztg6yppa0t30rslkrneva5c9qju40rhndjnuy356kxw83s6n95nu';
select * from _cbi_active_stake_cache_account where stake_address = 'stake_test1upxue2rk4tp0e3tp7l0nmfmj6ar7y9yvngzu0vn7fxs9ags2apttt';
select * from _cbi_active_stake_cache_account where stake_address = 'stake_test1uqp6h3d80tvhj8regwrufqmxethkhht9ywj9s3n5d58svxqqxpeff';

select max(no) from epoch;

drop table _cbi_stake_distribution_cache;

create table if not exists public._cbi_stake_distribution_cache (
	stake_address varchar primary key,
	stake_id bigserial,
	is_registered boolean,
	last_reg_event_epoch_no numeric,
	pool_id varchar,
	delegated_since_epoch_no numeric,
	total_balance numeric,
	utxo numeric,
	rewards numeric,
	withdrawals numeric,
	rewards_available numeric
);




			


