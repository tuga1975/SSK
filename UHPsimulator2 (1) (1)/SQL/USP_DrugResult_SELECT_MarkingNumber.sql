USE [UHPSimulatorII]
GO

/****** Object:  StoredProcedure [dbo].[USP_DrugResult_SELECT_MarkingNumber]    Script Date: 16/12/2017 10:50:31 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[USP_DrugResult_SELECT_MarkingNumber] 
	(                         
	 @markingnumber	nvarchar(50) = ''
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT TOP 1 (CASE WHEN [AMPH] = 1 OR 
				  [BENZ] = 1 OR 
				  [OPI] = 1 OR 
				  [THC] = 1 OR 
				  [COCA] = 1 OR 
				  [BARB] = 1 OR 
				  [LSD] = 1 OR 
				  [METH] = 1 OR 
				  [MTQL] = 1 OR 
				  [PCP] = 1 OR 
				  [KET] = 1 OR 
				  [BUPRE] = 1 OR 
				  [CAT] = 1 OR 
				  [PPZ] = 1 OR 
				  [NPS] = 1 THEN 'Positive' ELSE 'Negative'
		END) as result,
		[NRIC],
		[timestamp],
		[markingnumber],
		IIF([AMPH]=1,'Positive','Negative') AS AMPH,
		IIF([BENZ]=1,'Positive','Negative') AS BENZ,
		IIF([OPI]=1,'Positive','Negative') AS OPI,
		IIF([THC]=1,'Positive','Negative') AS THC,
		IIF([COCA]=1,'Positive','Negative') AS COCA,

		IIF([BARB]=1,'Positive','Negative') AS BARB,
		IIF([LSD]=1,'Positive','Negative') AS LSD,
		IIF([METH]=1,'Positive','Negative') AS METH,
		IIF([MTQL]=1,'Positive','Negative') AS MTQL,
		IIF([PCP]=1,'Positive','Negative') AS PCP,

		IIF([KET]=1,'Positive','Negative') AS KET,
		IIF([BUPRE]=1,'Positive','Negative') AS BUPRE,
		IIF([CAT]=1,'Positive','Negative') AS CAT,
		IIF([PPZ]=1,'Positive','Negative') AS PPZ,
		IIF([NPS]=1,'Positive','Negative') AS NPS
  FROM [UHPSimulatorII].[dbo].[DrugResults]
  WHERE [markingnumber] =@markingnumber
  ORDER BY timestamp DESC
END
GO


