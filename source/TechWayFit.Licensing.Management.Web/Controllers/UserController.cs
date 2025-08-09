using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Management.Web.Helpers;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Controller for user management operations
/// </summary>
[Authorize(Roles = "Administrator")]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Display list of users
    /// </summary>
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? departmentFilter,
        string? roleFilter,
        bool? isLockedFilter,
        bool? isAdminFilter,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(
                searchTerm, departmentFilter, roleFilter, isLockedFilter, isAdminFilter, page, pageSize);
            
            var totalUsers = await _userService.GetUserCountAsync(
                searchTerm, departmentFilter, roleFilter, isLockedFilter, isAdminFilter);

            var departments = await _userService.GetAvailableDepartmentsAsync();
            var roles = await _userService.GetAllRolesAsync();

            var viewModel = new UserListViewModel
            {
                Users = users.ToList(),
                SearchTerm = searchTerm,
                DepartmentFilter = departmentFilter,
                RoleFilter = roleFilter,
                IsLockedFilter = isLockedFilter,
                IsAdminFilter = isAdminFilter,
                CurrentPage = page,
                PageSize = pageSize,
                TotalUsers = totalUsers,
                AvailableDepartments = departments.ToList(),
                AvailableRoles = roles.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users list");
            TempData["ErrorMessage"] = "An error occurred while loading users.";
            return View(new UserListViewModel());
        }
    }

    /// <summary>
    /// Display user details
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await _userService.GetUserRolesAsync(id);
            
            var viewModel = new UserDetailsViewModel
            {
                User = user,
                UserRoles = userRoles.ToList(),
                LastLoginDate = user.LastLoginDate,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LockedDate = user.LockedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user details for ID: {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading user details.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display create user form
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            var roles = await _userService.GetAllRolesAsync();
            
            var viewModel = new CreateUserViewModel
            {
                AvailableRoles = roles.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create user form");
            TempData["ErrorMessage"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handle create user form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = (await _userService.GetAllRolesAsync()).ToList();
                return View(model);
            }

            var currentUser = GetCurrentUsername(); // You'll need to implement this based on your auth system
            
            var result = await _userService.CreateUserAsync(
                model.UserName,
                model.Password,
                model.FullName,
                model.Email,
                model.Department,
                model.IsAdmin,
                model.SelectedRoleIds,
                currentUser);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.User?.UserId });
            }

            ModelState.AddModelError("", result.Message);
            model.AvailableRoles = (await _userService.GetAllRolesAsync()).ToList();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            ModelState.AddModelError("", "An error occurred while creating the user.");
            model.AvailableRoles = (await _userService.GetAllRolesAsync()).ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Display edit user form
    /// </summary>
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userService.GetAllRolesAsync();
            var userRoles = await _userService.GetUserRolesAsync(id);

            var viewModel = new EditUserViewModel
            {
                UserId = user.UserId.ConvertToString(),
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Department = user.Department,
                IsLocked = user.IsLocked,
                IsAdmin = user.IsAdmin,
                SelectedRoleIds = userRoles.Select(r => r.RoleId).ToList(),
                AvailableRoles = roles.ToList(),
                CurrentRoles = userRoles.ToList(),
                CreatedOn = user.Audit.CreatedOn,
                CreatedBy = user.Audit.CreatedBy,
                UpdatedOn = user.Audit.UpdatedOn,
                UpdatedBy = user.Audit.UpdatedBy,
                LastLoginDate = user.LastLoginDate,
                FailedLoginAttempts = user.FailedLoginAttempts
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit user form for ID: {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the user.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handle edit user form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var roles = await _userService.GetAllRolesAsync();
                var userRoles = await _userService.GetUserRolesAsync(model.UserId.ToGuid());
                
                model.AvailableRoles = roles.ToList();
                model.CurrentRoles = userRoles.ToList();
                return View(model);
            }

            var currentUser = GetCurrentUsername();
            
            var result = await _userService.UpdateUserAsync(
                model.UserId.ToGuid(),
                model.UserName,
                model.FullName,
                model.Email,
                model.Department,
                model.IsAdmin,
                model.SelectedRoleIds,
                currentUser);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = model.UserId });
            }

            ModelState.AddModelError("", result.Message);
            
            var allRoles = await _userService.GetAllRolesAsync();
            var currentUserRoles = await _userService.GetUserRolesAsync(model.UserId.ToGuid());

            model.AvailableRoles = allRoles.ToList();
            model.CurrentRoles = currentUserRoles.ToList();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", model.UserId);
            ModelState.AddModelError("", "An error occurred while updating the user.");
            
            var roles = await _userService.GetAllRolesAsync();
            var userRoles = await _userService.GetUserRolesAsync(model.UserId.ToGuid());

            model.AvailableRoles = roles.ToList();
            model.CurrentRoles = userRoles.ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Handle user deletion
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var currentUser = GetCurrentUsername();
            var result = await _userService.DeleteUserAsync(id, currentUser);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the user.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handle user lock/unlock
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(Guid id, bool isLocked)
    {
        try
        {
            var currentUser = GetCurrentUsername();
            var result = await _userService.SetUserLockStatusAsync(id, isLocked, currentUser);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user lock status: {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while updating user lock status.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Display change password form
    /// </summary>
    public async Task<IActionResult> ChangePassword(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ChangePasswordViewModel
            {
                UserId = user.UserId.ConvertToString(),
                UserName = user.UserName
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading change password form for user: {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Handle change password form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = GetCurrentUsername();
            var result = await _userService.ChangePasswordAsync(
                model.UserId.ToGuid(),
                model.CurrentPassword,
                model.NewPassword,
                currentUser);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = model.UserId });
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", model.UserId);
            ModelState.AddModelError("", "An error occurred while changing the password.");
            return View(model);
        }
    }

    /// <summary>
    /// Handle password reset (admin only)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        try
        {
            var currentUser = GetCurrentUsername();
            var result = await _userService.ResetPasswordAsync(id, currentUser);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{result.Message}. Temporary password: {result.TempPassword}";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user: {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while resetting the password.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// API endpoint to check username availability
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckUsernameAvailability(string username, Guid? excludeUserId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new { available = false, message = "Username is required" });
            }

            var isAvailable = await _userService.IsUsernameAvailableAsync(username, excludeUserId);
            return Json(new { available = isAvailable, message = isAvailable ? "Username is available" : "Username is already taken" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability: {Username}", username);
            return Json(new { available = false, message = "Error checking username availability" });
        }
    }

    /// <summary>
    /// API endpoint to check email availability
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckEmailAvailability(string email, Guid? excludeUserId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { available = false, message = "Email is required" });
            }

            var isAvailable = await _userService.IsEmailAvailableAsync(email, excludeUserId);
            return Json(new { available = isAvailable, message = isAvailable ? "Email is available" : "Email is already in use" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", email);
            return Json(new { available = false, message = "Error checking email availability" });
        }
    }

    /// <summary>
    /// Get current username from authentication context
    /// TODO: Replace with actual authentication implementation
    /// </summary>
    private string GetCurrentUsername()
    {
        // TODO: Replace with actual user authentication logic
        // This should get the current authenticated user's username
        return User?.Identity?.Name ?? "System";
    }
}
