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
using Microsoft.AspNetCore.Identity.Data;
using DoctorAppointment.Utilities;

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
        public async Task<string> LoginAdmin(LoginRequest request)
        {
            var admin = await _adminRepository.GetAdminByEmailAsync(request.Email);

            if (admin == null || admin.Password != request.Password)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            return JwtTokenGenerator.GenerateToken(admin.Id.ToString(), admin.Email, "Admin", _configuration);
        }
        public async Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image)
        {
            var doctor = MapToDoctorEntity(doctorDto);
            doctor.Image = await SaveImageAsync(image);
            doctor.Password = _doctorpasswordHasher.HashPassword(doctor, doctorDto.Password);

            await _adminRepository.AddDoctorAsync(doctor);
            return doctor;
        }

        private Doctor MapToDoctorEntity(DoctorDto doctorDto)
        {
            return new Doctor
            {
                Id = Guid.NewGuid().ToString(),
                Name = doctorDto.Name,
                Email = doctorDto.Email,
                Speciality = doctorDto.Speciality,
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
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0) return null;

            var extension = Path.GetExtension(image.FileName).ToLower();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid image format. Only .jpg, .jpeg, and .png are supported.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");

            Directory.CreateDirectory(uploadsDirectory); 

            var filePath = Path.Combine(uploadsDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }

        //public async Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image)
        //{
        //    string fileName = null;

        //    if (image != null && image.Length > 0)
        //    {
        //        var extension = Path.GetExtension(image.FileName).ToLower();
        //        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        //            throw new ArgumentException("Invalid image format. Only support .jpg, .jpeg və .png formats.");

        //        fileName = Guid.NewGuid().ToString() + extension;

        //        var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");

        //        if (!Directory.Exists(uploadsDirectory))
        //        {
        //            Directory.CreateDirectory(uploadsDirectory);
        //        }

        //        var filePath = Path.Combine(uploadsDirectory, fileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await image.CopyToAsync(stream);
        //        }
        //    }

        //    var doctor = new Doctor
        //    {
        //        Id= Guid.NewGuid().ToString(),
        //        Name = doctorDto.Name,
        //        Email = doctorDto.Email,
        //        Password = doctorDto.Password,
        //        Speciality = doctorDto.Speciality,
        //        Image = fileName != null ? "/uploads/" + fileName : null,
        //        Degree = doctorDto.Degree,
        //        Experience = doctorDto.Experience,
        //        Fees = doctorDto.Fees,
        //        About = doctorDto.About,
        //        Address = new Address
        //        {
        //            Line1 = doctorDto.Address1,
        //            Line2 = doctorDto.Address2
        //        }
        //    };
        //    var hashedPassword = _doctorpasswordHasher.HashPassword(doctor, doctorDto.Password);
        //    doctor.Password = hashedPassword;
        //    await _adminRepository.AddDoctorAsync(doctor);

        //    return doctor;
        //}


        public async Task<AdminDashboardDto> GetAdminDashboardStatisticsAsync(int latestAppointmentsCount)
        {
            var statistics = new AdminDashboardDto
            {
                Doctors = await _adminRepository.GetDoctorsCountAsync(),
                Appointments = await _adminRepository.GetAppointmentsCountAsync(),
                Patients = await _adminRepository.GetPatientsCountAsync(),
                LatestAppointments = await _adminRepository.GetLatestAppointmentsAsync(latestAppointmentsCount)
            };

            return statistics;
        }


    }
}
