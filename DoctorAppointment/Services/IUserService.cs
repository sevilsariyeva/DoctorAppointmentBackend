using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using System.Security.Claims;

namespace DoctorAppointment.Services
{
    public interface IUserService
    {
        Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request);
        Task<LoginUserResponse> LoginUserAsync(LoginUserRequest request);
        Task<GetProfileResponse> GetProfileAsync(string currentUserId);
        Task<UpdateUserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
        
    }
}
