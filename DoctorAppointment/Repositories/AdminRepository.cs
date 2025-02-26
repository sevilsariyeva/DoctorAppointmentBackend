using MongoDB.Driver;
using DoctorAppointment.Models;
using DoctorAppointment.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DoctorAppointment.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IMongoCollection<Admin> _adminCollection;
        private readonly IMongoCollection<Appointment> _appointmentCollection;
        private readonly IMongoCollection<Doctor> _doctorCollection;
        private readonly MongoDbService _mongoDbService;

        public AdminRepository(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            _adminCollection = mongoDbService.GetDatabase().GetCollection<Admin>("admins");
            _appointmentCollection = mongoDbService.GetDatabase().GetCollection<Appointment>("appointments");
            _doctorCollection = mongoDbService.GetDatabase().GetCollection<Doctor>("doctors");
        }

        public async Task<Admin> GetAdminByEmailAsync(string email)
        {
            return await _adminCollection.Find(admin => admin.Email == email).FirstOrDefaultAsync();
        }
        public async Task<Doctor> GetDoctorByEmailAsync(string email)
        {
            return await _doctorCollection.Find(d => d.Email == email).FirstOrDefaultAsync();
        }

        public async Task<Doctor> AddDoctorAsync(Doctor doctor)
        {
            try
            {
                Console.WriteLine($"Inserting doctor: {JsonConvert.SerializeObject(doctor)}");

                await _doctorCollection.InsertOneAsync(doctor);
                return doctor;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting doctor: {ex.Message}");
                throw;
            }
        }


        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _appointmentCollection.Find(_ => true).ToListAsync();
        }

        public async Task<long> GetDoctorsCountAsync()
        {
            return await _doctorCollection.CountDocumentsAsync(FilterDefinition<Doctor>.Empty);
        }

        public async Task<long> GetAppointmentsCountAsync()
        {
            return await _appointmentCollection.CountDocumentsAsync(FilterDefinition<Appointment>.Empty);
        }

        public async Task<long> GetPatientsCountAsync()
        {
            var usersCollection = _mongoDbService.GetDatabase().GetCollection<User>("users");
            return await usersCollection.CountDocumentsAsync(FilterDefinition<User>.Empty);
        }

        public async Task<List<Appointment>> GetLatestAppointmentsAsync(int count)
        {
            return await _appointmentCollection
                .Find(FilterDefinition<Appointment>.Empty)
                .SortByDescending(a => a.Date)
                .Limit(count)
                .ToListAsync();
        }


    }
}
