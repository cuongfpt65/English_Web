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
            _apiKey = config["OpenAI:ApiKey"]
                      ?? throw new Exception("OpenAI:ApiKey is not configured");
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var url = "https://api.openai.com/v1/chat/completions";

            var body = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            using var response = await _httpClient.PostAsJsonAsync(url, body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
                return "Hiện tại chưa có dịch vụ đó";

            var text = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            text = CleanMarkdownJson(text);
            Console.WriteLine("ChatGPT Response: " + text);
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
