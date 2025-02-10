using MongoDB.Driver;
using DoctorAppointment.Models;
using DoctorAppointment.Services;

namespace DoctorAppointment.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IMongoCollection<Admin> _adminCollection;
        private readonly MongoDbService _mongoDbService;

        public AdminRepository(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            _adminCollection = mongoDbService.GetDatabase().GetCollection<Admin>("admins");
        }

        public async Task<Admin> GetAdminByEmailAsync(string email)
        {
            return await _adminCollection.Find(admin => admin.Email == email).FirstOrDefaultAsync();
        }
    }
}
