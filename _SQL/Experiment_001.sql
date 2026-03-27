USE micsdev
GO

IF OBJECT_ID('tempdb..#tempTable') IS NOT NULL DROP TABLE #tempTable

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO

CREATE TABLE #tempTable(
	[call1] [char](9) NOT NULL,
	[call2] [char](9) NOT NULL,
	[bndcde] [char](4) NOT NULL,
	[chid] [char](4) NOT NULL,
	[freqtx] [float] NULL,
	[antnumbtx1] [tinyint] NULL,
	[fomTx] [int] NULL, 
	[licenseTx] [char](63) NULL, 
	[freqrx] [float] NULL,
	[antnumbrx1] [tinyint] NULL,
	[fomRx] [int] NULL, 
	[licenseRx] [char](63) NULL, 
 CONSTRAINT [PK_AH_20230817] PRIMARY KEY CLUSTERED 
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

INSERT INTO #tempTable
	(call1, call2, bndcde, chid)
	SELECT DISTINCT m_call1, m_call2, m_bndcde, m_chid
	--FROM hulme.LinkMatch_TaflToMdb_20230628
	FROM hulme.LinkMatch_MdbToTafl_20230628
	WHERE x_LAF = 'LAF'

UPDATE #tempTable
	SET 
		freqTx = m_freqTx,
		antnumbTx1 = m_anum,
		fomTx = x_FOM,
		licenseTx = t_AuthorizationNumber
	--FROM hulme.LinkMatch_TaflToMdb_20230628
	FROM hulme.LinkMatch_MdbToTafl_20230628
	WHERE (call1 = m_call1) AND (call2 = m_call2) AND (bndcde = m_bndcde) AND (chid = m_chid) 
		AND (m_TxRx = 'TX') AND (x_LAF = 'LAF')

UPDATE #tempTable
	SET 
		freqRx = m_freqRx,
		antnumbRx1 = m_anum,
		fomRx = x_FOM,
		licenseRx = t_AuthorizationNumber
	--FROM hulme.LinkMatch_TaflToMdb_20230628
	FROM hulme.LinkMatch_MdbToTafl_20230628
	WHERE (call1 = m_call1) AND (call2 = m_call2) AND (bndcde = m_bndcde) AND (chid = m_chid) 
		AND (m_TxRx = 'RX') AND (x_LAF = 'LAF')

SELECT * FROM #tempTable
	WHERE licenseTx != licenseRx

SELECT * FROM #tempTable
	WHERE SUBSTRING(licenseTx, 1, 9) != SUBSTRING(licenseRx, 1, 9)