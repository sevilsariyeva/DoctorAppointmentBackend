using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using Microsoft.AspNetCore.Identity.Data;
using System.Security.Claims;

namespace DoctorAppointment.Services
{
    public interface IUserService
    {
        Task<JwtTokenResponse> RegisterUserAsync(RegisterUserRequest request);
        Task<JwtTokenResponse> LoginUserAsync(LoginRequest request);
        Task<User> GetProfileAsync(string currentUserId);
        Task<User> UpdateUserAsync(string userId, UpdateUserRequest request);


    }
}
