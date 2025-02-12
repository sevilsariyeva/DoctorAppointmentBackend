using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DoctorAppointment.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoCollection<Doctor> _doctorsCollection;

        public DoctorRepository(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            _doctorsCollection = _mongoDbService.GetCollection<Doctor>("doctors"); 
        }

        public async Task<Doctor> AddDoctorAsync(Doctor doctor)
        {
            await _doctorsCollection.InsertOneAsync(doctor);
            return doctor;
        }
        public async Task<List<DoctorDto>> GetAllDoctorsAsync()
        {
            var doctors = await _doctorsCollection.Find(_ => true).ToListAsync(); 

            return doctors.Select(d => new DoctorDto
            {
                Name = d.Name,
                Email = d.Email,
                Speciality = d.Speciality,
                Degree = d.Degree,
                Experience = d.Experience,
                Fees = d.Fees,
                About = d.About,
                Available=d.Available,
                Address1 = d.Address?.Line1,
                Address2 = d.Address?.Line2,
                Image = d.Image
            }).ToList();
        }

    }
}
