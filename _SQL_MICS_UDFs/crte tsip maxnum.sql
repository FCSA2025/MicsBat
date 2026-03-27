USE [fcsa]
GO

/****** Object:  UserDefinedFunction [tsip].[maxnum]    Script Date: 4/3/2019 7:12:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create function [tsip].[maxnum](@N1 as float, @N2 as float)
returns float
begin
	declare @retval float;
	
	if @N1 is null begin
	  set @N1 = 0.0;
	end;
	if @N2 is null begin
	  set @N2 = 0.0;
	end;
	
	if @N1 >= @N2 begin
	  set @retval = @N1;
	  end;
	else begin
	  set @retval = @N2;
	end;
		
	return @retval;
end

GO

EXEC sys.sp_addextendedproperty @name=N'IsDeterministic', @value=N'TRUE' , @level0type=N'SCHEMA',@level0name=N'tsip', @level1type=N'FUNCTION',@level1name=N'maxnum'
GO


