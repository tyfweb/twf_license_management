using System;
using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

public interface IAuthenticationService
{
    /// <summary>
    /// Validates user credentials and returns user profile if valid
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<bool> ValidateUserAsync(string username, string password);
    /// <summary>
    /// Retrieves user profile by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Task<UserProfile?> GetUserAsync(string username);
    /// <summary>
    /// Signs in the user and updates last login time
    /// </summary>
    /// <param name="user"></param>
    /// <param name="rememberMe"></param>
    /// <returns></returns>
    Task SignInAsync(UserProfile user, bool rememberMe);
    /// <summary>
    /// Signs out the user and updates last login time
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Task SignOutAsync(string username);
}
