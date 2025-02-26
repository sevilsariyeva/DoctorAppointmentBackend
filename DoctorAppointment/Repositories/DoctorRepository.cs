using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DoctorAppointment.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly IMongoCollection<Doctor> _doctorsCollection;

        public DoctorRepository(MongoDbService mongoDbService)
        {
            _doctorsCollection = mongoDbService.GetCollection<Doctor>("doctors"); 
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
                Image = d.Image,
                Id = d.Id.ToString()
            }).ToList();
        }
        public async Task<Doctor> GetDoctorByIdAsync(string doctorId)
        {

            var filter = Builders<Doctor>.Filter.Eq(d => d.Id, doctorId);

            return await _doctorsCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<Doctor> GetDoctorByEmailAsync(string email)
        {
            return await _doctorsCollection.Find(doctor => doctor.Email == email).FirstOrDefaultAsync();
        }
        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            var filter = Builders<Doctor>.Filter.Eq(d => d.Id, doctor.Id);
            await _doctorsCollection.ReplaceOneAsync(filter, doctor);
        }

    }
}
