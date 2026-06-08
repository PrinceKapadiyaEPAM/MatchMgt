namespace Inventory.Infrastructure.Services;

using Inventory.Domain.ViewModels;

public interface IUserManagementService
{
    Task<List<UserListItemVM>> GetAllUsersAsync();
    Task<UserListItemVM?> GetUserByIdAsync(string id);
    Task<EditUserVM?> GetUserForEditAsync(string id);
    Task<List<string>> GetAllRoleNamesAsync();
    Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(CreateUserVM vm);
    Task<(bool Success, IEnumerable<string> Errors)> UpdateUserAsync(EditUserVM vm);
    Task<bool> DeleteUserAsync(string id);
}
