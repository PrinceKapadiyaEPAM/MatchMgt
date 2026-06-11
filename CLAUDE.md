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

`IdentitySeeder.SeedAsync` runs at startup (not in a migration) and creates roles (`Admin`, `StoreManager`) and a default admin account: `admin@ambit.com` / `Admin@123`. It also seeds default `StoreManager` permissions for all 6 modules (`CanView=true`, `CanEdit=true`, `CanDelete=false`). The seeder is idempotent.

## Architecture

Four projects in clean-architecture style:

- **`Inventory.Domain`** — Entities and ViewModels only; no dependencies on other projects.
- **`Inventory.Application`** — Placeholder, currently unused.
- **`Inventory.Infrastructure`** — `ApplicationDbContext`, EF migrations, `IdentitySeeder`, services (`IPermissionService`, `IMenuVisibilityService`, `IPermissionManagementService`, `IRolePermissionRepository`, `IUserRoleService`, `IUserManagementService`). All registered as `AddScoped`. Depends on Domain.
- **`Inventory.Web`** — ASP.NET Core MVC app. Controllers inject `ApplicationDbContext` directly (no repository layer). Depends on all other projects.

Controllers are the thickest layer; there is no service/repository abstraction between controllers and the DbContext. Services are used only for permission and user/role management.

Middleware order: `UseHttpsRedirection` → `UseRouting` → `UseStaticFiles` → `UseAuthentication` → `UseAuthorization`. `AddMemoryCache` is registered (used for permission caching).

## Modules & Authorization

### AppModules

Six modules exist as string constants in `Inventory.Domain/Constants/AppModules.cs`:
`Catalogue`, `Inventory`, `Party`, `Design`, `Program`, `Dispatch`. The static `AppModules.All` list is the authoritative set iterated for seeding, permission matrix UI, and menu visibility.

### Two-Tier Authorization

**Tier 1 — Admin-only (`[Authorize(Roles = "Admin")]`, no `[RequirePermission]`):**
`CompanyController`, `PermissionsController`, `MenuController`, `RolesController`, `UsersController`

**Tier 2 — `[Authorize]` + `[RequirePermission]` per action:**
`CatalogueController`, `InventoryController`, `PartyController`, `DesignController`, `ProgramController`, `DispatchController`

### RequirePermission Filter

`[RequirePermission(AppModules.X, "View|Edit|Delete")]` is placed on each action individually (not the controller). Implemented as `TypeFilterAttribute` wrapping `PermissionFilter : IAsyncAuthorizationFilter`. Denial redirects to `/Identity/Account/AccessDenied`.

**Admin role bypasses all permission checks** — `PermissionService` returns `true` immediately for Admin. The permission management UI explicitly excludes Admin from the configurable role list.

Permissions are stored in the `RolePermissions` table (`RolePermission` entity: `RoleName`, `Module`, `CanView`, `CanEdit`, `CanDelete`, `IsMenuVisible`). They are cached per-role in `IMemoryCache` with a 5-minute TTL and explicitly invalidated when permissions or menu visibility are saved.

### Menu Visibility

`IsMenuVisible` on `RolePermission` is independent of `CanView` — a module can be hidden from the nav without revoking view permission. `_Layout.cshtml` calls `IPermissionService.GetMenuVisibleModulesAsync(User)` and wraps each nav link in `@if (viewable.Contains(AppModules.X))`. The nav label for the `Inventory` module is **"Transaction"**.

## Domain Model

The core domain tracks **textile designs** and **production programs**:

- `Catalogue` — Product catalog item with optional photo/PDF. Has `IsDeleted` soft-delete flag (used in dispatch/program filters, but `CatalogueController.DeleteConfirmed` currently does a hard delete). Stock is a live computed value — no stored column.
- `InventoryTransaction` — `TransactionType` enum (`Stock = 1`, `Order = 2`). Stock transactions add; Order transactions subtract.
- `Design` → `DesignPlate` → `DesignMatching` — A design has N plates, each plate has N color matchings (`MatchingNo` + `Colour`). A unique index enforces `(DesignPlateId, MatchingNo)` — no duplicate matching numbers per plate.
- `ProgramEntry` (table: `Program`) — A production run linked to a `Party`. References matchings via `ProgramMatching` rows, which store `DesignId`, `PlateId`, `DesignMatchingId`, `MatchingNo`, and `Colour` as a **denormalized snapshot**. On save, the entire `ProgramMatchings` child collection is deleted and re-inserted.
- `DispatchEntry` (table: `DispatchEntries`) — Records dispatches of catalogue items. `Status` is a plain string; known values: `"Pending"`, `"Ok"`. Both statuses reduce catalogue stock.
- `Party` — Customer or supplier.
- `CompanyProfile` — Singleton row (always accessed via `FirstOrDefault()`). Stores company info, logo, `LetterheadHtml` (raw HTML injected via `@Html.Raw` in print views), and `ThemeColor` (CSS variable `--theme-color` used in all print layouts).

**Catalogue stock formula:**
```
Stock = Sum(InventoryTransaction where Stock type)
      - Sum(InventoryTransaction where Order type)
      - Sum(DispatchEntry.Qty where Status = "Ok" or "Pending")
```

**ProgramController.Save matching resolution:** `SelectedMatchings` is posted as `"DesignId|MatchingNo"` strings. The controller resolves these against live `DesignMatching` rows and writes denormalized `ProgramMatching` rows.

### ApplicationDbContext — DbSet Reference

| Property | Entity | Table |
|---|---|---|
| `Catalogues` | `Catalogue` | `Catalogues` |
| `InventoryTransactions` | `InventoryTransaction` | `InventoryTransactions` |
| `Designs` | `Design` | `Designs` |
| `DesignPlates` | `DesignPlate` | `DesignPlates` |
| `DesignMatchings` | `DesignMatching` | `DesignMatchings` |
| `Party` | `Party` | `Party` |
| `Program` | `ProgramEntry` | `Program` |
| `ProgramMatchings` | `ProgramMatching` | `ProgramMatchings` |
| `DispatchEntries` | `DispatchEntry` | `DispatchEntries` |
| `CompanyProfile` | `CompanyProfile` | `CompanyProfile` |
| `RolePermissions` | `RolePermission` | `RolePermissions` |

All `DbSet<T>` properties use expression bodies: `public DbSet<T> Xs => Set<T>();`

## File Uploads

Uploaded files are stored under `wwwroot/uploads/`:
- Catalogue photos/PDFs → `wwwroot/uploads/catalogues/`
- Program photos → `wwwroot/uploads/programs/`
- Company logo → `wwwroot/uploads/company/`

Filenames are generated as `photo_{Guid}.ext` or `doc_{Guid}.ext` (company: `logo_{Guid}.ext`). Program photos are validated: jpg/jpeg/png only, max 3 MB.

## Print / PDF Export

A separate print layout `Views/Shared/_Layout_Print.cshtml` is used for all print views. It reads ViewBag properties: `PageSize` (default A4), `PageOrientation`, `PageMargin`, `FontSize`, `FontFamily`. Every print view embeds `<partial name="_LetterheadPartial" />` which injects `CompanyProfile` data and `ThemeColor` as a CSS variable.

Print views: `Design/Print_Matching.cshtml`, `Program/Print_Program.cshtml`, `Program/Print_DesignMatching.cshtml`. Client-side PDF export uses **html2pdf.js** (CDN) — there is no server-side PDF generation.

## Identity & Authorization

All controllers carry `[Authorize]`. The `AccountController.cs` is excluded from compilation (`<Compile Remove=...>` in the csproj); authentication UI is handled by scaffolded Razor Pages under `Areas/Identity`. Password policy: min length 8, lockout after 5 failed attempts.

`ApplicationUser` extends `IdentityUser` with `string FullName` (in `Inventory.Infrastructure/Entities/`). Identity is registered with `AddIdentity<ApplicationUser, IdentityRole>` (not `AddDefaultIdentity`) so that `RoleManager` is available.

`IUserRoleService` (registered as scoped) wraps `UserManager` and `RoleManager` for role/user CRUD used by `RolesController` and `UsersController`.

## Pagination

`PaginatedList<T>` (in `Inventory.Domain/ViewModels/PaginatedList.cs`) is used across all list views. Default page size is 20 for Designs/Programs, 10 for Catalogues. `page` and `pageSize` are passed as query parameters.

## Key EF / Controller Patterns

- Use `AsNoTracking()` on all read queries in Index/GET actions.
- Use `EF.Functions.ILike` for case-insensitive search in PostgreSQL (not `.Contains()`). Note: `CatalogueController` currently uses `EF.Functions.Like` (case-sensitive) — use `ILike` for new code.
- `DateOnly` for business dates throughout; `DateTime` (always UTC) for timestamps — use `DateTime.SpecifyKind(..., DateTimeKind.Utc)` on insert.
- `[ValidateNever]` on navigation properties when the entity is used directly as a form model binder.
- AJAX JSON actions return `Json(new { success = true, id = x })` or `Json(new { success = false, error = ex.Message })`. Form POSTs use `TempData["Success"]` + `RedirectToAction`.
- `DispatchController` uses an AJAX row-save pattern (`SaveRow`/`DeleteRow` JSON endpoints) — the only controller without a traditional Add/Edit form page.

## Frontend Libraries (CDN)

Loaded from CDN in `_Layout.cshtml` — no local npm/webpack build:
- Bootstrap 5.3.0 (CSS + JS bundle)
- Bootstrap Icons
- Select2 4.1.0-rc.0 + select2-bootstrap-5-theme 1.3.0
- html2pdf.js 0.9.2 (print views only)
