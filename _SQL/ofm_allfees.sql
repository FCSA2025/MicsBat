USE [fcsa]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

/* Step #0 */



/* Step #1 */

IF OBJECT_ID(N'hulme.ofm_feeallts',N'U') IS NOT NULL
	drop table hulme.ofm_feeallts;
IF OBJECT_ID(N'hulme.ofm_txamts',N'U') IS NOT NULL
	drop table hulme.ofm_txamts;
IF OBJECT_ID(N'hulme.ofm_rxamts',N'U') IS NOT NULL
	drop table hulme.ofm_rxamts;
IF OBJECT_ID(N'hulme.ofm_feealles',N'U') IS NOT NULL
	drop table hulme.ofm_feealles;
IF OBJECT_ID(N'hulme.ofm_txamtses',N'U') IS NOT NULL
	drop table hulme.ofm_txamtses;
IF OBJECT_ID(N'hulme.ofm_rxamtses',N'U') IS NOT NULL
	drop table hulme.ofm_rxamtses;
IF OBJECT_ID(N'hulme.ofm_tsfeetot',N'U') IS NOT NULL
	drop table hulme.ofm_tsfeetot;
IF OBJECT_ID(N'hulme.ofm_esfeetot',N'U') IS NOT NULL
	drop table hulme.ofm_esfeetot;
IF OBJECT_ID(N'hulme.ofm_feetot',N'U') IS NOT NULL
	drop table hulme.ofm_feetot;

/* Step #2 */

create table hulme.ofm_feeallts
   (date date,oper char(6),feetx char(2),txcount smallint,
      txamt int,feerx char(2),rxcount smallint,rxamt int,
      mycol uniqueidentifier rowguidcol default newid());

/* Step #3 */
-- Warning: Null value is eliminated by an aggregate or other SET operation.
insert hulme.ofm_feeallts(date,oper,feetx,txcount,txamt,feerx,
   rxcount,rxamt)
   select getdate()today,oper, feetx,txcount=count(feetx),
      txamt=0.00,
      feerx,rxcount=count(feerx),
      rxamt=0.00
   from main.mt_site, main.mt_chan
   where main.mt_site.call1=main.mt_chan.call1 and main.mt_site.oprtyp='FT'
group by oper,feetx,feerx;

/* Step #4 */

  select date,oper,feetx,txcount,
      txamt=(fee*hulme.ofm_feeallts.txcount),mycol
      into hulme.ofm_txamts
         from hulme.ofm_feeallts,techdef.fee_codes
   where techdef.fee_codes.code=hulme.ofm_feeallts.feetx;

/* Step #5 */

   select date,oper,feerx,rxcount,
      rxamt=(fee* hulme.ofm_feeallts.rxcount),mycol
      into hulme.ofm_rxamts
   from hulme.ofm_feeallts,techdef.fee_codes
   where techdef.fee_codes.code=hulme.ofm_feeallts.feerx;

/* Step #6 */

update hulme.ofm_feeallts
   set txamt=hulme.ofm_txamts.txamt
   from hulme.ofm_txamts
   where hulme.ofm_feeallts.oper=hulme.ofm_txamts.oper and
      hulme.ofm_feeallts.feetx=hulme.ofm_txamts.feetx and
      hulme.ofm_feeallts.txcount=hulme.ofm_txamts.txcount
      and hulme.ofm_feeallts.mycol=hulme.ofm_txamts.mycol;

/* Step #7 */

update hulme.ofm_feeallts
   set rxamt=hulme.ofm_rxamts.rxamt
      from hulme.ofm_rxamts
   where hulme.ofm_feeallts.mycol=hulme.ofm_rxamts.mycol;

/* Step #8 */

create table hulme.ofm_feealles
   (date date,oper char(6),feetx char(2),txcount smallint,
      txamt int, feerx char(2),rxcount smallint, rxamt int,
      mycol uniqueidentifier rowguidcol default newid());

/* Step #9 */

insert hulme.ofm_feealles (date,oper,feetx,txcount,txamt,
   feerx,rxcount,rxamt)
   select getdate()today,oper,feetx,txcount=count(feetx),
      txamt=0.00,
      feerx,rxcount=count(feerx),
      rxamt=0.00
   from main.me_site,main.me_chan
   where main.me_site.location=main.me_chan.location and main.me_site.oprtyp='FE'
group by oper,feetx,feerx;

/* Step #10 */

   select date,oper,feetx,txcount,
      txamt=(techdef.fee_codes.fee*hulme.ofm_feealles.txcount),mycol
      into hulme.ofm_txamtses
   from hulme.ofm_feealles,techdef.fee_codes
   where techdef.fee_codes.code=hulme.ofm_feealles.feetx;
   
/* Step #11 */
   
   select date,oper,feerx,rxcount,
      rxamt=(techdef.fee_codes.fee*hulme.ofm_feealles.rxcount),mycol
      into hulme.ofm_rxamtses
   from hulme.ofm_feealles,techdef.fee_codes
   where hulme.ofm_feealles.feerx=code;

/* Step #12 */

update hulme.ofm_feealles
   set txamt=hulme.ofm_txamtses.txamt
   from hulme.ofm_txamtses
    where hulme.ofm_feealles.mycol=hulme.ofm_txamtses.mycol;

/* Step #13 */

update hulme.ofm_feealles
   set rxamt=hulme.ofm_rxamtses.rxamt
      from hulme.ofm_rxamtses
   where hulme.ofm_feealles.mycol=hulme.ofm_rxamtses.mycol;
--select * from hulme.ofm_feealles order by oper,feetx,feerx;

/* Step #14 */

select date,admin,hulme.ofm_feeallts.oper,
      sum(txamt+rxamt) as tstot
      into hulme.ofm_tsfeetot
   from hulme.ofm_feeallts,main.sd_oper
   where hulme.ofm_feeallts.oper=main.sd_oper.oper
   group by date,admin,hulme.ofm_feeallts.oper;

--select * from hulme.ofm_tsfeetot;

/* Step #15 */

select date, admin, hulme.ofm_feealles.oper,
      sum(txamt+rxamt) as estot
      into hulme.ofm_esfeetot
   from hulme.ofm_feealles,main.sd_oper
   where hulme.ofm_feealles.oper=main.sd_oper.oper
   group by date,admin,hulme.ofm_feealles.oper;

--select * from hulme.ofm_esfeetot;

/* Step #16 */

select date,admin,oper,
      tstot,
      0 as estot
      into hulme.ofm_feetot
   from hulme.ofm_tsfeetot;

/* Step #17 */

 insert hulme.ofm_feetot (date,admin,oper,tstot,estot)
   values (getdate(),'TELESAT','TELS',0,0);
   
/* Step #18 */
   
update hulme.ofm_feetot
   set estot=hulme.ofm_esfeetot.estot
   from hulme.ofm_esfeetot
   where hulme.ofm_esfeetot.oper=hulme.ofm_feetot.oper;
   
--select * from hulme.ofm_feetot order by admin;

/* Step #19 */

-- If the results table hulme.ofm_feecalalltot does not already 
-- exist then create it.
IF OBJECT_ID(N'hulme.ofm_feecalalltot',N'U') IS NULL
	CREATE TABLE [hulme].[ofm_feecalalltot](
		[date] [date] NULL,
		[admin] [char](12) NULL,
		[oper] [char](6) NULL,
		[tstot] [int] NULL,
		[estot] [int] NOT NULL
	) ON [PRIMARY];

-- AH: REMOVE
--TRUNCATE TABLE hulme.ofm_feecalalltot


-- Insert/accumulate the results of this script into the table
-- hulme.ofm_feecalalltot
insert hulme.ofm_feecalalltot 
select * from hulme.ofm_feetot 
order by admin;
go

SET ANSI_PADDING OFF
GO

SELECT * FROM hulme.ofm_feecalalltot;
