-- CREATE UNIQUE INDEX IF NOT EXISTS unique_ada_pots ON public.ada_pots USING btree (block_id);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_col_txin ON public.collateral_tx_in USING btree (tx_in_id, tx_out_id, tx_out_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_col_txout ON public.collateral_tx_out USING btree (tx_id, index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_delegation ON public.delegation USING btree (tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_epoch_param ON public.epoch_param USING btree (epoch_no, block_id);

CREATE UNIQUE INDEX IF NOT EXISTS unique_ma_tx_mint ON public.ma_tx_mint USING btree (ident, tx_id);
CREATE UNIQUE INDEX IF NOT EXISTS unique_ma_tx_out ON public.ma_tx_out USING btree (ident, tx_out_id);
CREATE INDEX IF NOT EXISTS idx_ma_tx_out_ident ON ma_tx_out (ident) ;

CREATE UNIQUE INDEX idx_multi_asset_fingerprint ON public.multi_asset USING btree (fingerprint);


-- CREATE UNIQUE INDEX IF NOT EXISTS unique_param_proposal ON public.param_proposal USING btree (key, registered_tx_id);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_pool_owner ON public.pool_owner USING btree (addr_id, pool_update_id);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_pool_relay ON public.pool_relay USING btree (update_id, ipv4, ipv6, dns_name);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_pool_retiring ON public.pool_retire USING btree (announced_tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_ref_tx_in ON reference_tx_in USING btree (tx_in_id, tx_out_id, tx_out_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_pool_update ON public.pool_update USING btree (registered_tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_pot_transfer ON public.pot_transfer USING btree (tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_redeemer ON public.redeemer USING btree (tx_id, purpose, index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_reserves ON public.reserve USING btree (addr_id, tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_stake_deregistration ON public.stake_deregistration USING btree (tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_stake_registration ON public.stake_registration USING btree (tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_treasury ON public.treasury USING btree (addr_id, tx_id, cert_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_txin ON tx_in USING btree (tx_out_id, tx_out_index);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_tx_metadata ON public.tx_metadata USING btree (key, tx_id);
-- CREATE UNIQUE INDEX IF NOT EXISTS unique_withdrawal ON public.withdrawal USING btree (addr_id, tx_id);
