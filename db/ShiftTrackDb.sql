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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251220233211_InitialCreate'
)
BEGIN
    CREATE TABLE [ShiftDefinitions] (
        [Id] int NOT NULL IDENTITY,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [BreakStartTime] time NOT NULL,
        [BreakEndTime] time NOT NULL,
        [IsOvertimeEligible] bit NOT NULL,
        CONSTRAINT [PK_ShiftDefinitions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251220233211_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251220233211_InitialCreate', N'10.0.1');
END;

COMMIT;
GO

