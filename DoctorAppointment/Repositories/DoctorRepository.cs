using DoctorAppointment.Models;
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

    }
}
