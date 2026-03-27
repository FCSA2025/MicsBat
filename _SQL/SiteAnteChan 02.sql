SELECT chanA.call1, chanA.call2, siteA.oper, siteB.oper, siteA.prov, siteB.prov, chanA.bndcde, chanB.bndcde, chanA.chid, chanB.chid, anteA.ause, anteB.ause, chanA.freqtx, chanB.freqrx, anteA.apoint, anteB.apoint, chanA.freqrx, chanB.freqtx, chanA.feetx, chanA.feerx, chanB.feetx, chanB.feerx, chanA.traftx, chanA.trafrx, chanB.traftx, chanB.trafrx
    FROM main.mt_chan chanA
       JOIN main.mt_chan chanB ON (chanA.call1 = chanB.call2) AND (chanA.call2 = chanB.call1) /*AND (chanA.bndcde = chanB.bndcde)*/ AND ((chanA.freqtx = chanB.freqrx) OR (chanA.freqrx = chanB.freqtx)) --(chanA.chid = chanB.chid)
       JOIN main.mt_ante anteA ON (anteA.call1 = chanA.call1) AND (anteA.call2 = chanA.call2) AND (anteA.bndcde = chanA.bndcde)
       JOIN main.mt_ante anteB ON (anteB.call1 = chanB.call1) AND (anteB.call2 = chanB.call2) AND (anteB.bndcde = chanB.bndcde)
       JOIN main.mt_site siteA ON (siteA.call1 = chanA.call1)
       JOIN main.mt_site siteB ON (siteB.call1 = chanB.call1)
       WHERE
           -- No ficticious call signs.
                     LEFT(chanA.call1, 1) NOT IN ('=', '$', '%', ';')
              AND LEFT(chanA.call2, 1) NOT IN ('=', '$', '%', ';')
              -- At least one end in Canada or SP.
              AND ((siteA.prov IN ('YT','NW','BC','AB','SK','MB','ON','QC','NB','PE','NS','NL','NU', 'SP')) OR (siteB.prov IN ('YT','NW','BC','AB','SK','MB','ON','QC','NB','PE','NS','NL','NU', 'SP')))
              -- At least one end is a FCSA member.
              AND ((siteA.oper IN (SELECT oper FROM adm.account_ids)) OR (siteB.oper IN (SELECT oper FROM adm.account_ids)))
              --AND chanA.chid != chanB.chid
              --AND chanA.bndcde != chanB.bndcde
              --AND anteA.apoint = 'I19A'
              --AND anteB.apoint = 'I19A'
              --AND (chanA.feetx = 'X' OR  chanA.feerx = 'X' OR  chanB.feetx = 'X' OR  chanB.feerx = 'X')
			  --AND (chanA.call1 = 'VDE895' AND chanB.call1 = 'XKX903')
			  AND 
       ORDER BY chanA.call1, chanA.call2, chanA.bndcde, chanA.chid

SELECT * FROM main.mt_chan WHERE (call1 = 'CF0722' AND call2 = 'CF0723')
SELECT * FROM main.mt_chan WHERE (call2 = 'CF0722' AND call1 = 'CF0723')
