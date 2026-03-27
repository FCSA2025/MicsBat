USE [fcsa]
GO

/****** Object:  UserDefinedFunction [dbo].[user_schema]    Script Date: 4/3/2019 7:13:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[user_schema] ()
RETURNS CHAR(8)
AS
BEGIN
	DECLARE @uschema CHAR (8);
	SELECT @uschema = default_schema_name FROM sys.database_principals WHERE type='U' AND name = USER;
	RETURN(@uschema);
END


GO


