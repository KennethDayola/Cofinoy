# Quick Migration Guide

## Step-by-Step Instructions

### 1. Create the Migration

Open PowerShell or Command Prompt and navigate to the Cofinoy.Data project:

```powershell
cd "C:\Users\johnk\Downloads\Cofinoyv2\Cofinoyv2\Cofinoy.Data"
```

Create the migration:

```powershell
dotnet ef migrations add AddCartItemCustomizations --startup-project ..\Cofinoy.WebApp\Cofinoy.WebApp.csproj
```

**Expected Output:**
- You should see a new migration file created in `Cofinoy.Data/Migrations/` folder
- File name format: `YYYYMMDDHHMMSS_AddCartItemCustomizations.cs`

### 2. Review the Migration (Optional but Recommended)

Open the generated migration file and verify it contains:
- `CreateTable` for `CartItemCustomizations` table
- Foreign key relationship to `CartItems`
- Proper column definitions (Id, CartItemId, Name, Value, Type)

### 3. Update the Database

Apply the migration to your database:

```powershell
dotnet ef database update --startup-project ..\Cofinoy.WebApp\Cofinoy.WebApp.csproj
```

**Expected Output:**
- Migration should apply successfully
- New `CartItemCustomizations` table should be created in your database

### 4. Verify Database Changes

Connect to your database using SQL Server Management Studio or Azure Data Studio:

**Connection String:** (from appsettings.json)
```
Server=CofinoyDBv2.mssql.somee.com
Database=CofinoyDBv2
User Id=johnkenneth_SQLLogin_1
Password=zqc97nknkk
```

Run this query to verify:
```sql
-- Check if table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'CartItemCustomizations'

-- Check table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CartItemCustomizations'
ORDER BY ORDINAL_POSITION

-- Check foreign key
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    tr.name AS ReferencedTable
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables AS tr ON fk.referenced_object_id = tr.object_id
WHERE tp.name = 'CartItemCustomizations'
```

### 5. Test the Application

1. **Start the application:**
   ```powershell
   cd ..\Cofinoy.WebApp
   dotnet run
   ```

2. **Test flow:**
   - Navigate to Menu page
   - Select a product
   - Add customizations (Temperature, Size, etc.)
   - Add to cart
   - View cart page
   - Verify customizations display correctly

### Troubleshooting

#### If migration creation fails:

1. **Check project references:**
   ```powershell
   dotnet list reference
   ```

2. **Restore packages:**
   ```powershell
   dotnet restore
   ```

3. **Clean and rebuild:**
   ```powershell
   dotnet clean
   dotnet build
   ```

#### If database update fails:

1. **Check connection string** in `appsettings.json`

2. **Test database connectivity:**
   ```powershell
   dotnet ef dbcontext info --startup-project ..\Cofinoy.WebApp\Cofinoy.WebApp.csproj
   ```

3. **View migration history:**
   ```sql
   SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC
   ```

4. **Rollback if needed:**
   ```powershell
   dotnet ef database update <PreviousMigrationName> --startup-project ..\Cofinoy.WebApp\Cofinoy.WebApp.csproj
   ```

### Alternative: Manual SQL Script (if EF migration fails)

If you need to create the table manually:

```sql
CREATE TABLE [dbo].[CartItemCustomizations] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [CartItemId] NVARCHAR(450) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Value] NVARCHAR(200) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL,
    CONSTRAINT [FK_CartItemCustomizations_CartItems_CartItemId] 
        FOREIGN KEY ([CartItemId]) 
        REFERENCES [CartItems] ([Id]) 
        ON DELETE CASCADE
);

CREATE INDEX [IX_CartItemCustomizations_CartItemId] 
    ON [CartItemCustomizations] ([CartItemId]);
```

## Post-Migration Checklist

- [ ] Migration created successfully
- [ ] Database updated successfully
- [ ] `CartItemCustomizations` table exists in database
- [ ] Foreign key relationship verified
- [ ] Application builds without errors
- [ ] Application runs without errors
- [ ] Can add items with customizations to cart
- [ ] Customizations display in cart page
- [ ] Can complete checkout with customized items

## Need Help?

If you encounter issues:

1. Check the build output for detailed error messages
2. Review `CART_CUSTOMIZATION_CHANGES.md` for implementation details
3. Verify all file changes were applied correctly
4. Check Entity Framework logs in the application output
