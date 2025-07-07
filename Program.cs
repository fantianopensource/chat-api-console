using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string geminiApiKey = config["GeminiApiKey"]!;

// System prompt
string systemPrompt = @"You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
You introduce yourself when first saying hello.
When helping people out, you always ask them for this information to inform the hiking recommendation you provide:
1. The location where they would like to hike
2. What hiking intensity they are looking for
You will then provide three suggestions for nearby hikes that vary in length after you get that information. You will also share an interesting fact about the local nature on the hikes when making a recommendation. At the end of your response, ask if there is anything else you can help with.";

var httpClient = new HttpClient();

while (true)
{
    Console.WriteLine("Your prompt:");
    string? userPrompt = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userPrompt)) continue;

    // Construct Gemini API request body
    var requestBody = new
    {
        contents = new[]
        {
            new { role = "user", parts = new[] { new { text = systemPrompt + "\n" + userPrompt } } }
        }
    };

    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
    var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiApiKey}", content);
    var responseString = await response.Content.ReadAsStringAsync();

    // Parse Gemini response
    try
    {
        using var doc = JsonDocument.Parse(responseString);
        var aiText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        Console.WriteLine("AI Response:");
        Console.WriteLine(aiText);
    }
    catch
    {
        Console.WriteLine("AI Response (raw):");
        Console.WriteLine(responseString);
    }
}
