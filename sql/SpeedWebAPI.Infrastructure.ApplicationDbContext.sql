IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221215025324_MyMigration')
BEGIN
    CREATE TABLE [SpeedLimit] (
        [Lat] decimal(18,10) NOT NULL,
        [Lng] decimal(18,10) NOT NULL,
        [ProviderType] int NOT NULL,
        [Position] nvarchar(50) NOT NULL,
        [MinSpeed] int NULL,
        [MaxSpeed] int NULL,
        [PointError] bit NULL,
        [SegmentID] bigint NULL,
        [IsUpdateSpeed] bit NULL,
        [CreatedDate] datetime2 NULL,
        [UpdatedDate] datetime2 NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [DeleteFlag] int NULL,
        [UpdateCount] int NULL,
        CONSTRAINT [PK_SpeedLimit] PRIMARY KEY ([Lat], [Lng], [ProviderType], [Position])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221215025324_MyMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221215025324_MyMigration', N'5.0.10');
END;
GO

COMMIT;
GO

