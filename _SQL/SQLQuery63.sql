USE fcsa

IF OBJECT_ID('hulme.temp') IS NOT NULL DROP TABLE hulme.temp
SELECT AY.call1, AY.call2, AY.bndcde, AY.anum, AX.bndcde AS bndcdeX, AX.anum AS anumX
    INTO hulme.temp
    FROM hulme.mt_ante AS AX
	INNER JOIN hulme.mt_ante AS AY ON (AX.call1=AY.call1 AND AX.call2=AY.call2 AND AX.bndcde='11A' AND AY.bndcde='11B' /*AND AX.anum=AY.anum*/)

SELECT * FROM hulme.temp ORDER BY call1, call2, bndcde, anum

INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CGV837', 'CGY624', '11A', '99')
INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CGV837', 'CGY624', '11A', '1')
INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CGV837', 'CGY624', '11A', '2')
INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CGV837', 'CGY624', '11A', '3')
INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CGV837', 'CGY624', '11A', '5')

INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CIJ526', 'CIJ525', '11A', '50')
INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CIJ526', 'CIJ525', '11A', '3')
INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CIJ526', 'CIJ525', '11A', '4')
INSERT INTO hulme.mt_ante (call1, call2, bndcde, anum) VALUES ('CIJ526', 'CIJ525', '11A', '13')
