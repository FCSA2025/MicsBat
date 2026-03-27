USE [fcsa]
GO

/****** Object:  UserDefinedFunction [tsip].[minnum]    Script Date: 4/3/2019 7:13:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create function [tsip].[minnum](@N1 as float, @N2 as float)
returns float
begin
	declare @retval float;
	
	if @N1 is null begin
	  set @N1 = 0.0;
	end;
	if @N2 is null begin
	  set @N2 = 0.0;
	end;
	
	if @N1 <= @N2 begin
	  set @retval = @N1;
	  end;
	else begin
	  set @retval = @N2;
	end;
		
	return @retval;
end

GO


