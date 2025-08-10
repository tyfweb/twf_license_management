using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Helpers;

namespace TechWayFit.Licensing.Management.Services.Implementations.User;

/// <summary>
/// Service implementation for user management operations
/// Using Unit of Work pattern for consistent data access
/// </summary>
public class UserService : IUserService
{ 
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService( 
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    { 
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger;
    }

    #region User Management

    public async Task<IEnumerable<UserProfile>> GetAllUsersAsync(
        string? searchTerm = null,
        string? departmentFilter = null,
        string? roleFilter = null,
        bool? isLockedFilter = null,
        bool? isAdminFilter = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var users = await _unitOfWork.Users.SearchUsersAsync(
                searchTerm,
                departmentFilter,
                roleFilter,
                isLockedFilter,
                isAdminFilter,
                page,
                pageSize
            );
            return users.Users ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with filters");
            return new List<UserProfile>();
        }
    }

    public async Task<int> GetUserCountAsync(
        string? searchTerm = null,
        string? departmentFilter = null,
        string? roleFilter = null,
        bool? isLockedFilter = null,
        bool? isAdminFilter = null)
    {
        try
        {
            var users = await _unitOfWork.Users.SearchUsersAsync(
                        searchTerm,
                        departmentFilter,
                        roleFilter,
                        isLockedFilter,
                        isAdminFilter
            );
             
            return users.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count");
            return 0;
        }
    }

    public async Task<UserProfile?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<UserProfile?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username: {Username}", username);
            return null;
        }
    }

    public async Task<UserProfile?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<(bool Success, string Message, UserProfile? User)> CreateUserAsync(
        string username,
        string password,
        string fullName,
        string email,
        string? department,
        bool isAdmin,
        List<Guid> roleIds,
        string createdBy)
    {
        try
        {
            // Validate input
            var validationResult = ValidateUserInput(username, email, password);
            if (!validationResult.IsValid)
            {
                return (false, validationResult.Message, null);
            }

            // Check if username already exists
            if (!await IsUsernameAvailableAsync(username))
            {
                return (false, "Username already exists", null);
            }

            // Check if email already exists
            if (!await IsEmailAvailableAsync(email))
            {
                return (false, "Email already exists", null);
            }

            // Validate roles exist
            var existingRoles = await _unitOfWork.UserRoles.GetActiveRolesByIdsAsync(roleIds);
            if (existingRoles.Count() != roleIds.Count())
            {
                return (false, "One or more selected roles do not exist", null);
            }

            // Create user entity
            var userEntity = new UserProfile
            {
                UserId = Guid.NewGuid(),
                UserName = username.Trim(), 
                FullName = fullName.Trim(),
                Email = email.Trim().ToLower(),
                Department = department?.Trim(),
                Password= password,
                IsAdmin = isAdmin
            };

           var userProfile= await _unitOfWork.Users.AddAsync(userEntity);

            // Create role mappings
            foreach (var roleId in roleIds)
            {
                var roleMapping = new UserRoleMapping
                {
                    UserId = userProfile.UserId,
                    RoleId = roleId,
                    AssignedDate = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    CreatedOn = DateTime.UtcNow
                };
                await _unitOfWork.UserRoleMappings.AddAsync(roleMapping);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User created successfully: {Username} by {CreatedBy}", username, createdBy);

            var createdUser = await GetUserByIdAsync(userEntity.UserId);
            return (true, "User created successfully", createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", username);
            return (false, "An error occurred while creating the user", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(
        Guid userId,
        string username,
        string fullName,
        string email,
        string? department,
        bool isAdmin,
        List<Guid> roleIds,
        string updatedBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Audit.IsDeleted)
            {
                return (false, "User not found");
            }

            // Validate input
            var validationResult = ValidateUserInput(username, email);
            if (!validationResult.IsValid)
            {
                return (false, validationResult.Message);
            }

            // Check username availability (excluding current user)
            if (!await IsUsernameAvailableAsync(username, userId))
            {
                return (false, "Username already exists");
            }

            // Check email availability (excluding current user)
            if (!await IsEmailAvailableAsync(email, userId))
            {
                return (false, "Email already exists");
            }

            // Validate roles exist
            var existingRoles = await _unitOfWork.UserRoles
                .GetActiveRolesByIdsAsync(roleIds);

            if (existingRoles.Count() != roleIds.Count())
            {
                return (false, "One or more selected roles do not exist");
            }

            // Update user properties
            user.UserName = username.Trim();
            user.FullName = fullName.Trim();
            user.Email = email.Trim().ToLower();
            user.Department = department?.Trim();
            user.IsAdmin = isAdmin; 

            // Update role mappings
            await UpdateUserRolesAsync(userId, roleIds, updatedBy);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User updated successfully: {UserId} by {UpdatedBy}", userId, updatedBy);
            return (true, "User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return (false, "An error occurred while updating the user");
        }
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(Guid userId, string deletedBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Audit.IsDeleted)
            {
                return (false, "User not found");
            }

            

            // Deactivate role mappings
            var roleMappings = await _unitOfWork.UserRoleMappings.GetByUserIdAsync(userId);

            foreach (var mapping in roleMappings)
            {
                await _unitOfWork.UserRoleMappings.DeleteAsync(mapping.MappingId);
            }
            // Soft delete
            await _unitOfWork.Users.DeleteAsync(user.UserId); 

            _logger.LogInformation("User deleted successfully: {UserId} by {DeletedBy}", userId, deletedBy);
            return (true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return (false, "An error occurred while deleting the user");
        }
    }

    public async Task<(bool Success, string Message)> SetUserLockStatusAsync(Guid userId, bool isLocked, string updatedBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Audit.IsDeleted)
            {
                return (false, "User not found");
            }

            user.IsLocked = isLocked;
            user.LockedDate = isLocked ? DateTime.UtcNow : null; 

            // Reset failed login attempts when unlocking
            if (!isLocked)
            {
                user.FailedLoginAttempts = 0;
            }
            await _unitOfWork.Users.UpdateAsync(user.UserId,user); 

            var action = isLocked ? "locked" : "unlocked";
            _logger.LogInformation("User {Action}: {UserId} by {UpdatedBy}", action, userId, updatedBy);
            return (true, $"User {action} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user lock status: {UserId}", userId);
            return (false, "An error occurred while updating user lock status");
        }
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string updatedBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Audit.IsDeleted)
            {
                return (false, "User not found");
            }
            if (await _unitOfWork.Users.ValidatePasswordAsync(user.UserName, currentPassword))
            {
                    return (false, "Current password is incorrect");                
            }

            // Validate new password
                var passwordValidation = ValidatePassword(newPassword);
            if (!passwordValidation.IsValid)
            {
                return (false, passwordValidation.Message);
            }

            await _unitOfWork.Users.UpdatePasswordAsync(userId, newPassword);

            _logger.LogInformation("Password changed for user: {UserId} by {UpdatedBy}", userId, updatedBy);
            return (true, "Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return (false, "An error occurred while changing the password");
        }
    }

    public async Task<(bool Success, string Message, string? TempPassword)> ResetPasswordAsync(Guid userId, string resetBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Audit.IsDeleted)
            {
                return (false, "User not found", null);
            }

            // Generate temporary password
            var tempPassword = SecurityHelper.GenerateTemporaryPassword();
            
            await _unitOfWork.Users.UpdatePasswordAsync(userId, tempPassword);

            _logger.LogInformation("Password reset for user: {UserId} by {ResetBy}", userId, resetBy);
            return (true, "Password reset successfully", tempPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
            return (false, "An error occurred while resetting the password", null);
        }
    }

    public async Task<(bool Success, UserProfile? User, string Message)> AuthenticateAsync(string username, string password)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
            {
                return (false, null, "Invalid username or password");
            }

            if (user.IsLocked)
            {
                return (false, null, "Account is locked. Please contact administrator.");
            }
            var passworValid = await _unitOfWork.Users.ValidatePasswordAsync(username, password);
            if (!passworValid)
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;
                
                // Lock account after 5 failed attempts
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockedDate = DateTime.UtcNow;
                }

                await _unitOfWork.Users.UpdateAsync(user.UserId, user);
                return (false, null, "Invalid username or password");
            }

            // Successful login - reset failed attempts and update last login
            user.FailedLoginAttempts = 0;
            user.LastLoginDate = DateTime.UtcNow;
            var userProfile  = await _unitOfWork.Users.UpdateAsync(user.UserId, user);
 
            return (true, userProfile, "Authentication successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Username}", username);
            return (false, null, "An error occurred during authentication");
        }
    }

    #endregion

    #region Role Management

    public async Task<IEnumerable<UserRole>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _unitOfWork.UserRoles.GetActiveRolesAsync();

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return new List<UserRole>();
        }
    }

    public async Task<UserRole?> GetRoleByIdAsync(Guid roleId)
    {
        try
        {
            var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);

            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by ID: {RoleId}", roleId);
            return null;
        }
    }

    public async Task<UserRole?> GetRoleByNameAsync(string roleName)
    {
        try
        {
            var role = await _unitOfWork.UserRoles.GetByNameAsync(roleName);
            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
            return null;
        }
    }

    public async Task<(bool Success, string Message, UserRole? Role)> CreateRoleAsync(
        string roleName,
        string? roleDescription,
        bool isAdmin,
        string createdBy)
    {
        try
        {
            // Check if role name already exists
            var existingRole = await GetRoleByNameAsync(roleName);
            if (existingRole != null)
            {
                return (false, "Role name already exists", null);
            }

            var roleEntity = new UserRole
            {
                RoleName = roleName.Trim(),
                RoleDescription = roleDescription?.Trim(),
                IsAdmin = isAdmin
            };

            var createdRole = await _unitOfWork.UserRoles.AddAsync(roleEntity); 

            _logger.LogInformation("Role created successfully: {RoleName} by {CreatedBy}", roleName, createdBy);

            return (true, "Role created successfully", createdRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", roleName);
            return (false, "An error occurred while creating the role", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateRoleAsync(
        Guid roleId,
        string roleName,
        string? roleDescription,
        bool isAdmin,
        string updatedBy)
    {
        try
        {
            var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
            if (role == null || !role.IsActive)
            {
                return (false, "Role not found");
            }

            // Check if role name already exists (excluding current role)
            var existingRole = await _unitOfWork.UserRoles.GetByNameAsync(roleName);
            if (existingRole != null && existingRole.RoleId != roleId)
            {
                return (false, "Role name already exists");
            }

            role.RoleName = roleName.Trim();
            role.RoleDescription = roleDescription?.Trim();
            role.IsAdmin = isAdmin;
            role.UpdatedBy = updatedBy;
            role.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Role updated successfully: {RoleId} by {UpdatedBy}", roleId, updatedBy);
            return (true, "Role updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", roleId);
            return (false, "An error occurred while updating the role");
        }
    }

    public async Task<(bool Success, string Message)> DeleteRoleAsync(Guid roleId, string deletedBy)
    {
        try
        {
            var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
            if (role == null || !role.IsActive)
            {
                return (false, "Role not found");
            }

            // Check if role is assigned to any users
            var assignedUsers = await _unitOfWork.UserRoleMappings.GetByRoleIdAsync(roleId);

            if (assignedUsers.Count() > 0)
            {
                return (false, $"Cannot delete role. It is assigned to {assignedUsers} user(s).");
            }

            // Soft delete
            role.IsActive = false;
            role.UpdatedBy = deletedBy;
            role.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Role deleted successfully: {RoleId} by {DeletedBy}", roleId, deletedBy);
            return (true, "Role deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
            return (false, "An error occurred while deleting the role");
        }
    }

    public async Task InitializeDefaultRolesAsync(string createdBy = "System")
    {
        try
        {
            var defaultRoles = PredefinedRoles.GetAllRoles();

            foreach (var (name, description, isAdmin) in defaultRoles)
            {
                var existingRole = await GetRoleByNameAsync(name);
                if (existingRole == null)
                {
                    await CreateRoleAsync(name, description, isAdmin, createdBy);
                }
            }

            _logger.LogInformation("Default roles initialized by {CreatedBy}", createdBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default roles");
        }
    }

    #endregion

    #region User Role Assignments

    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            var userRoles = await _unitOfWork.UserRoleMappings.GetByUserIdAsync(userId);

            return userRoles?.Select(x => x.Role).Where(role => role != null)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles for user: {UserId}", userId);
            return new List<UserRole>();
        }
    }

    public async Task<(bool Success, string Message)> AssignRoleToUserAsync(
        Guid userId,
        Guid roleId,
        DateTime? expiryDate,
        string assignedBy)
    {
        try
        {
            // Check if user exists
            var userExists = await _unitOfWork.Users.GetByIdAsync(userId);
            if (!userExists?.Audit.IsActive ?? false)
            {
                return (false, "User not found");
            }
            // Check if role exists
            var roleExists = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
            if (roleExists == null || !roleExists.IsActive)
            {
                return (false, "Role not found");
            }

            // Check if mapping already exists
            var existingMapping = await _unitOfWork.UserRoleMappings.GetByUserAndRoleAsync(userId, roleId);

            if (existingMapping != null)
            {
                if (existingMapping.IsActive)
                {
                    return (false, "User already has this role");
                }
                else
                {
                    // Reactivate existing mapping
                    existingMapping.IsActive = true;
                    existingMapping.AssignedDate = DateTime.UtcNow;
                    existingMapping.ExpiryDate = expiryDate;
                    existingMapping.UpdatedBy = assignedBy;
                    existingMapping.UpdatedOn = DateTime.UtcNow;
                }
            }
            else
            {
                // Create new mapping
                var roleMapping = new UserRoleMapping
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedDate = DateTime.UtcNow,
                    ExpiryDate = expiryDate,
                    CreatedBy = assignedBy,
                    CreatedOn = DateTime.UtcNow
                };
                await _unitOfWork.UserRoleMappings.AddAsync(roleMapping);
            }
 

            _logger.LogInformation("Role assigned to user: {UserId} -> {RoleId} by {AssignedBy}", userId, roleId, assignedBy);
            return (true, "Role assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId} -> {RoleId}", userId, roleId);
            return (false, "An error occurred while assigning the role");
        }
    }

    public async Task<(bool Success, string Message)> RemoveRoleFromUserAsync(Guid userId, Guid roleId, string removedBy)
    {
        try
        {
            var roleMapping = await _unitOfWork.UserRoleMappings
                .GetByUserAndRoleAsync(userId, roleId);

            if (roleMapping == null)
            {
                return (false, "Role assignment not found");
            }

            roleMapping.IsActive = false;
            roleMapping.UpdatedBy = removedBy;
            roleMapping.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.UserRoleMappings.UpdateAsync(roleMapping.MappingId, roleMapping);

            _logger.LogInformation("Role removed from user: {UserId} -> {RoleId} by {RemovedBy}", userId, roleId, removedBy);
            return (true, "Role removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId} -> {RoleId}", userId, roleId);
            return (false, "An error occurred while removing the role");
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserRolesAsync(
        Guid userId,
        List<Guid> roleIds,
        string updatedBy)
    {
        try
        {
            // Get current role mappings
            var currentMappings = await _unitOfWork.UserRoleMappings
                .GetByUserIdAsync(userId);

            // Deactivate roles not in the new list
            var rolesToRemove = currentMappings
                .Where(cm => !roleIds.Contains(cm.RoleId))
                .ToList();

            foreach (var mapping in rolesToRemove)
            {
                mapping.IsActive = false;
                mapping.UpdatedBy = updatedBy;
                mapping.UpdatedOn = DateTime.UtcNow;
            }

            // Add new roles
            var currentRoleIds = currentMappings.Select(cm => cm.RoleId).ToList();
            var rolesToAdd = roleIds.Where(rid => !currentRoleIds.Contains(rid)).ToList();

            foreach (var roleId in rolesToAdd)
            {
                await AssignRoleToUserAsync(userId, roleId, null, updatedBy);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User roles updated: {UserId} by {UpdatedBy}", userId, updatedBy);
            return (true, "User roles updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user roles: {UserId}", userId);
            return (false, "An error occurred while updating user roles");
        }
    }

    #endregion

    #region Utility Methods

    public async Task<IEnumerable<string>> GetAvailableDepartmentsAsync()
    {
        try
        {
            var departments = await _unitOfWork.Users.
                GetAvailableDepartmentsAsync();

            return departments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available departments");
            return new List<string>();
        }
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);

            if (excludeUserId.HasValue && user?.UserId == excludeUserId.Value)
            {
                return true;
            }

            return user == null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability: {Username}", username);
            return false;
        }
    }


    public async Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId = null)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
            {
                return true;
            }

            if (excludeUserId.HasValue && user.UserId == excludeUserId.Value)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", email);
            return false;
        }
    }


    public (bool IsValid, string Message) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Password is required");
        }

        if (password.Length < 8)
        {
            return (false, "Password must be at least 8 characters long");
        }

        if (password.Length > 100)
        {
            return (false, "Password cannot exceed 100 characters");
        }

        // Check for at least one uppercase letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return (false, "Password must contain at least one uppercase letter");
        }

        // Check for at least one lowercase letter
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return (false, "Password must contain at least one lowercase letter");
        }

        // Check for at least one digit
        if (!Regex.IsMatch(password, @"\d"))
        {
            return (false, "Password must contain at least one number");
        }

        // Check for at least one special character
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            return (false, "Password must contain at least one special character");
        }

        return (true, "Password is valid");
    }

    #endregion

    #region Private Helper Methods

    

    private (bool IsValid, string Message) ValidateUserInput(string username, string email, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return (false, "Username is required");
        }

        if (username.Length > 50)
        {
            return (false, "Username cannot exceed 50 characters");
        }

        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]+$"))
        {
            return (false, "Username can only contain letters, numbers, underscore, dot, and hyphen");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Email is required");
        }

        if (email.Length > 255)
        {
            return (false, "Email cannot exceed 255 characters");
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return (false, "Please enter a valid email address");
        }

        if (password != null)
        {
            var passwordValidation = ValidatePassword(password);
            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }
        }

        return (true, "Valid");
    }

    

    #endregion
}
