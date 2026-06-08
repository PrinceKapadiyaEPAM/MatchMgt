namespace Inventory.Domain.ViewModels;

public class PermissionMatrixVM
{
    public string RoleName { get; set; } = string.Empty;
    public List<ModulePermissionVM> Modules { get; set; } = new();
}

public class ModulePermissionVM
{
    public string Module { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
