/****** Script for SelectTopNRows command from SSMS  ******/
DECLARE @lat AS REAL= 49.92887	
DECLARE @lng AS REAL= -99.38856

SELECT @lat, @lng		

SELECT 
	   [keyfield] AS Line#,
      @lat AS lat, 
	  @lng AS lng,
	   [TXRX]
      ,[FrequencyMhz]/*
      ,[Frequencyrecordidentifier]
      ,[Regulatoryservice]
      ,[CommunicationType]
      ,[Conformitytofrequencyplan]
      ,[Frequencyallocationname]
      ,[Channel]
      ,[Internationalcoordinationnumber]
      ,[Analogdigital]
      ,[OccupiedbandwidthkHz]
      ,[Designationofemission]
      ,[Modulationtype]
      ,[Filtrationinstalled]
      ,[TxeffectiveradiatedpowerERPdBW]
      ,[TxtransmitterpowerW]
      ,[TotallossesdB]
      ,[AnalogcapacityChannels]
      ,[DigitalcapacityMbits]
      ,[RxunfadedreceivedsignalleveldBW]
      ,[RxthresholdsignallevelforBER10e3dBW]
      ,[Manufacturer]
      ,[Modelnumber]
      ,[AntennagaindBi]
      ,[Antennapattern]
      ,[Halfpower3dBbeamwidthdeg]
      ,[FronttobackratiodB]
      ,[Polarization]
      ,[Heightabovegroundlevelm] AS aht*/
      ,STR([Azimuthofmainlobedeg], 5, 1) AS azimuth/*
      ,[Verticalelevationangledeg]
      ,[Stationlocation]
      ,[Licenseestationreference]*/
      ,[Callsign]/*
      ,[Typeofstation]
      ,[ITUclassofstation]
      ,[Stationcostcategory]
      ,[Numberofidenticalstations]
      ,[Referenceidentifier]*/
      ,[Provinces]
      ,[LatitudeWGS84]
      ,[LongitudeWGS84]/*
      ,[Groundelevationabovemeansealevelm]
      ,[Antennastructureheightabovegroundlevelm]
      ,[Congestionzone]
      ,[Radiusofoperationkm]
      ,[Satellitename]*/
      ,[Authorizationnumber]/*
      ,[MWService]
      ,[Subservice]
      ,[Licencetype]
      ,[Authorizationstatus]
      ,[Inservicedate]
      ,[Accountnumber]*/
      ,[Licenseename]/*
      ,[Licenseeaddress]
      ,[Operationalstatus]
      ,[Stationclass]
      ,[HorizontalpowerW]
      ,[VerticalpowerW]
      ,[Standbytransmitterinformation]
      ,[MICSoper]
      ,[MICSopnote]
      ,[MICScompany]
      ,[RecordAction]*/
	  ,STR(1000*hulme.distance_deg( @lat, @lng, T.LatitudeWGS84, T.LongitudeWGS84), 3, 0) AS d_meters

  FROM hulme.TAFL20230111 AS T
  WHERE
	--TXRX = 'TX' 
	--AND ([hulme].[distance_deg]( @lat, @lng, T.LatitudeWGS84, T.LongitudeWGS84)  < 0.3)
      ([hulme].[distance_less_than_deg]( @lat, @lng, T.LatitudeWGS84, T.LongitudeWGS84, 0.3) = 1)
  ORDER BY d_meters, FrequencyMhz, Azimuthofmainlobedeg

  ----------------------------------------
  ----------------------------------------

  --SELECT Provinces, Count(Provinces) FROM [fcsa].[venn].[ISEDCleanedI20A] GROUP BY Provinces ORDER BY Provinces

 --SELECT keyfield, MICSoper, MICSopnote FROM [fcsa].[venn].[ISEDFullCleanedI20A] WHERE MICSopnote IN ('FC', 'FT') ORDER BY MICSopnote--WHERE keyfield = '44643'

 --SELECT * FROM [fcsa].[frse].[ISEDCleanedI21B]