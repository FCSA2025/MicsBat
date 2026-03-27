-----------------------------------------------------------------------------------
-- This T-SQL script makes 'local' copies of the MDB TS tables mt_ante and mt_chan.
-----------------------------------------------------------------------------------

USE fcsa;
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO

IF OBJECT_ID('hulme.mt_ante') IS NOT NULL DROP TABLE hulme.mt_ante
IF OBJECT_ID('hulme.mt_chan') IS NOT NULL DROP TABLE hulme.mt_chan
GO

CREATE TABLE [hulme].[mt_ante](
	[call1] [char](9) NOT NULL,
	[call2] [char](9) NOT NULL,
	[bndcde] [char](4) NOT NULL,
	[anum] [smallint] NOT NULL,
	[ause] [char](3) NULL,
	[acode] [char](12) NULL,
	[aht] [real] NULL,
	[azmth] [real] NULL,
	[elvtn] [real] NULL,
	[dist] [real] NULL,
	[offazm] [char](1) NULL,
	[tazmth] [real] NULL,
	[telvtn] [real] NULL,
	[tgain] [real] NULL,
	[txfdlnth] [char](2) NULL,
	[txfdlnlh] [real] NULL,
	[txfdlntv] [char](2) NULL,
	[txfdlnlv] [real] NULL,
	[rxfdlnth] [char](2) NULL,
	[rxfdlnlh] [real] NULL,
	[rxfdlntv] [char](2) NULL,
	[rxfdlnlv] [real] NULL,
	[txpadpam] [real] NULL,
	[rxpadlna] [real] NULL,
	[txcompl] [real] NULL,
	[rxcompl] [real] NULL,
	[obsloss] [real] NULL,
	[kvalue] [real] NULL,
	[atwrno] [tinyint] NULL,
	[nota] [char](4) NULL,
	[apoint] [char](4) NULL,
	[sdate] [char](10) NULL,
	[mdate] [char](10) NULL,
	[mtime] [char](8) NULL,
	[userid] [char](12) NULL,
	[licence] [char](13) NULL,
 CONSTRAINT [PK_mt_ante] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[anum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [hulme].[mt_chan](
	[call1] [char](9) NOT NULL,
	[call2] [char](9) NOT NULL,
	[bndcde] [char](4) NOT NULL,
	[splan] [char](4) NULL,
	[hl] [tinyint] NULL,
	[vh] [tinyint] NULL,
	[chid] [char](4) NOT NULL,
	[freqtx] [float] NULL,
	[poltx] [char](1) NULL,
	[antnumbtx1] [tinyint] NULL,
	[antnumbtx2] [tinyint] NULL,
	[eqpttx] [char](8) NULL,
	[eqptutx] [char](1) NULL,
	[pwrtx] [real] NULL,
	[atpccde] [real] NULL,
	[afsltx1] [real] NULL,
	[afsltx2] [real] NULL,
	[traftx] [char](6) NULL,
	[srvctx] [char](6) NULL,
	[stattx] [char](1) NULL,
	[freqrx] [float] NULL,
	[polrx] [char](1) NULL,
	[antnumbrx1] [tinyint] NULL,
	[antnumbrx2] [tinyint] NULL,
	[antnumbrx3] [tinyint] NULL,
	[eqptrx] [char](8) NULL,
	[eqpturx] [char](1) NULL,
	[afslrx1] [real] NULL,
	[afslrx2] [real] NULL,
	[afslrx3] [real] NULL,
	[pwrrx1] [real] NULL,
	[pwrrx2] [real] NULL,
	[pwrrx3] [real] NULL,
	[trafrx] [char](6) NULL,
	[esint] [real] NULL,
	[tsint] [real] NULL,
	[srvcrx] [char](6) NULL,
	[statrx] [char](1) NULL,
	[routnumb] [char](8) NULL,
	[stnnumb] [tinyint] NULL,
	[hopnumb] [tinyint] NULL,
	[sdate] [char](10) NULL,
	[notetx] [char](4) NULL,
	[noterx] [char](4) NULL,
	[notegnl] [char](4) NULL,
	[cpoint] [char](4) NULL,
	[feetx] [char](2) NULL,
	[feerx] [char](2) NULL,
	[mdate] [char](10) NULL,
	[mtime] [char](8) NULL,
	[userid] [char](12) NULL,
 CONSTRAINT [PK_mt_chan] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[chid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

------------------------------------------------
-- Populate hulme.mt_ante with required records.
------------------------------------------------
INSERT INTO hulme.mt_ante 
	SELECT * FROM fcsaSQL3.fcsa.main.mt_ante /*WHERE (bndcde IN ('6B', '7A', '11B', '18A')) OR (bndcde IN ('6A', '6D', '6E', '11A', '18B'))*/

------------------------------------------------
-- Populate hulme.mt_chan with required records.
------------------------------------------------
INSERT INTO hulme.mt_chan 
	SELECT * FROM fcsaSQL3.fcsa.main.mt_chan /*WHERE (bndcde IN ('6B', '7A', '11B', '18A')) OR (bndcde IN ('6A', '6D', '6E', '11A', '18B'))*/

SELECT 'Local copies of mt_ante and mt_chan created.'

