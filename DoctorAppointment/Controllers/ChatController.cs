using DoctorAppointment.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private const string OpenAiApiKey = "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";
    private const string OpenAiUrl = "https://api.openai.com/v1/chat/completions";

    private static readonly Dictionary<string, string> commonQuestions = new()
    {
        { "How do I book an appointment?", "Go to the 'All Doctors' menu, select your desired doctor, and click on 'Book Appointment' on their profile page." },
        { "What doctors are available?", "We have various specialists on our platform. You can view the full list under the 'All Doctors' section." },
        { "What are your working hours?", "Our platform is available 24/7, but each doctor's working hours vary. You can check their schedule on their profile page." }
    };

    public ChatController()
    {
        _httpClient = new HttpClient();
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskChatGPT([FromBody] ChatRequest request)
    {
        if (commonQuestions.ContainsKey(request.Message))
        {
            return Ok(new { reply = commonQuestions[request.Message] });
        }

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "user", content = request.Message }
            },
            max_tokens = 300 
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", OpenAiApiKey);

        var response = await _httpClient.PostAsync(OpenAiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        dynamic responseObject = JsonConvert.DeserializeObject(responseString);

        string reply = responseObject?.choices?[0]?.message?.content ?? "Sorry, I couldn't understand your question. Please try again.";

        return Ok(new { reply });
    }
}
