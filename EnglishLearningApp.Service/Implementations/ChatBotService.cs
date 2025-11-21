namespace ERSP.Api.Services
{
    using ERSP.Service.DTOs.Chatbot;
    using System.Text;

    public class ChatBotService
    {
        private readonly ChatNlpService _chatNlp;
        private readonly GeminiClient _gemini;

        public ChatBotService(ChatNlpService chatNlp, GeminiClient gemini)
        {
            _chatNlp = chatNlp;
            _gemini = gemini;
        }

        public async Task<string> HandleAsync(string userMessage, string type)
        {
            switch (type)
            {
                case "smalltalk":
                    return await HandleSmallTalkAsync(userMessage);

                case "error":
                    return await HandleErrorAsync(userMessage);

                case "grammar_fix":
                    return await HandleGrammarFixAsync(userMessage);

                case "answer_suggest":
                    return await HandleAnswerSuggestAsync(userMessage);

                case "structure_review":
                    return await HandleStructureReviewAsync(userMessage);

                case "essay":
                    return await HandleEssayAsync(userMessage);

                default:
                    return "Hiện tại hệ thống chưa có dịch vụ đó.";
            }
        }

        // -------------------------------------------------------------
        // 1) Smalltalk
        // -------------------------------------------------------------
        private async Task<string> HandleSmallTalkAsync(string userMessage)
        {
            var prompt = $@"
Bạn là chatbot luyện tiếng Anh thân thiện.
Trả lời ngắn gọn bằng tiếng Việt + có thể thêm ví dụ tiếng Anh đơn giản.

Câu người dùng:
""{userMessage}""

Giới hạn:
- Không trả lời chính trị, tôn giáo, phạm pháp.
- Gợi ý người dùng có thể hỏi về: sửa ngữ pháp, luyện câu, viết văn, gợi ý đáp án.

Hãy trả lời:";
            return await _gemini.GenerateAsync(prompt);
        }

        // -------------------------------------------------------------
        // 2) Error – user hỏi lung tung
        // -------------------------------------------------------------
        private async Task<string> HandleErrorAsync(string userMessage)
        {
            var prompt = $@"
Người dùng gửi câu không rõ mục đích.

Câu:
""{userMessage}""

Hãy trả lời thân thiện, gợi ý họ:
- sửa ngữ pháp
- luyện câu
- nhờ giải thích cấu trúc câu
- yêu cầu viết bài văn mẫu
- nhờ gợi ý đáp án tiếng Anh.";

            return await _gemini.GenerateAsync(prompt);
        }

        // -------------------------------------------------------------
        // 3) Sửa lỗi ngữ pháp
        // -------------------------------------------------------------
        private async Task<string> HandleGrammarFixAsync(string userMessage)
        {
            var prompt = $@"
Bạn là công cụ sửa lỗi ngữ pháp tiếng Anh.

Nhiệm vụ:
- Sửa câu tiếng Anh cho đúng.
- Giải thích ngắn gọn lý do (bằng tiếng Việt).
- Giữ nguyên ý nghĩa ban đầu.
- Không được thay đổi nội dung quá nhiều.

Câu người dùng:
""{userMessage}""

Hãy trả lời theo format:

✔ **Câu đã sửa:**
...

📝 **Giải thích:**
...";

            return await _gemini.GenerateAsync(prompt);
        }

        // -------------------------------------------------------------
        // 4) Gợi ý đáp án
        // -------------------------------------------------------------
        private async Task<string> HandleAnswerSuggestAsync(string userMessage)
        {
            var prompt = $@"
Bạn là trợ lý tiếng Anh chuyên gợi ý đáp án.

Người dùng gửi câu hỏi:
""{userMessage}""

Nhiệm vụ:
- Đưa ra 1–3 đáp án gợi ý.
- Giải thích ngắn gọn lý do chọn đáp án.
- Nếu câu hỏi mơ hồ → yêu cầu người dùng gửi rõ hơn.

Hãy trả lời theo format:

✔ **Đáp án gợi ý:**
1. ...
2. ...

📝 **Giải thích:**
...";

            return await _gemini.GenerateAsync(prompt);
        }

        // -------------------------------------------------------------
        // 5) Gợi ý cấu trúc câu sai
        // -------------------------------------------------------------
        private async Task<string> HandleStructureReviewAsync(string userMessage)
        {
            var prompt = $@"
Bạn là giáo viên tiếng Anh chuyên phân tích cấu trúc câu.

Câu người dùng:
""{userMessage}""

Nhiệm vụ:
- Kiểm tra xem cấu trúc câu đã đúng chưa.
- Nếu sai → sửa + giải thích.
- Nếu đúng → khen và giải thích thêm cách dùng nâng cao.

Trả lời theo format:

✔ **Phiên bản đúng (nếu có):**
...

📝 **Phân tích cấu trúc:**
...";

            return await _gemini.GenerateAsync(prompt);
        }

        // -------------------------------------------------------------
        // 6) Tạo bài văn theo chủ đề
        // -------------------------------------------------------------
        private async Task<string> HandleEssayAsync(string userMessage)
        {
            var prompt = $@"
Bạn là công cụ viết văn tiếng Anh.

Yêu cầu người dùng:
""{userMessage}""

Nhiệm vụ:
- Viết đoạn văn 120–180 từ bằng tiếng Anh.
- Chủ đề đúng 100% với yêu cầu.
- Văn phong: tự nhiên, dễ hiểu, phù hợp học sinh – sinh viên.
- Sau đoạn văn, giải thích 5 từ vựng hay (bằng tiếng Việt).

Format:

📘 **Essay:**
...

📚 **Từ vựng hay:**
- word (nghĩa): giải thích
";

            return await _gemini.GenerateAsync(prompt);
        }
    }
}
