USE micsdev
GO

ALTER PROCEDURE hulme.Log2
					@identifier VARCHAR(MAX),
					@text VARCHAR(MAX)

AS
	INSERT INTO hulme.log SELECT GETDATE(), @identifier, @text
GO
