USE micsdev
GO

-- Simulate an instance of member-entered license data for a Tx channel component.
UPDATE main.sd_licn
	SET
		tx_licence = '666666666-666',
		tx_provenance = 'WEBMICS_MEMBER_ENTERED',
		tx_mdate = '2023.12.31'
	WHERE call1 = 'CFA674' AND call2 = 'VEL551' AND bndcde = '78A' AND chid = '1001'

-- Simulate an instance of member-entered license data for a Rx channel component.
UPDATE main.sd_licn
	SET
		rx_licence = '123456789-123',
		rx_provenance = 'WEBMICS_MEMBER_ENTERED',
		rx_mdate = '2023.11.05'
	WHERE call1 = 'VAD403' AND call2 = '=RCTL8624' AND bndcde = '13A' AND chid = '1001'
	
