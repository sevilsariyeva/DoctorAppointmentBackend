using MongoDB.Driver;
using DoctorAppointment.Models;
using DoctorAppointment.Services;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointment.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IMongoCollection<Admin> _adminCollection;
        private readonly IMongoCollection<Appointment> _appointmentCollection;
        private readonly MongoDbService _mongoDbService;

        public AdminRepository(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            _adminCollection = mongoDbService.GetDatabase().GetCollection<Admin>("admins");
            _appointmentCollection = mongoDbService.GetDatabase().GetCollection<Appointment>("appointments");
        }

        public async Task<Admin> GetAdminByEmailAsync(string email)
        {
            return await _adminCollection.Find(admin => admin.Email == email).FirstOrDefaultAsync();
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            try
            {
                var appointments = await _appointmentCollection.Find(_ => true).ToListAsync();

                return appointments;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching appointments: {ex.Message}");
            }
        }

    }
}
