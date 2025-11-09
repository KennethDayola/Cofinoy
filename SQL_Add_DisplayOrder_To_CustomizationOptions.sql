-- Migration: Add DisplayOrder column to CustomizationOptions table
-- Date: 2025-01-25
-- Description: Adds DisplayOrder field with default value 0 to support ordering of customization options

-- Add the DisplayOrder column
ALTER TABLE [dbo].[CustomizationOptions]
ADD [DisplayOrder] INT NOT NULL DEFAULT 0;

-- Optional: Update existing records to have sequential DisplayOrder based on their current order
-- This query sets DisplayOrder based on alphabetical order of Name within each Customization
WITH RankedOptions AS (
    SELECT 
        [Id],
        ROW_NUMBER() OVER (PARTITION BY [CustomizationId] ORDER BY [Name]) AS RowNum
    FROM [dbo].[CustomizationOptions]
)
UPDATE co
SET co.[DisplayOrder] = ro.RowNum
FROM [dbo].[CustomizationOptions] co
INNER JOIN RankedOptions ro ON co.[Id] = ro.[Id];

-- Verify the changes
SELECT 
    co.[Id],
    co.[Name],
    co.[DisplayOrder],
    co.[CustomizationId],
    c.[Name] AS CustomizationName
FROM [dbo].[CustomizationOptions] co
INNER JOIN [dbo].[Customizations] c ON co.[CustomizationId] = c.[Id]
ORDER BY c.[Name], co.[DisplayOrder];
