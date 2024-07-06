using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMDb.DTOs;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;
using MyMDb.ServiceInterfaces;

namespace MyMDb.Services
{
    public class UserService : IUserService
    {
        public readonly IUserRepository _userRepository;
        public readonly UserManager<AppUser> _userManager;
        
        public UserService(IUserRepository userRepository, UserManager<AppUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<UserProfile?> CreateUserProfileAsync(UserProfile profile)
        {
            if(profile.UserId == null)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(profile.UserId);
            if (user == null)
            {
                return null;
            }

            if (await _userRepository.GetProfileByUserIdAsync(profile.UserId) != null) 
            {
                return null;
            }

            var userProfile = new UserProfile();
            userProfile.UserId = profile.UserId;
            userProfile.ProfilePicPath = profile.ProfilePicPath;
            userProfile.UserName = profile.UserName;

            return await _userRepository.AddProfileAsync(userProfile);
        }

        public async Task<UserProfile?> EditUserProfileAsync(string userId, UserProfile editedProfile)
        {
            var profile = await GetUserProfileAsync(userId);
            if (profile == null)
            {
                return null;
            }
            
            return await _userRepository.UpdateProfileAsync(profile);
        }

        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            return await _userRepository.GetProfileByUserIdAsync(userId); 
        }
    }
}
