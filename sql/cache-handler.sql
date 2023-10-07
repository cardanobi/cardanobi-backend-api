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
			--   stake_address varchar not null,
			--   pool_id varchar not null,
			  stake_address_id int8 not null,
			  pool_hash_id int8 not null,
			  epoch_no int8 not null,
			  amount lovelace default 0,
			  primary key (stake_address_id, pool_hash_id, epoch_no)
			);

			create index if not exists _cbi_casca_idx_stake_address_epoch_no on public._cbi_active_stake_cache_account (stake_address_id, epoch_no);
			create index if not exists _cbi_casca_idx_pool_id_epoch_no on public._cbi_active_stake_cache_account (pool_hash_id, epoch_no);

			-- _cbi_poolstats_cache
			create table if not exists public."_cbi_pool_stats_cache" (
				epoch_no int8 not null,
				pool_hash_id int8 not null,
				delegator_count int8 default 0,
				delegated_stakes int8 default 0,
				tx_count int8 default 0,
				block_count int8 default 0,
				constraint "_cbi_pool_stats_cache_unique" PRIMARY KEY (epoch_no, pool_hash_id)
			);
			create index idx_cbi_pool_stats_pool_hash_id ON public._cbi_pool_stats_cache USING btree (pool_hash_id);
		
			-- _cbi_stake_distribution_cache	
			create table if not exists public._cbi_stake_distribution_cache (
			--   stake_address varchar primary key,
			  stake_address_id int8 primary key,
			  is_registered boolean,
			  last_reg_dereg_tx varchar,
			  last_reg_dereg_epoch_no numeric,
			--   pool_id varchar,
			  pool_hash_id int8,
			  delegated_since_epoch_no numeric,
			  last_deleg_tx varchar,
			  total_balance numeric,
			  utxo numeric,
			  rewards numeric,
			  withdrawals numeric,
			  rewards_available numeric
			);

			create index if not exists _cbi_casca_idx_pool_hash_id on public._cbi_stake_distribution_cache (pool_hash_id);

			create index if not exists _cbi_casca_idx_stake_address_id_pool_hash_id on public._cbi_stake_distribution_cache (stake_address_id, pool_hash_id);

			-- _cbi_address_info_cache	
			create table if not exists public._cbi_address_info_cache (
			  address varchar,
			  stake_address_id int8,
			  stake_address varchar,
			  script_hash text
			);

			CREATE UNIQUE INDEX _cbi_address_info_cache_1 ON public._cbi_address_info_cache USING btree (address);
			CREATE INDEX _cbi_address_info_cache_2 ON public._cbi_address_info_cache USING btree (stake_address);
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
select * from _cbi_pool_stats;

insert into "_cbi_cache_handler_state"(table_name, last_tx_id) values('toto',123);

delete from _cbi_cache_handler_state where table_name = 'toto';



drop table _cbi_active_stake_cache_account;

create table if not exists public._cbi_active_stake_cache_account (
--   stake_address varchar not null,
--   pool_id varchar not null,
	stake_address_id int8 not null,
	pool_hash_id int8 not null,
	epoch_no int8 not null,
	amount lovelace default 0,
	primary key (stake_address_id, pool_hash_id, epoch_no)
);

create index if not exists _cbi_casca_idx_stake_address_epoch_no on public._cbi_active_stake_cache_account (stake_address_id, epoch_no);
create index if not exists _cbi_casca_idx_pool_id_epoch_no on public._cbi_active_stake_cache_account (pool_hash_id, epoch_no);

select * from _cbi_active_stake_cache_account;

select * from _cbi_stake_distribution_cache limit 10;