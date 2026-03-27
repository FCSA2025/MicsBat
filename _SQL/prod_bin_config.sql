SELECT * FROM micsdev.hulme.prod_bin_config ORDER BY fileName, sortDate DESC

SELECT * FROM micsdev.hulme.prod_bin_config GROUP BY sortDate ORDER BY fileName

SELECT * FROM hulme.prod_bin_config WHERE description LIKE '%licence%' ORDER BY fileName, sortDate DESC

DELETE FROM hulme.prod_bin_config WHERE fileName = 'TsipInitiator.exe.config'

DROP TABLE hulme.temp_z
SELECT fileName, MAX(sortDate) AS sortDate INTO hulme.temp_z FROM micsdev.hulme.prod_bin_config GROUP BY fileName ORDER BY fileName

SELECT * FROM hulme.temp_z

SELECT * 
	FROM micsdev.hulme.prod_bin_config AS A
	INNER JOIN hulme.temp_z AS B ON (A.fileName = B.fileName) AND (A.sortDate = B.sortDate)
	ORDER BY A.sortDate DESC

--SELECT * FROM hulme.prod_bin_config_baseline

DROP TABLE hulme.prod_bin_config

EXEC sp_rename 'hulme.prod_bin_config.lastModDate', 'lastModified', 'COLUMN';

UPDATE hulme.prod_bin_config SET description='Baseline established on 20210427.'

CREATE TABLE [hulme].[prod_bin_config] ( [fileName] [varchar](40) NOT NULL, [custodian] [varchar](20) NOT NULL, [lastModified] [varchar](19) NOT NULL, [sortDate] [varchar](8) NOT NULL, [md5] [varchar](32) NOT NULL, [description] [varchar](1000) NOT NULL, CONSTRAINT [PK_ProdBinConfig] PRIMARY KEY CLUSTERED  ( [fileName] ASC, [md5] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY] ) ON [PRIMARY];

INSERT INTO [hulme].[prod_bin_config] SELECT * FROM hulme.prod_bin_config_baseline


INSERT INTO hulme.prod_bin_config VALUES ( 'MtUpdate.exe','HULME','9/26/2021 1:04 PM','20210926','A65F16E224D666A76044AE3F3C0A2062','Queuing maximum wait time raised to 120 seconds.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'FePrint.exe','HULME','9/26/2021 1:04 PM','20210926','6BFBBD2D20CDBD6F7BDDC977C71E76BB','Added command-line option to write output to prescribed file path.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','9/26/2021 1:04 PM','20210926','5BA2920448172638E20389E2CAE7A011','Fix for DB READ/WRITE access queuing #1079. Enhancement of HiLo analysis #1021. Change to enable PFDcont to use new email setup.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','9/26/2021 1:04 PM','20210926','CB6FD255B6DE21638D09C8E042EBEF96','Fix for DB READ/WRITE access queuing #1079. Enhancement of HiLo analysis #1021. Change to enable PFDcont to use new email setup.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','9/26/2021 1:04 PM','20210926','A352449B2FCC6246AB5D3695A64FD594','Fix for DB READ/WRITE access queuing #1079. Enhancement of HiLo analysis #1021. Change to enable PFDcont to use new email setup.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','9/26/2021 1:04 PM','20210926','C0CFDEB54CA1F4D7ECFCE792A340237C','Fix for DB READ/WRITE access queuing #1079. Enhancement of HiLo analysis #1021. Change to enable PFDcont to use new email setup.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','9/26/2021 1:04 PM','20210926','5AAA34421B6936C0B9F28583C07435F4','Fix for DB READ/WRITE access queuing #1079. Enhancement of HiLo analysis #1021. Change to enable PFDcont to use new email setup.' )


INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','10/13/2021 11:59 AM','20211013','6D9E5B5DF6DBE2FC8A43BC9D0E9BC47E','Re-release. Same functionality as 20210926.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','10/13/2021 11:58 AM','20211013','1BD996494E7ADFE220F6F561785D6E3A','Re-release. Same functionality as 20210926.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','10/13/2021 11:58 AM','20211013','0DA198AC8B978E4D71FB1E01B0FBE390','Re-release. Same functionality as 20210926.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','10/13/2021 11:58 AM','20211013','25309A3C9481AAB37723B1BC9170D2E9','Re-release. Same functionality as 20210926.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','10/13/2021 11:58 AM','20211013','718128F7A7157B7E4C3CA00F2AE7D6C1','Default HiLo mode set to OLD while bug in 20210926 is fixed.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'MtUpdate.exe','HULME','10/13/2021 11:59 AM','20211013','70671F33AE5E453712D79F61B29FA7EB','Re-release. Same functionality as 20210926.' )


INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','10/15/2021 1:29 PM','20211015','00A8D8B229F2001F976B3FBCDEEA6881','Explicit selection of TLS1.2 Security Protocol to send email via Office 365 SMTP server.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','10/15/2021 1:29 PM','20211015','A133F14A569B228E46A7DA4821B89904','Explicit selection of TLS1.2 Security Protocol to send email via Office 365 SMTP server.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','10/15/2021 1:29 PM','20211015','F7758E4DF9CD8518F33BB52210F973A6','Explicit selection of TLS1.2 Security Protocol to send email via Office 365 SMTP server.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','10/15/2021 1:29 PM','20211015','C769E0443FA56E0303AB8042A57C3AA7','Explicit selection of TLS1.2 Security Protocol to send email via Office 365 SMTP server.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','10/15/2021 1:29 PM','20211015','7DA1EF381B1125CAEC1C4D6091124D9E','Explicit selection of TLS1.2 Security Protocol to send email via Office 365 SMTP server.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'TsipInitiator.exe','HULME','10/15/2021 1:29 PM','20211015','E4D9AB7AE36F58401FEC4A311A174FFA','Improved TSIP log messages. Repeated attempts to send email (max. 5 times)' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','10/27/2021 3:30 PM','20211027','F99D9F6BC9678CA0826940717F2D1B31','More detailed description of email send failure written to TSIP log.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','10/27/2021 3:30 PM','20211027','79FA19F46778EDAC6E4DF257F11C5C28','More detailed description of email send failure written to TSIP log.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','10/27/2021 3:30 PM','20211027','194D8DD3EE94A4469144A08BC8D49AED','More detailed description of email send failure written to TSIP log.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','10/27/2021 3:30 PM','20211027','5193088F5DB284D040B232E74E374AED','More detailed description of email send failure written to TSIP log.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','10/27/2021 3:30 PM','20211027','132B774B98492114FED0A9D9E609E662','More detailed description of email send failure written to TSIP log.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'TsipInitiator.exe','HULME','10/27/2021 3:30 PM','20211027','A25A21767DA6D7CE24CE23B1EEE6DEA2','More detailed description of email send failure written to TSIP log.' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','11/1/2021 4:30 PM','20211101','3FB6A971BD043DB78740A29CAD89273E','Enable the user to specify any database name on the command line, not just fcsa or test.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','11/1/2021 4:30 PM','20211101','C622B563422FCFF628D9E4AD2F5DFF0B','Enable the user to specify any database name on the command line, not just fcsa or test.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','11/1/2021 4:30 PM','20211101','E97ED4CEA2D5A2C9779040461151D62A','Enable the user to specify any database name on the command line, not just fcsa or test.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','11/1/2021 4:30 PM','20211101','096DC91C69716EA07E23EE722EB4A7B4','Enable the user to specify any database name on the command line, not just fcsa or test.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','11/1/2021 4:30 PM','20211101','83AEED85E08D7BE7174FCE9BA8D5E4A0','Enable the user to specify any database name on the command line, not just fcsa or test.' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','11/4/2021 12:53 PM','20211104','E7892CD951DC474AE7E968A5DE48215E','Bug fix: email address syntax checking now copes with upper and lower case.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','11/4/2021 12:53 PM','20211104','E05254FA8888D5C565189396BB66240B','Bug fix: email address syntax checking now copes with upper and lower case.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','11/4/2021 12:53 PM','20211104','E888AAE78B3C94E1C54EB56A94141884','Bug fix: email address syntax checking now copes with upper and lower case.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','11/4/2021 12:53 PM','20211104','D86002F6CDC2C186FDA92D401AB1C85B','Bug fix: email address syntax checking now copes with upper and lower case.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','11/4/2021 12:53 PM','20211104','6FA2968E1AF86AA2508A162DAAD79063','Bug fix: email address syntax checking now copes with upper and lower case.' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','11/8/2021 2:29 PM','20211108','63F5204A0F02EFAF04CB9722D7C76A66','Make new HiLo available to selected micsid volunteers.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','11/8/2021 2:29 PM','20211108','F6B5D1BF5812782E7A4760209780C2D4','Make new HiLo available to selected micsid volunteers.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','11/8/2021 2:29 PM','20211108','D1DC97C218B1CB9F6C855D3FE492DFDD','Make new HiLo available to selected micsid volunteers.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','11/8/2021 2:29 PM','20211108','5ACC1E3DD46692632F1D34BBCF557124','Make new HiLo available to selected micsid volunteers.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','11/8/2021 2:29 PM','20211108','D4F3BB47DAE738D430991726A6CBED86','Make new HiLo available to selected micsid volunteers.' )

INSERT INTO hulme.prod_bin_config VALUES ( 'FtValidate.exe','HULME','11/15/2021 1:58 PM','20211115','6C48B63DD6F11BB2A522BA2B0B8B7345','Bug fix b191119A: checking of licences commented out.' )

INSERT INTO hulme.prod_bin_config VALUES ( 'SetEmailPassword.exe','HULME','12/21/2021 1:13 PM','20211221','66A6BC3CFB2D55B96CA1D51CA8CDF04A','Enhanced GUI and logic. Instructions visible in main dialog.' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','1/7/2022 5:09 PM','20220107','86DE9EB3D70BBF15717011C995B1A335','Temporary update to write context data to TSIP log file to investigate dbo and schema problems.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','1/7/2022 5:09 PM','20220107','EA3B8685C41DD3E9820D7EE5EC56AFC7','Temporary update to write context data to TSIP log file to investigate dbo and schema problems.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','1/7/2022 5:09 PM','20220107','A562C3A162186D389E6E47D292518C71','Temporary update to write context data to TSIP log file to investigate dbo and schema problems.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','1/7/2022 5:09 PM','20220107','3ADB80FAB79D192F3AE16D69F809CF92','Temporary update to write context data to TSIP log file to investigate dbo and schema problems.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','1/7/2022 5:09 PM','20220107','F5316AB86AAA753B48A9846C600AAB81','Temporary update to write context data to TSIP log file to investigate dbo and schema problems.' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','6/9/2022 3:01 PM','20220609','3773650A620EA35C467CF4531191D7C2','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','6/9/2022 3:01 PM','20220609','F595E6DEC515888B7E8E6543BC16C30B','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','6/9/2022 3:01 PM','20220609','ACB061C35D590A51E7E6B5AD71414EC9','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','6/9/2022 3:01 PM','20220609','D654E0E0E3F57B04756ACB8B3DEAC518','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','6/9/2022 3:01 PM','20220609','4CE55B35AA5E665CB9CDCBB42948B184','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'FtImport.exe','HULME','6/9/2022 3:01 PM','20220609','624220A57FD510F72C9E87DE1D0B7304','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'FtValidate.exe','HULME','6/9/2022 3:01 PM','20220609','0E804383EBB34650841C0D54DD31E7D2','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'MtUpdate.exe','HULME','6/9/2022 3:01 PM','20220609','EC13ADBDABFBE94E856A3B3AED7DB9EF','Update to fix schema issue for FCSA staff.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'pfdcont.exe','HULME','9/20/2021 4:51 PM','20210920','4F84E9EE825CC4A9115BEE49F1AE74C6','No functional change.' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','9/20/2022 2:43 PM','20220920','217283D680611831C83B8786715FA747','All libraries updated to support implicit Log2.x(String.Format()) capability.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','9/20/2022 2:43 PM','20220920','5BA9444FCED11868AB10FEADF9DB5B0E','All libraries updated to support implicit Log2.x(String.Format()) capability.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','9/20/2022 2:43 PM','20220920','81AB6ED32F6B0CEE6DDC5DE1D2EDE674','All libraries updated to support implicit Log2.x(String.Format()) capability.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','9/20/2022 2:43 PM','20220920','715478B250B24D1C35E582F4C8A8CB3D','All libraries updated to support implicit Log2.x(String.Format()) capability.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','9/20/2022 2:43 PM','20220920','E9DE47955B93C6C67141449F8BFD9FE1','All libraries updated to support implicit Log2.x(String.Format()) capability.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'FeImport.exe','HULME','9/20/2022 2:43 PM','20220920','716E07E3B53EE4E6E82A53312617B9DB','Updated to ignore missing AT and AR mandatory fields for AK cmd = ADD.' )

INSERT INTO hulme.prod_bin_config VALUES ( 'FtImport.exe','HULME','3/3/2023 2:25 PM','20230303','9F6717DCEFE935D6C5BD3A9A39B50924','Added [-!<schema>] command-line argument. Improved error reporting for UtConnect() failures.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'FtValidate.exe','HULME','3/3/2023 2:25 PM','20230303','6692CBE0A988A4322FCB5197B60FD65E','Added [-!<schema>] command-line argument. Improved error reporting for UtConnect() failures.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'MtUpdate.exe','HULME','3/3/2023 2:25 PM','20230303','C048FDF9AEF7E1C9B51DBEFF06FAE102','Added [-!<schema>] command-line argument. Improved error reporting for UtConnect() failures.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'TpRunTsip.exe','HULME','1/5/2023 12:07 PM','20230105','3B9843BD82483C957F0BF83D5BD719E9','Added [-u<micsUser>] command-line option. Corrected drive letter for dted50 and dted250 location.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'CopyTable.exe','VENN','1/2/2023 7:45 PM','20230102','57F464FA89FC234D2320A21BBAB67891','Bill modified this to work correctly on CloudMicsDev.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'KillTable.exe','VENN','1/2/2023 2:46 PM','20230102','8F73AA43CDD784F086E6B98ECF7CAC10','Bill modified this to work correctly on CloudMicsDev.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','3/2/2023 1:59 PM','20230302','1D70877F218E454E5D12B39C0B04DC36','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','3/2/2023 1:59 PM','20230302','D3CEAF2C664AC339A832BB2CD2B6E5BC','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','3/2/2023 1:59 PM','20230302','272E21021F0DFF3410A179967DDD4EA6','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','3/2/2023 1:59 PM','20230302','4BFE05FE7CC8FE364746E312EE9C68B7','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','3/2/2023 1:59 PM','20230302','FEBA3AA468A48B59941097BE46A4E04A','Minor changes required to work on MicsCloudDev: server names, database names, etc.' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','3/31/2023 12:42 PM','20230331','E21A0AC97037056B3C42987A1591E0AF','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','4/4/2023 6:51 PM','20230404','7622005DABDA30DADFE7C90F6578D88C','No changes - recompiled only' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','3/31/2023 12:42 PM','20230331','DBB8FE9A6FD0D5F6E2EC59389CDBA2F3','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','3/31/2023 12:42 PM','20230331','AE16825CE30ABF2614633C6B017F42C7','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_OHloss.dll','HULME','3/31/2023 12:42 PM','20230331','70E87BB1EE018F54F5D74CDC8B5176B9','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','3/31/2023 12:42 PM','20230331','C056733D7C5C551A2D9BCE5C990E8754','Removed dependence on [sys].[database_principals] for lookup of ''oper'' for prescribed ''micsid'' - now only uses [adm].[account_details].' )

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','4/5/2023 1:12 PM','20230405','650D3241590551AABD325A0CE822FEAA','No changes - recompiled only.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','4/5/2023 1:12 PM','20230405','C19CCD21693DA70087712F074CE3EE0A','No changes - recompiled only.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','4/5/2023 1:12 PM','20230405','FBC368683BB1045574ECE8F308FB3B4C','No changes - recompiled only.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','4/5/2023 1:12 PM','20230405','6FC88D7C15AA80AEAEFED67800366DA8','Removed erroneous change to Info.CollateExeMetaData() that upset FtPrint.' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','4/5/2023 1:12 PM','20230405','1F7384DDF31CED4557BF5CE5E8FACD0A','No changes - recompiled only.' )

INSERT INTO hulme.prod_bin_config VALUES ( 'FtValidate.exe','HULME','5/2/2023 12:01 PM','20230502','65F7FC0FB35C4FAB0C0F0CA81BAE7AEC','Updated to give all users access to HiLo2021.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'HiLoCheck.exe','HULME','5/2/2023 12:01 PM','20230502','15B4F9E6D16B3014F1091A0AFC928B92','Updated to give all users access to HiLo2021.' )
INSERT INTO hulme.prod_bin_config VALUES ( 'TpRunTsip.exe','HULME','5/2/2023 12:01 PM','20230502','178F5F90E5764F53030AEB6867D08869','Updated to give all users access to HiLo2021.' )

INSERT INTO hulme.prod_bin_config VALUES ( 'pfdcont.exe','HULME','5/3/2023 12:47 PM','20230503','21F59B5EA3FD5B802E53A2BF1A4835E3','Updated to remove blank line at start of KML file.' )


INSERT INTO hulme.prod_bin_config VALUES ('Chg_Mbr_Pass.exe','VENN','5/20/2023 8:50 AM','20230520','46A36738A6A43EC15A76CD1460A33ACD','Modified to work in CloudMics.') 
INSERT INTO hulme.prod_bin_config VALUES ('CopyTable.exe','VENN','5/4/2023 3:19 PM','20230504','E19AC39812AF0F0BDFADF5CAE75B65D8','Modified to work in CloudMics.') 
INSERT INTO hulme.prod_bin_config VALUES ('KillTable.exe','VENN','5/4/2023 3:23 PM','20230504','398664F59D42D3DDE7A5CF5464CC61A0','Modified to work in CloudMics.') 
INSERT INTO hulme.prod_bin_config VALUES ('sdfImport.exe','VENN','5/4/2023 4:35 PM','20230504','5A3412578D30F1FA289872B6DC500A2A','Modified to work in CloudMics.') 
INSERT INTO hulme.prod_bin_config VALUES ('sdfPrint.exe','VENN','5/4/2023 3:25 PM','20230504','20EEBD4C591BCE41549F3206CEFB05B0','Modified to work in CloudMics.') 
INSERT INTO hulme.prod_bin_config VALUES ('sdfValidate.exe','VENN','5/9/2023 8:42 AM','20230509','6C2942D203F806974CB6676439AC1AB5','Modified to work in CloudMics.')

UPDATE hulme.prod_bin_config 
	SET description = 'The contents of file HiloCheckSupp.cs have been commented out - FtValidate, HiLoCheck and TpRunTsip now use class HiLoAnalysis2021.'
	WHERE md5 = '1F7384DDF31CED4557BF5CE5E8FACD0A'

INSERT INTO hulme.prod_bin_config VALUES ( '_Auxlib.dll','HULME','7/26/2023 12:26 PM','20230726','F2CBD269BAD6C091E4B7A6F3BDBFE833','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Configuration.dll','HULME','7/26/2023 12:26 PM','20230726','92F77A5DF425CB760A08DED525975A9F','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_DataStructures.dll','HULME','7/26/2023 12:26 PM','20230726','68F200D4D23E66826A60C15D982D9824','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_NewLib.dll','HULME','7/26/2023 12:26 PM','20230726','EEDCC608D1AA8DA05BD5AA41F48DA590','No changes - recompiled to date/time align with _Utillib.dll' )
INSERT INTO hulme.prod_bin_config VALUES ( '_Utillib.dll','HULME','7/26/2023 12:26 PM','20230726','AA0279B6ED1C28FC3A61DBB7E3298F1F','Qutils.EnterQueue() modified to have minimum wait time of 30s.' )
