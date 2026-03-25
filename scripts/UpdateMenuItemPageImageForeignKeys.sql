SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.MenuItemPages', N'U') IS NULL
    BEGIN
        THROW 50000, 'Table dbo.MenuItemPages does not exist.', 1;
    END;

    IF OBJECT_ID(N'dbo.Images', N'U') IS NULL
    BEGIN
        THROW 50000, 'Table dbo.Images does not exist. Run the image assets migration/script first.', 1;
    END;

    IF COL_LENGTH('dbo.MenuItemPages', 'FeaturedImageId') IS NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.MenuItemPages ADD FeaturedImageId UNIQUEIDENTIFIER NULL;');
    END;

    IF COL_LENGTH('dbo.MenuItemPages', 'HeaderImageId') IS NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.MenuItemPages ADD HeaderImageId UNIQUEIDENTIFIER NULL;');
    END;

    IF COL_LENGTH('dbo.MenuItemPages', 'FeaturedImage') IS NOT NULL
    BEGIN
        EXEC(N'
            UPDATE page
            SET FeaturedImageId = image.Id
            FROM dbo.MenuItemPages AS page
            INNER JOIN dbo.Images AS image ON image.Url = page.FeaturedImage
            WHERE page.FeaturedImage IS NOT NULL
              AND page.FeaturedImageId IS NULL;');
    END;

    IF COL_LENGTH('dbo.MenuItemPages', 'HeaderImage') IS NOT NULL
    BEGIN
        EXEC(N'
            UPDATE page
            SET HeaderImageId = image.Id
            FROM dbo.MenuItemPages AS page
            INNER JOIN dbo.Images AS image ON image.Url = page.HeaderImage
            WHERE page.HeaderImage IS NOT NULL
              AND page.HeaderImageId IS NULL;');
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_MenuItemPages_FeaturedImageId'
          AND object_id = OBJECT_ID('dbo.MenuItemPages'))
    BEGIN
        EXEC(N'CREATE NONCLUSTERED INDEX IX_MenuItemPages_FeaturedImageId ON dbo.MenuItemPages(FeaturedImageId);');
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_MenuItemPages_HeaderImageId'
          AND object_id = OBJECT_ID('dbo.MenuItemPages'))
    BEGIN
        EXEC(N'CREATE NONCLUSTERED INDEX IX_MenuItemPages_HeaderImageId ON dbo.MenuItemPages(HeaderImageId);');
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_MenuItemPages_Images_FeaturedImageId')
    BEGIN
        EXEC(N'
            ALTER TABLE dbo.MenuItemPages
                ADD CONSTRAINT FK_MenuItemPages_Images_FeaturedImageId
                FOREIGN KEY (FeaturedImageId) REFERENCES dbo.Images(Id) ON DELETE NO ACTION;');
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_MenuItemPages_Images_HeaderImageId')
    BEGIN
        EXEC(N'
            ALTER TABLE dbo.MenuItemPages
                ADD CONSTRAINT FK_MenuItemPages_Images_HeaderImageId
                FOREIGN KEY (HeaderImageId) REFERENCES dbo.Images(Id) ON DELETE NO ACTION;');
    END;

    IF COL_LENGTH('dbo.MenuItemPages', 'FeaturedImage') IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.MenuItemPages DROP COLUMN FeaturedImage;');
    END;

    IF COL_LENGTH('dbo.MenuItemPages', 'HeaderImage') IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.MenuItemPages DROP COLUMN HeaderImage;');
    END;

    COMMIT TRANSACTION;

    PRINT 'MenuItemPages image columns converted to Images foreign keys.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
