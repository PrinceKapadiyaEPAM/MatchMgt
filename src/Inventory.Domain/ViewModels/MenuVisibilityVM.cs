namespace Inventory.Domain.ViewModels;

public class MenuVisibilityVM
{
    public string RoleName { get; set; } = string.Empty;
    public List<MenuItemVisibilityVM> Items { get; set; } = new();
}

public class MenuItemVisibilityVM
{
    public string Module { get; set; } = string.Empty;
    public bool IsMenuVisible { get; set; }
}
