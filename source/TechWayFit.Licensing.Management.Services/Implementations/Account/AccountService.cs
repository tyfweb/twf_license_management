using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Services.Implementations.Account;

public class AccountService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountService> _logger;
    
    public AccountService(IUnitOfWork unitOfWork, ILogger<AccountService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<UserProfile?> GetUserAsync(string username)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return null;
        }
        return user.ToModel();
    }

    public async Task SignInAsync(UserProfile user, bool rememberMe)
    {
        await _unitOfWork.Users.UpdateLastLoginAsync(user.UserId);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {Username} signed in successfully", user.UserName);
    }

    public async Task SignOutAsync(string username)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("User not found for sign out: {Username}", username);
            return;
        }
        await _unitOfWork.Users.UpdateLastLoginAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {Username} signed out successfully", username);
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var isValid = await _unitOfWork.Users.ValidatePasswordAsync(username, password);
        if (!isValid)
        {
            _logger.LogWarning("Invalid login attempt for user: {Username}", username);
            return false;
        }

        _logger.LogInformation("User {Username} validated successfully", username);
        return true;
    }
}
 
