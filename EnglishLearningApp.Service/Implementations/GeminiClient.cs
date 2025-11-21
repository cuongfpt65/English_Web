namespace ERSP.Api.Services
{
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;

    public class GeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"]
                      ?? throw new Exception("Gemini:ApiKey is not configured");
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
            };

            using var response = await _httpClient.PostAsJsonAsync(url, body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
                return "Hiện tại chưa có dịch vụ đó";

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            text = CleanMarkdownJson(text);
            Console.WriteLine("Gemini Response: " + text);
            return string.IsNullOrWhiteSpace(text) ? "Hiện tại chưa có dịch vụ đó" : text;
        }
        private static string CleanMarkdownJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Trim();

            // Remove Markdown ```...``` blocks
            if (text.StartsWith("```"))
            {
                int first = text.IndexOf("```") + 3;
                int last = text.LastIndexOf("```");

                if (last > first)
                {
                    text = text.Substring(first, last - first);
                }

                text = text.Trim();
            }

            // 🔥 Remove a standalone 'json' line
            if (text.StartsWith("json\n") || text.StartsWith("json\r\n"))
            {
                text = text.Substring(text.IndexOf('{')).Trim();
            }
            else if (text.StartsWith("json"))
            {
                // Case: "json   { ... }"
                int braceIndex = text.IndexOf('{');
                if (braceIndex > 0)
                {
                    text = text.Substring(braceIndex).Trim();
                }
            }

            return text;
        }


    }
}
