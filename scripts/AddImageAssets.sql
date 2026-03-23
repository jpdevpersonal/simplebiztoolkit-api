IF OBJECT_ID(N'[Images]', N'U') IS NULL
BEGIN
    CREATE TABLE [Images] (
        [Id] uniqueidentifier NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [BlobName] nvarchar(450) NOT NULL,
        [AltText] nvarchar(max) NULL,
        [Caption] nvarchar(max) NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [UpdatedUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Images] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Images_BlobName] ON [Images] ([BlobName]);
END;
