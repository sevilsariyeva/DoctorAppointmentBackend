namespace DoctorAppointment.Models.Dtos
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }

        public ServiceResponse(T data, string message = "", bool success = true)
        {
            Data = data;
            Message = message;
            Success = success;
        }
    }
}
