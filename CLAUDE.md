# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build the solution
dotnet build AmbitInventory.slnx

# Run the web app
dotnet run --project src/Inventory.Web

# EF Core migrations (run from solution root)
dotnet ef migrations add <MigrationName> --project src/Inventory.Infrastructure --startup-project src/Inventory.Web
dotnet ef database update --project src/Inventory.Infrastructure --startup-project src/Inventory.Web
```

## Database Configuration

The app uses PostgreSQL via Npgsql. Connection string is read via `GetConnectionString("DefaultConnection")` in `Program.cs`. In development, set it in `src/Inventory.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=Inventory;Username=postgres;Password=admin"
  }
}
```

On first run, `IdentitySeeder` creates roles (`Admin`, `StoreManager`) and a default admin account: `admin@ambit.com` / `Admin@123`.

## Architecture

Four projects in clean-architecture style:

- **`Inventory.Domain`** — Entities and ViewModels only; no dependencies on other projects.
- **`Inventory.Application`** — Placeholder, currently unused.
- **`Inventory.Infrastructure`** — `ApplicationDbContext`, EF migrations, `IdentitySeeder`, `IUserRoleService`/`UserRoleService`. Depends on Domain.
- **`Inventory.Web`** — ASP.NET Core MVC app. Controllers inject `ApplicationDbContext` directly (no repository layer). Depends on all other projects.

Controllers are the thickest layer; there is no service/repository abstraction between controllers and the DbContext.

## Domain Model

The core domain tracks **textile designs** and **production programs**:

- `Catalogue` — Product catalog item with a unique `Code`, optional photo/PDF files. Stock is computed from `InventoryTransaction` records (no stored stock column).
- `Design` → `DesignPlate` → `DesignMatching` — A design has N plates, each plate has N color matchings (`MatchingNo` + `Colour`).
- `ProgramEntry` (table name: `Program`) — A production run linked to a `Party`. References matchings via `ProgramMatching` rows, which store `DesignId`, `PlateId`, `DesignMatchingId`, `MatchingNo`, and `Colour` as a denormalized snapshot.
- `Party` — Customer or supplier.

**Key relationship:** `ProgramController.Save` resolves the `SelectedMatchings` list (posted as `"DesignId|MatchingNo"` strings) against live `DesignMatching` records from the DB, then writes denormalized `ProgramMatching` rows. On edit, all existing `ProgramMatchings` are deleted and re-inserted.

## File Uploads

Uploaded files are stored under `wwwroot/uploads/`:
- Catalogue photos/PDFs → `wwwroot/uploads/catalogues/`
- Program photos → `wwwroot/uploads/programs/`

Filenames are generated as `photo_{Guid}.ext` or `doc_{Guid}.ext`. Program photos are validated: jpg/jpeg/png only, max 3 MB.

## Identity & Authorization

All controllers carry `[Authorize]`. The `AccountController.cs` is excluded from compilation (`<Compile Remove=...>` in the csproj); authentication UI is handled by scaffolded Razor Pages under `Areas/Identity`.

`IUserRoleService` (registered as scoped) wraps `UserManager` and `RoleManager` for role/user CRUD used by `RolesController`.

## Pagination

`PaginatedList<T>` (in `Inventory.Domain/ViewModels/PaginatedList.cs`) is used across all list views. Default page size is 20 for Designs/Programs, 10 for Catalogues. `page` and `pageSize` are passed as query parameters.
