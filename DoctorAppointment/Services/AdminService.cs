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
        private readonly IPasswordHasher<Doctor> _doctorpasswordHasher;
        private readonly IWebHostEnvironment _environment;

        public AdminService(IAdminRepository adminRepository, IConfiguration configuration, IPasswordHasher<Admin> passwordHasher,IWebHostEnvironment environment, IPasswordHasher<Doctor> doctorpasswordHasher)
        {
            _configuration = configuration;
            _adminRepository = adminRepository;
            _passwordHasher = passwordHasher;
            _environment = environment;
            _doctorpasswordHasher = doctorpasswordHasher;
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
        public async Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image)
        {
            string fileName = null;

            if (image != null && image.Length > 0)
            {
                var extension = Path.GetExtension(image.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    throw new ArgumentException("Invalid image format. Only support .jpg, .jpeg və .png formats.");

                fileName = Guid.NewGuid().ToString() + extension;

                var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                var filePath = Path.Combine(uploadsDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
            }

            var doctor = new Doctor
            {
                Id= Guid.NewGuid().ToString(),
                Name = doctorDto.Name,
                Email = doctorDto.Email,
                Password = doctorDto.Password,
                Speciality = doctorDto.Speciality,
                Image = fileName != null ? "/uploads/" + fileName : null,
                Degree = doctorDto.Degree,
                Experience = doctorDto.Experience,
                Fees = doctorDto.Fees,
                About = doctorDto.About,
                Address = new Address
                {
                    Line1 = doctorDto.Address1,
                    Line2 = doctorDto.Address2
                }
            };
            var hashedPassword = _doctorpasswordHasher.HashPassword(doctor, doctorDto.Password);
            doctor.Password = hashedPassword;
            await _adminRepository.AddDoctorAsync(doctor);

            return doctor;
        }

        private string GenerateJwtToken(Admin admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:SecretKey"];
            var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes");

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
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<int> GetDoctorsCountAsync()
        {
            return await _adminRepository.GetDoctorsCountAsync();
        }

        public async Task<int> GetAppointmentsCountAsync()
        {
            return await _adminRepository.GetAppointmentsCountAsync();
        }

        public async Task<int> GetPatientsCountAsync()
        {
            return await _adminRepository.GetPatientsCountAsync();
        }

        public async Task<List<Appointment>> GetLatestAppointmentsAsync(int count)
        {
            return await _adminRepository.GetLatestAppointmentsAsync(count);
        }


    }
}
