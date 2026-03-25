-- Destructive script: drop and recreate MenuItems, MenuCategories, and MenuItemPages
-- WARNING: This will remove all data in these tables. Run only if you intend to recreate schema from model.

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.MenuItemPages', 'U') IS NOT NULL
    DROP TABLE dbo.MenuItemPages;

IF OBJECT_ID('dbo.MenuCategories', 'U') IS NOT NULL
    DROP TABLE dbo.MenuCategories;

IF OBJECT_ID('dbo.MenuItems', 'U') IS NOT NULL
    DROP TABLE dbo.MenuItems;

-- Create MenuItems
CREATE TABLE dbo.MenuItems (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuItems PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT('draft')
);

-- Create MenuCategories (belongs to a MenuItem). Deleting a MenuItem cascades to categories.
CREATE TABLE dbo.MenuCategories (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuCategories PRIMARY KEY,
    MenuItemId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT('draft'),
    CONSTRAINT FK_MenuCategories_MenuItems FOREIGN KEY (MenuItemId) REFERENCES dbo.MenuItems(Id) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_MenuCategories_MenuItemId ON dbo.MenuCategories(MenuItemId);

-- Create MenuItemPages: Pages can belong to a MenuCategory (nullable, ON DELETE SET NULL)
-- or to a MenuItem (nullable, ON DELETE NO ACTION to avoid multiple cascade paths).
CREATE TABLE dbo.MenuItemPages (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuItemPages PRIMARY KEY,
    MenuCategoryId UNIQUEIDENTIFIER NULL,
    MenuItemId UNIQUEIDENTIFIER NULL,
    Slug NVARCHAR(450) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Subtitle NVARCHAR(MAX) NULL,
    Description NVARCHAR(MAX) NULL,
    Content NVARCHAR(MAX) NULL,
    DateISO DATETIME2 NOT NULL,
    DateModified DATETIME2 NOT NULL,
    FeaturedImageId UNIQUEIDENTIFIER NULL,
    HeaderImageId UNIQUEIDENTIFIER NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT('draft'),
    SeoTitle NVARCHAR(MAX) NULL,
    SeoDescription NVARCHAR(MAX) NULL,
    OgImage NVARCHAR(MAX) NULL,
    CanonicalUrl NVARCHAR(MAX) NULL
);

ALTER TABLE dbo.MenuItemPages
    ADD CONSTRAINT UQ_MenuItemPages_Slug UNIQUE (Slug);

ALTER TABLE dbo.MenuItemPages
    ADD CONSTRAINT FK_MenuItemPages_MenuCategories FOREIGN KEY (MenuCategoryId) REFERENCES dbo.MenuCategories(Id) ON DELETE SET NULL;

-- Keep MenuItem FK as NO ACTION to avoid multiple cascade paths
ALTER TABLE dbo.MenuItemPages
    ADD CONSTRAINT FK_MenuItemPages_MenuItems FOREIGN KEY (MenuItemId) REFERENCES dbo.MenuItems(Id) ON DELETE NO ACTION;

ALTER TABLE dbo.MenuItemPages
    ADD CONSTRAINT FK_MenuItemPages_Images_FeaturedImageId FOREIGN KEY (FeaturedImageId) REFERENCES dbo.Images(Id) ON DELETE NO ACTION;

ALTER TABLE dbo.MenuItemPages
    ADD CONSTRAINT FK_MenuItemPages_Images_HeaderImageId FOREIGN KEY (HeaderImageId) REFERENCES dbo.Images(Id) ON DELETE NO ACTION;

CREATE NONCLUSTERED INDEX IX_MenuItemPages_MenuCategoryId ON dbo.MenuItemPages(MenuCategoryId);
CREATE NONCLUSTERED INDEX IX_MenuItemPages_MenuItemId ON dbo.MenuItemPages(MenuItemId);
CREATE NONCLUSTERED INDEX IX_MenuItemPages_FeaturedImageId ON dbo.MenuItemPages(FeaturedImageId);
CREATE NONCLUSTERED INDEX IX_MenuItemPages_HeaderImageId ON dbo.MenuItemPages(HeaderImageId);

COMMIT TRANSACTION;

PRINT 'MenuItems, MenuCategories, MenuItemPages recreated.';
