using DoctorAppointment.Repositories;
using DoctorAppointment.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DoctorAppointment.Models.Dtos;

namespace DoctorAppointment.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<Admin> _passwordHasher;

        public AdminService(IAdminRepository adminRepository, IConfiguration configuration, IPasswordHasher<Admin> passwordHasher)
        {
            _configuration = configuration;
            _adminRepository = adminRepository;
            _passwordHasher =passwordHasher;
        }

        public async Task<string> LoginAdmin(string email, string password)
        {
            var admin = await _adminRepository.GetAdminByEmailAsync(email);  

            if (admin == null || admin.Password != password)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var token = GenerateJwtToken(admin);
            return token;
        }

        private string GenerateJwtToken(Admin admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Role, "Admin")  
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<ServiceResponse<List<Appointment>>> GetUserAppointmentsAsync()
        {
            try
            {
                var appointments = await _adminRepository.GetAllAppointmentsAsync();

                if (appointments == null || appointments.Count == 0)
                {
                    return new ServiceResponse<List<Appointment>>(null, "No appointments found.", false);
                }

                return new ServiceResponse<List<Appointment>>(appointments, "Appointments retrieved successfully.", true);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Appointment>>(null, $"Error occurred: {ex.Message}", false);
            }
        }

    }
}
