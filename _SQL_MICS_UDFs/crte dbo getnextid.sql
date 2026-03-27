USE [fcsa]
GO

/****** Object:  StoredProcedure [dbo].[getnextid]    Script Date: 4/3/2019 7:17:23 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create procedure  [dbo].[getnextid] (@idname varchar(12)) 
as
begin
 declare @idvalue int;
 set transaction isolation level serializable;
 begin transaction
	select @idvalue = nextid from adm.nexttable where idval=@idname;
	if @@rowcount = 0 begin
		INSERT into adm.nexttable (idval, nextid) values (@idname, 1);
		set @idvalue = 1;
	end
	update adm.nexttable set nextid = nextid + 1 where idval=@idname
 commit
 return @idvalue
end

GO


