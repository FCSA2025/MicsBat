USE [fcsa]
GO

/****** Object:  StoredProcedure [dbo].[FCSASchema]    Script Date: 4/3/2019 7:18:31 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[FCSASchema] @cSchema as varchar(6) OUTPUT
as 
begin
	set @cSchema = (SELECT default_schema_name FROM
			sys.database_principals WHERE
			name = USER_NAME());
end;

GO


