USE [UHPSimulatorII]
GO

/****** Object:  StoredProcedure [dbo].[USP_Notification_Insert]    Script Date: 16/12/2017 10:51:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Ludovic>
-- Create date: <4 Dec 2017>
-- Description:	<Add 1 notification to Notifications table>
-- =============================================
CREATE PROCEDURE [dbo].[USP_Notification_Insert]  
	-- Add the parameters for the stored procedure here
	(                         
	 @Source			nvarchar(50) = '',
	 @Type				nvarchar(50) = '',
	 @Content			text = '',
	 @Datetime			datetime = NULL,
	 @NotificationCode	varchar(50) = ''
    ) 
	
AS
BEGIN
	INSERT INTO [Notifications]   
                    ([NotificationID],[Source],[Type],[Content],[Datetime],[notification_code])  
                     VALUES (NEWID(),@Source,@Type,@Content,@Datetime,@NotificationCode)  
                                 
                    Select 'Ok' as results  
END
GO


