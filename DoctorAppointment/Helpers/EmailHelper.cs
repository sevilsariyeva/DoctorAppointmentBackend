using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DoctorAppointment.Helpers

{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class EmailHelper
    {
        private static readonly string apiKey = "95ade8352ba4bc502047b3ad7072ca7aac7f2248";


public static async Task<bool> EmailExists(string email)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.hunter.io/v2/email-verifier?email={email}&api_key={apiKey}";
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string status = json["data"]?["status"]?.ToString();

                return status == "valid";
            }
        }
        return false;
    }


}

}