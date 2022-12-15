--USE [SpeedWebAPI]
--GO

/****** Object:  StoredProcedure [dbo].[Get_GetSpeedLimitFromSpeedTable]    Script Date: 12/15/2022 2:18:16 PM ******/
DROP PROCEDURE [dbo].[Get_GetSpeedLimitFromSpeedTable]
GO

/****** Object:  StoredProcedure [dbo].[Get_GetSpeedLimitFromSpeedTable]    Script Date: 12/15/2022 2:18:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--DROP PROCEDURE  [dbo].[Get_GetSpeedLimitFromSpeedTable]

CREATE PROCEDURE  [dbo].[Get_GetSpeedLimitFromSpeedTable]
	@SpeedLimit SpeedLimit READONLY
AS
BEGIN


	BEGIN TRY
        BEGIN TRANSACTION;
			-- SET NOCOUNT ON added to prevent extra result sets from
			-- interfering with SELECT statements.
			SET NOCOUNT ON;


			DECLARE @ListSpeedLimit TABLE(Lat	[decimal](18, 10)
				,Lng	[decimal](18, 10)
				,ProviderType	int
				,Position	nvarchar(50)
				,MinSpeed	int
				,MaxSpeed	int
				,PointError	bit
				,SegmentID	bigint
				,CreatedDate	datetime2(7)
				,UpdatedDate	datetime2(7)
				,CreatedBy	nvarchar(256)
				,UpdatedBy	nvarchar(256)
				,DeleteFlag	int
				,UpdateCount	int)


				--Khai báo biến để lưu nội dung đọc
				DECLARE 
				@Lat	[decimal](18, 10)
				,@Lng	[decimal](18, 10)
				,@ProviderType	int
				,@Position	nvarchar(50)
				,@MinSpeed	int
				,@MaxSpeed	int
				,@PointError	bit
				,@SegmentID	bigint
				,@CreatedDate	datetime2
				,@UpdatedDate	datetime2
				,@CreatedBy	nvarchar(256)
				,@UpdatedBy	nvarchar(256)
				,@DeleteFlag	int
				,@UpdateCount	int

				declare @cnt int = 0;

				DECLARE curSpeedLimit CURSOR FOR  -- khai báo con trỏ cursorProduct
				SELECT [Lat]
				  ,[Lng]
				  ,[ProviderType]
				  ,[Position]
				  ,[MinSpeed]
				  ,[MaxSpeed]
				  ,[PointError]
				  ,[SegmentID]
				  ,[CreatedDate]
				  ,[UpdatedDate]
				  ,[CreatedBy]
				  ,[UpdatedBy]
				  ,[DeleteFlag]
				  ,[UpdateCount] FROM @SpeedLimit     -- dữ liệu trỏ tới

				OPEN curSpeedLimit                -- Mở con trỏ

				FETCH NEXT FROM curSpeedLimit     -- Đọc dòng đầu tiên
					  INTO @Lat
						,@Lng
						,@ProviderType
						,@Position
						,@MinSpeed
						,@MaxSpeed
						,@PointError
						,@SegmentID
						,@CreatedDate
						,@UpdatedDate
						,@CreatedBy
						,@UpdatedBy
						,@DeleteFlag
						,@UpdateCount


				WHILE @@FETCH_STATUS = 0          --vòng lặp WHILE khi đọc Cursor thành công
				BEGIN
												  --In kết quả hoặc thực hiện bất kỳ truy vấn
												  --nào dựa trên kết quả đọc được
						select @cnt = count(*) from dbo.SpeedLimit
						where Lat = @Lat and Lng =  @Lng AND ProviderType = @ProviderType and RTRIM(Position) = RTRIM(@Position) and DeleteFlag = 0 and MaxSpeed > 0
						and DeleteFlag = 0

						IF @cnt <> 0
						BEGIN
							insert into  @ListSpeedLimit
							select top 1	
								[Lat]
								  ,[Lng]
								  ,[ProviderType]
								  ,[Position]
								  ,[MinSpeed]
								  ,[MaxSpeed]
								  ,[PointError]
								  ,[SegmentID]
								  ,[CreatedDate]
								  ,[UpdatedDate]
								  ,[CreatedBy]
								  ,[UpdatedBy]
								  ,[DeleteFlag]
								  ,[UpdateCount]
							 from dbo.SpeedLimit
							 where Lat = @Lat and Lng =  @Lng AND ProviderType = @ProviderType and RTRIM(Position) = RTRIM(@Position) and DeleteFlag = 0
						END
						SET @cnt = 0;
						-- Insert statements for procedure here
						
					FETCH NEXT FROM curSpeedLimit -- Đọc dòng tiếp
						  INTO @Lat
							,@Lng
							,@ProviderType
							,@Position
							,@MinSpeed
							,@MaxSpeed
							,@PointError
							,@SegmentID
							,@CreatedDate
							,@UpdatedDate
							,@CreatedBy
							,@UpdatedBy
							,@DeleteFlag
							,@UpdateCount
				END

				CLOSE curSpeedLimit              -- Đóng Cursor
				DEALLOCATE curSpeedLimit         -- Giải phóng tài nguyên

				select * from @ListSpeedLimit
		
        COMMIT TRANSACTION;  
    END TRY
	 BEGIN CATCH
       
        -- Test if the transaction is uncommittable.  
        IF (XACT_STATE()) = -1  
        BEGIN  
            PRINT  N'The transaction is in an uncommittable state.' +  
                    'Rolling back transaction.'  
            ROLLBACK TRANSACTION;  
        END;  
        
        -- Test if the transaction is committable.  
        IF (XACT_STATE()) = 1  
        BEGIN  
            PRINT N'The transaction is committable.' +  
                'Committing transaction.'  
            COMMIT TRANSACTION;     
        END;  
    END CATCH

END
GO


