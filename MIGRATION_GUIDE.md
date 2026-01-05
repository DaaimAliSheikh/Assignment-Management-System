# Database Migration Commands

## Using Visual Studio Package Manager Console

Open **Package Manager Console** in Visual Studio:
- Go to: Tools ? NuGet Package Manager ? Package Manager Console

Then run these commands:

```powershell
# Create the initial migration
Add-Migration InitialCreate

# Apply migration to create database
Update-Database
```

## Alternative: Using .NET CLI (if you install dotnet-ef globally)

If you prefer command line, you can install the tool globally:

```bash
dotnet tool install --global dotnet-ef
```

Then run from the project directory:

```bash
# Create migration
dotnet ef migrations add InitialCreate --project AssignmentManagementSystem/AssignmentManagementSystem.csproj

# Update database
dotnet ef database update --project AssignmentManagementSystem/AssignmentManagementSystem.csproj
```

## What These Commands Do

1. **Add-Migration InitialCreate**
   - Creates a Migrations folder in your project
   - Generates migration files based on your models
   - Files created:
     - `YYYYMMDDHHMMSS_InitialCreate.cs` (migration)
     - `YYYYMMDDHHMMSS_InitialCreate.Designer.cs` (metadata)
     - `ApplicationDbContextModelSnapshot.cs` (current model state)

2. **Update-Database**
   - Creates the database if it doesn't exist
   - Applies all pending migrations
   - Creates all tables, relationships, and constraints
   - Seeds the Teacher and Student roles
   - Creates default test accounts

## Verify Database Creation

After running the commands, you can verify the database was created:

1. **In Visual Studio:**
   - View ? SQL Server Object Explorer
   - Expand: (localdb)\MSSQLLocalDB ? Databases ? AssignmentManagementDb

2. **Expected Tables:**
   - AspNetUsers
   - AspNetRoles
   - AspNetUserRoles
   - AspNetUserClaims
   - AspNetUserLogins
   - AspNetUserTokens
   - AspNetRoleClaims
   - Classrooms
   - StudentClassrooms
   - Assignments
   - Submissions

## Troubleshooting

### Error: "Build failed"
- Make sure the project compiles: `dotnet build`
- Fix any compilation errors first

### Error: "Connection string not found"
- Check `appsettings.Development.json` exists
- Verify ConnectionStrings section is present

### Error: "SQL Server not found"
- Install SQL Server Express or LocalDB
- Start SQL Server service
- Verify connection string matches your SQL Server instance

### Need to Reset Database
```powershell
# Remove database
Drop-Database

# Or remove and recreate
Update-Database -Migration 0
Remove-Migration
Add-Migration InitialCreate
Update-Database
```

## Next Steps After Migration

1. Run the application: `dotnet run`
2. Open Swagger: https://localhost:5001/swagger
3. Test login with default accounts:
   - Teacher: teacher@test.com / Teacher@123
   - Student: student@test.com / Student@123
