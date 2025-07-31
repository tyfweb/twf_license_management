using System;
using TechWayFit.Licensing.Management.Core.Models.User; 

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

public interface IAuthenticationService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<UserProfile?> GetUserAsync(string username);
        Task SignInAsync(UserProfile user, bool rememberMe);
        Task SignOutAsync(string username);
    }
