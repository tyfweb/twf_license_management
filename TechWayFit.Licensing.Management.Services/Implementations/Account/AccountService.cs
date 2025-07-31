using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Services.Implementations.Account;

public class AccountService : IAuthenticationService
{
    private readonly IUserProfileRepository _userProfileRepository; 
    private readonly ILogger<AccountService> _logger;
    public AccountService(IUserProfileRepository? userProfileRepository, ILogger<AccountService> logger)
    {
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<UserProfile?> GetUserAsync(string username)
    {
        var user = await _userProfileRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return null;
        }
        return user.ToModel();
    }

    public Task SignInAsync(UserProfile user, bool rememberMe)
    {
        _userProfileRepository.UpdateLastLoginAsync(user.UserId);
        _logger.LogInformation("User {Username} signed in successfully", user.UserName);
        return Task.CompletedTask;
    }

    public async Task SignOutAsync(string username)
    {
        var user = await _userProfileRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("User not found for sign out: {Username}", username);
            return;
        }
        await _userProfileRepository.UpdateLastLoginAsync(user.UserId);
        _logger.LogInformation("User {Username} signed out successfully", username);
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var isValid = await _userProfileRepository.ValidatePasswordAsync(username, password);
        if (!isValid)
        {
            _logger.LogWarning("Invalid login attempt for user: {Username}", username);
            return false;
        }

        _logger.LogInformation("User {Username} validated successfully", username);
        return true;
    }
}
 
