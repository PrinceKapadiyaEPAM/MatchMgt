namespace Inventory.Domain.Constants;

public static class AppModules
{
    public const string Catalogue = "Catalogue";
    public const string Inventory = "Inventory";
    public const string Party     = "Party";
    public const string Design    = "Design";
    public const string Program   = "Program";
    public const string Dispatch  = "Dispatch";

    public static readonly IReadOnlyList<string> All =
        [Catalogue, Inventory, Party, Design, Program, Dispatch];
}
