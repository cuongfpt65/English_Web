namespace ERSP.Api.Services
{
    using ERSP.Service.DTOs.Chatbot;
    using System.Text.Json;

    public class ChatNlpService
    {
        private readonly GeminiClient _gemini;

        public ChatNlpService(GeminiClient gemini)
        {
            _gemini = gemini;
        }

        public async Task<ChatIntentResult> ExtractIntentAsync(string userMessage)
        {
            var prompt = $@"
Bạn là bộ phân tích ý định cho chatbot nội bộ.

Nhiệm vụ:
- Đọc câu hỏi tiếng Việt của người dùng về:
  - mã giảm giá (discount code)
  - chuyến đi (trip)
- Trả về JSON với format NHƯ SAU (không thêm text khác):

{{
  ""Type"": ""discount|trip|smalltalk|error|unknown"",
  ""StartPoint"": ""string or null"",
  ""EndPoint"": ""string or null"",
  ""Status"": ""string or null"",
  ""AskForListActive"": true/false/null
}}

Quy tắc:
- Nếu người dùng hỏi về mã giảm giá: Type = ""discount"".
  Ví dụ: ""Có mã giảm giá nào không?"", ""Danh sách mã khuyến mãi hiện tại""
- Nếu người dùng hỏi về các chuyến đi (đi từ A đến B, tìm chuyến, chuyến đã hoàn thành, ...): Type = ""trip"".
- Nếu chỉ chào hỏi, hỏi bạn là ai, hoặc nói chuyện không liên quan: Type = ""smalltalk"".
- Nếu không hiểu: Type = ""unknown"".
- Nếu người dùng hỏi các câu hỏi về quy phạm pháp luật , tôn giáo, chính trị, an ninh mạng, hoặc các chủ đề nhạy cảm khác, hãy trả về Type = ""error"".

Cho trip:
- Cố gắng suy ra StartPoint, EndPoint từ câu hỏi (nếu có).
  Ví dụ: ""Từ Quận 1 đi Thủ Đức"".

Cho discount:
- Nếu người dùng muốn xem danh sách mã đang hoạt động hoặc áp dụng được,
  ""AskForListActive"": true.
- Nếu không rõ: để null.

Chỉ trả về JSON hợp lệ.

Câu hỏi người dùng: ""{userMessage}""
";

            var text = await _gemini.GenerateAsync(prompt);
            text = text.Trim();

            if (text.StartsWith("```"))
            {
                int first = text.IndexOf("```") + 3;
                int last = text.LastIndexOf("```");
                text = text.Substring(first, last - first).Trim();
            }

            // luôn lưu raw JSON
            var result = new ChatIntentResult { RawJson = text };

            try
            {
                var json = JsonSerializer.Deserialize<ChatIntentResult>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (json != null)
                {
                    json.RawJson = text;
                    Console.WriteLine("✅ Parse intent JSON OK: " + json.Type);
                    return json;
                }

                // json == null → fail
                Console.WriteLine("❌ JSON is null after parse");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception while parsing JSON: " + ex.Message);
            }

            // fallback
            return result;
        }
    }
}
