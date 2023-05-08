create or replace procedure public.cbi_cache_handler_init(mode text)
	language plpgsql
as $$
	begin
		if mode = 'create' then
			-- _cbi_cache_handler_state
			create table if not exists public._cbi_cache_handler_state (
			  id serial primary key,
			  table_name text,
			  last_tx_id bigint,
			  last_processed_epoch_no bigint,
			  last_processed_block_no bigint,
			  unique(table_name)
			);
		
			-- _cbi_asset_cache
			create table if not exists public._cbi_asset_cache (
			  asset_id bigint primary key not null,
			  creation_time timestamp,
			  total_supply numeric,
			  mint_cnt bigint,
			  burn_cnt bigint,
			  first_mint_tx_id bigint,
			  first_mint_tx_hash text,
			  first_mint_keys text[],
			  last_mint_tx_id bigint,
			  last_mint_tx_hash text,
			  last_mint_keys text[]
			);
		
			create index if not exists _cbi_ac_idx_first_mint_tx_id on public._cbi_asset_cache (first_mint_tx_id);
			create index if not exists _cbi_ac_idx_last_mint_tx_id on public._cbi_asset_cache (last_mint_tx_id);
		
			-- _cbi_asset_addresses_cache
			create table if not exists public._cbi_asset_addresses_cache (
			  asset_id bigint,
			  address varchar,
			  quantity numeric NOT null,
			  primary key (asset_id, address)
			);
		
			create index if not exists _cbi_aac_idx_1 on public._cbi_asset_addresses_cache (asset_id);
		
			-- _cbi_active_stake_cache_epoch
			create table if not exists public._cbi_active_stake_cache_epoch (
			  epoch_no bigint not null,
			  amount lovelace not null,
			  primary key (epoch_no)
			);
		
			-- _cbi_active_stake_cache_pool
			create table if not exists public._cbi_active_stake_cache_pool (
			  pool_id varchar not null,
			  epoch_no bigint not null,
			  amount lovelace not null,
			  primary key (pool_id, epoch_no)
			);
		
			-- _cbi_active_stake_cache_account
			create table if not exists public._cbi_active_stake_cache_account (
			  stake_address varchar not null,
			  pool_id varchar not null,
			  epoch_no bigint not null,
			  amount lovelace not null,
			  primary key (stake_address, pool_id, epoch_no)
			);
		
			-- _cbi_stake_distribution_cache	
			create table if not exists public._cbi_stake_distribution_cache (
			  stake_address varchar primary key,
			  stake_id bigserial,
			  is_registered boolean,
			  last_reg_dereg_tx varchar,
			  last_reg_dereg_epoch_no numeric,
			  pool_id varchar,
			  delegated_since_epoch_no numeric,
			  last_deleg_tx varchar,
			  total_balance numeric,
			  utxo numeric,
			  rewards numeric,
			  withdrawals numeric,
			  rewards_available numeric
			);
		end if;
	end;
$$;

create or replace procedure public.cbi_cache_handler_remove()
	language plpgsql
as $$
	begin
		drop table _cbi_cache_handler_state;
		drop table _cbi_asset_cache;
		drop table _cbi_asset_addresses_cache;
		drop table _cbi_active_stake_cache_pool;
		drop table _cbi_active_stake_cache_epoch;
		drop table _cbi_active_stake_cache_account;
		drop table _cbi_stake_distribution_cache;
	end;
$$;

drop table _cbi_stake_distribution_cache;


call public.cbi_cache_handler_remove();
call public.cbi_cache_handler_init('create');

select * from _cbi_cache_handler_state;
select * from _cbi_asset_cache;
select * from _cbi_asset_addresses_cache;
select * from _cbi_active_stake_cache_pool;
select * from _cbi_active_stake_cache_epoch;
select * from _cbi_active_stake_cache_account;
select * from _cbi_stake_distribution_cache;

insert into "_cbi_cache_handler_state"(table_name, last_tx_id) values('toto',123);

delete from _cbi_cache_handler_state where table_name = 'toto';