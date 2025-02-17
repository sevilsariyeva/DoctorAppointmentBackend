using DoctorAppointment.Models.Dtos;

namespace DoctorAppointment.Services
{
    public interface IUserService
    {
        Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request);
        Task<LoginUserResponse> LoginUserAsync(LoginUserRequest request);
    }
}
