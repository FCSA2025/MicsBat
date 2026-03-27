/* Step #1 */

IF OBJECT_ID(N'hulme.ofm_tfee1', N'U') IS NOT NULL
	drop table hulme.ofm_tfee1;
IF OBJECT_ID(N'hulme.ofm_tfee2', N'U') IS NOT NULL
	drop table hulme.ofm_tfee2;	
--IF OBJECT_ID('hulme.ofm_tfee1', 'U') IS NOT NULL
--	drop table hulme.ofm_tfee1;
IF OBJECT_ID(N'hulme.ofm_tfeecode', N'U') IS NOT NULL
	drop table hulme.ofm_tfeecode;
--IF OBJECT_ID('hulme.ofm_tfee2', 'U') IS NOT NULL
--	drop table hulme.ofm_tfee2;
IF OBJECT_ID(N'hulme.ofm_tfeecodees', N'U') IS NOT NULL
	drop table hulme.ofm_tfeecodees;
IF OBJECT_ID(N'hulme.ofm_tfee1es', N'U') IS NOT NULL
	drop table hulme.ofm_tfee1es;
IF OBJECT_ID(N'hulme.ofm_tfee2es', N'U') IS NOT NULL
drop table hulme.ofm_tfee2es;
--IF OBJECT_ID('hulme.ofm_tfee1es', 'U') IS NOT NULL
--	drop table hulme.ofm_tfee1es;
--IF OBJECT_ID('hulme.ofm_tfee2es', 'U') IS NOT NULL
--	drop table hulme.ofm_tfee2es;
IF OBJECT_ID(N'hulme.ofm_tfeevalue', N'U') IS NOT NULL
	drop table hulme.ofm_tfeevalue;
IF OBJECT_ID(N'hulme.ofm_tfee_extra', N'U') IS NOT NULL
	drop table hulme.ofm_tfee_extra;

/* Step #2 */
-- Populate table ofm_tfee1 with partial TS site data of FCSA members.
select oper, call1 
into hulme.ofm_tfee1
from main.mt_site
where mt_site.oprtyp='FT';

/* Step #3 */
-- Populate table ofm_tfee2 with partial TS site and channel data for
-- FCSA members ONLY where the feetx and/or feerx fields equal 'X'.
select oper, ofm_tfee1.call1, call2, chid, feetx, feerx, traftx, trafrx
into hulme.ofm_tfee2
from hulme.ofm_tfee1, main.mt_chan
where main.mt_chan.call1=hulme.ofm_tfee1.call1 and 
(mt_chan.feetx='X' or mt_chan.feerx='X');

/* Step #4 */

-- Replicate table ofm_tfee2 as ofm_tfeecode.
-- The TS intermediate results table is hulme.ofm_tfeecode.
select * 
into hulme.ofm_tfeecode 
from hulme.ofm_tfee2;

-- Repeat steps #2, #3, #4 for ES site and channel data, again 
-- restricted to records where feetx and/or feerx = 'X'.

/* Step #5 */

select oper, call1=me_site.location
into hulme.ofm_tfee1es
from main.me_site
where oprtyp='FE';

/* Step #6 */

select hulme.ofm_tfee1es.oper, hulme.ofm_tfee1es.call1, call2=me_chan.call1, chid, feetx, feerx, traftx, trafrx
into hulme.ofm_tfee2es
from hulme.ofm_tfee1es, main.me_chan
where me_chan.location =hulme.ofm_tfee1es.call1 and
(me_chan.feetx='X' or me_chan.feerx='X');

/* Step #7 */
-- The ES intermediate results table is hulme.ofm_tfeecodees.
select * 
into hulme.ofm_tfeecodees
from hulme.ofm_tfee2es;

/* Step #8 */
-- The records in the ES intermediate results table (hulme.ofm_tfeecodees)
-- are inserted into the TS intermediate results table (hulme.ofm_tfeecode).
-- Table hulme.ofm_tfeecode cannot differentiate between TS and ES contributions.

-- AH: REMOVE
-- This discards all the ES-related data.
-- Also, clear the results accumulator table hulme.ofm_feecalxreplace
--TRUNCATE TABLE hulme.ofm_tfeecodees;
--IF OBJECT_ID(N'hulme.ofm_feecalxreplace',N'U') IS NOT NULL TRUNCATE TABLE hulme.ofm_feecalxreplace

insert hulme.ofm_tfeecode 
select *
from hulme.ofm_tfeecodees;

/* Step #9 */
-- For the fields where feetx = 'X' we look-up the 'correct'
-- fee code (fee_factor) from the table acct.traf_fee via the 
-- intermediate results table field 'traffic'.
update hulme.ofm_tfeecode
set feetx=acct.traf_fee.fee_factor
from acct.traf_fee
where ofm_tfeecode.traftx=acct.traf_fee.traffic and
hulme.ofm_tfeecode.feetx='X';

/* Step #10 */
-- For the fields where feerx = 'X' we look-up the 'correct'
-- fee code (fee_factor) from the table acct.traf_fee via the 
-- intermediate results table field 'traffic'.
update hulme.ofm_tfeecode
set feerx=acct.traf_fee.fee_factor
from acct.traf_fee
where hulme.ofm_tfeecode.trafrx=acct.traf_fee.traffic and
hulme.ofm_tfeecode.feerx='X';

/* Step #11 */
-- Populate the table ofm_tfeevalue from the intermediate results table
-- ofm_tfeecode with two additional columns, feetxv and feerxv.
select oper, call1, call2, chid, feetx, feerx, traftx,
cast (0.000 as decimal) as feetxv, cast (0.000 as decimal) as feerxv
into hulme.ofm_tfeevalue
from hulme.ofm_tfeecode;

/* Step #12 */
-- Set the feetxv field of the table hulme.ofm_tfeevalue with the actual
-- fee, as found from the look-up table techdef.fee_codes via the 'code'.
update hulme.ofm_tfeevalue
set feetxv=techdef.fee_codes.fee
from techdef.fee_codes
where hulme.ofm_tfeevalue.feetx=techdef.fee_codes.code;

/* Step #13 */
-- Set the feerxv field of the table hulme.ofm_tfeevalue with the actual
-- fee, as found from the look-up table techdef.fee_codes via the 'code'.
update hulme.ofm_tfeevalue
set feerxv=techdef.fee_codes.fee
from techdef.fee_codes
where hulme.ofm_tfeevalue.feerx=techdef.fee_codes.code;

/* Step #14 */
-- Create a table to be populated with aggregate records to be grouped
-- over the 'admin' and 'oper' fields.
create table hulme.ofm_tfee_extra
(date date, admin char(12), oper char(6), txfee decimal, rxfee decimal);

/* Step #15 */
-- Aggregate the fees on a per-operator basis, into the table hulme.ofm_tfee_extra.
insert hulme.ofm_tfee_extra
(date, admin, oper, txfee, rxfee)
select getdate()today, sd_oper.admin, hulme.ofm_tfeevalue.oper, 
txfee=SUM(hulme.ofm_tfeevalue.feetxv),
rxfee=SUM(hulme.ofm_tfeevalue.feerxv)
from hulme.ofm_tfeevalue, main.sd_oper
where hulme.ofm_tfeevalue.oper=main.sd_oper.oper
group by admin,hulme.ofm_tfeevalue.oper;

/* Step #16 */
-- Set the txfee and rxfee columns to zero for member's TS sites. (???)
insert hulme.ofm_tfee_extra
(date, admin, oper, txfee, rxfee)
select distinct getdate()today, main.sd_oper.admin, main.mt_site.oper, 0.000, 0.000
from main.mt_site, main.sd_oper
where main.mt_site.oprtyp='FT' and main.mt_site.oper=main.sd_oper.oper
and not exists 
(select oper from hulme.ofm_tfee_extra
where main.mt_site.oper=hulme.ofm_tfee_extra.oper);

/* Step #17 */
-- Set the txfee and rxfee columns to zero for member's ES sites. (???)
insert hulme.ofm_tfee_extra
(date, admin, oper, txfee, rxfee)
select distinct getdate()today, main.sd_oper.admin, main.me_site.oper, 0.000, 0.000
from main.me_site, main.sd_oper
where main.me_site.oprtyp='FE' and main.me_site.oper=main.sd_oper.oper
and not exists
(select oper from hulme.ofm_tfee_extra
where main.me_site.oper=hulme.ofm_tfee_extra.oper);

SELECT * FROM hulme.ofm_tfee_extra ORDER BY oper

-- AH.
-- If the results table hulme.ofm_feecalxreplace does not already exist
-- then we need to create it.
IF OBJECT_ID(N'hulme.ofm_feecalxreplace',N'U') IS NULL
	CREATE TABLE [hulme].[ofm_feecalxreplace](
		[date] [date] NULL,
		[admin] [char](12) NULL,
		[oper] [char](6) NULL,
		[txfee] [decimal](18, 0) NULL,
		[rxfee] [decimal](18, 0) NULL
	) ON [PRIMARY];

/* Step #18 */
-- Insert/accumulate the results of this script into the table
-- hulme.ofm_feecalxreplace
insert hulme.ofm_feecalxreplace
select * from hulme.ofm_tfee_extra 
order by admin;
go

SELECT * FROM hulme.ofm_feecalxreplace_TS ORDER BY OPER

