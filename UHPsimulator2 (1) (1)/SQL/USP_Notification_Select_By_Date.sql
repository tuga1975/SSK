USE [UHPSimulatorII]
GO

/****** Object:  StoredProcedure [dbo].[USP_Notification_Select_By_Date]    Script Date: 16/12/2017 10:51:33 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[USP_Notification_Select_By_Date]  
	(                         
	 @Datetime			datetime = NULL
    ) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * FROM [Notifications] WHERE DateDiff(dd,[Datetime], @Datetime) = 0 Order By [Datetime] Desc
END
GO


