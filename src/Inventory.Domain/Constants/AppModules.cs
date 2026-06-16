namespace Inventory.Domain.Constants;

public static class AppModules
{
    public const string Catalogue = "Catalogue";
    public const string Inventory = "Inventory";
    public const string Party     = "Party";
    public const string Design    = "Design";
    public const string Program   = "Program";
    public const string Dispatch  = "Dispatch";
    public const string B2BUsers  = "B2BUsers";
    public const string Category  = "Category";

    public static readonly IReadOnlyList<string> All =
        [Catalogue, Inventory, Party, Design, Program, Dispatch, B2BUsers, Category];
}
