# .NET Feature Development — Best Practices

You are helping develop a feature in this ASP.NET Core MVC application. Follow these rules strictly.

## Project Structure
- **Domain** — Entities and ViewModels only. No dependencies on other layers.
- **Infrastructure** — `ApplicationDbContext`, EF Core migrations, services. Depends on Domain.
- **Web** — Controllers inject `ApplicationDbContext` directly (no repository layer). Views use Razor.

## Architecture Rules
- Place new entities in `src/Inventory.Domain/Entities/`
- Place ViewModels in `src/Inventory.Domain/ViewModels/`
- Register `DbSet<T>` in `src/Inventory.Infrastructure/ApplicationDbContext.cs`
- Controllers go in `src/Inventory.Web/Controllers/` with `[Authorize]` attribute
- Views go in `src/Inventory.Web/Views/{ControllerName}/`

## EF Core / Database
- Always run migrations after entity changes:
  ```
  dotnet ef migrations add <Name> --project src/Inventory.Infrastructure --startup-project src/Inventory.Web
  dotnet ef database update --project src/Inventory.Infrastructure --startup-project src/Inventory.Web
  ```
- Use `DateOnly` for date-only fields, `DateTime` for timestamps
- For PostgreSQL timestamps always use `DateTime.SpecifyKind(..., DateTimeKind.Utc)`
- Nullable reference types: use `string?`, `int?`, `decimal?` for optional fields

## Controller Patterns
- Return `Json(new { success = true, id = ... })` for AJAX actions
- Catch exceptions and return `Json(new { success = false, error = ex.Message })`
- Use `[HttpPost]` and `[FromBody]` for JSON API actions
- Use `TempData["Success"]` for success messages after redirects
- Never skip `[ValidateAntiForgeryToken]` on form POST actions

## View Patterns
- Never use C# ternary inside HTML attribute areas (causes RZ1031) — use `data-*` attributes and set values via JS instead
- Use Bootstrap 5 classes for layout and components
- Inline-editable tables: use `fetch` POST with JSON, update DOM on success without page reload
- For select pre-selection on existing rows: store value in `data-*` on `<tr>`, set via JS `bindRow()`

## Clean Code
- No comments unless the WHY is non-obvious
- No unused variables, imports, or dead code
- Keep controller actions focused — one responsibility per action
- Use `async/await` consistently throughout
- Prefer `var` for local variables when type is obvious

## Checklist Before Finishing
- [ ] Entity added to DbContext
- [ ] Migration created and applied
- [ ] Controller has `[Authorize]`
- [ ] No hardcoded strings that should be constants
- [ ] AJAX error cases handled in JS (`alert` on failure)
- [ ] New nav link added to `_Layout.cshtml` if new module

$ARGUMENTS
