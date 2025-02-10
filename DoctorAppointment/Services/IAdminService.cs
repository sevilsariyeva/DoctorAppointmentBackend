namespace DoctorAppointment.Services
{
    public interface IAdminService
    {
        Task<string> LoginAdmin(string email, string password);
    }
}
