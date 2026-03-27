USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[isMember]    Script Date: 5/19/2021 11:24:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Returns a BIT value indicating if the prescribed oper string is a member
-- of the table adm.account_ids, or not. 
ALTER FUNCTION [hulme].[isMember](@oper VARCHAR(6))
RETURNS BIT
BEGIN

	DECLARE @ret BIT; 

	-- oper is a key to the table adm.account_ids and so Count() can only be 0 or 1.
	SELECT @ret = COUNT(IDS.oper) FROM adm.account_ids AS IDS WHERE IDS.oper = @oper;

	RETURN @ret;

END