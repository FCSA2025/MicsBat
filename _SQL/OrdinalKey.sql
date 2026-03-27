USE [micsdev]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*  
	Returns a string that can be used to sort link tables based on call1-call2 pairs.
*/
ALTER function [hulme].[OrdinalKey](@call1 AS [char](9), @call2 AS [char](9))
returns [CHAR](19)
begin
	declare @c1 AS [VARCHAR](9) = LTRIM(RTRIM(@call1));
	declare @c2 AS [VARCHAR](9) = LTRIM(RTRIM(@call2));

	declare @first  AS [VARCHAR](9) = CASE WHEN (@c1 <= @c2) THEN @c1 ELSE @c2 END;
	declare @second AS [VARCHAR](9) = CASE WHEN (@c1 >  @c2) THEN @c1 ELSE @c2 END;

	return CONCAT(@first, '-', @second);
end