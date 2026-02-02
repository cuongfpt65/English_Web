namespace ERSP.Api.Services
{
    using ERSP.Service.DTOs.Chatbot;
    using EnglishLearningApp.Repository.Interfaces;
    using System.Text;

    public class ChatBotService
    {
        private readonly ChatNlpService _chatNlp;
        private readonly GeminiClient _gemini;
        private readonly IChatRepository _chatRepository;

        public ChatBotService(ChatNlpService chatNlp, GeminiClient gemini, IChatRepository chatRepository)
        {
            _chatNlp = chatNlp;
            _gemini = gemini;
            _chatRepository = chatRepository;
        }        public async Task<string> HandleAsync(string userMessage, string type, Guid? sessionId = null, Guid? userId = null)
        {
            // Lưu tin nhắn của user
            if (sessionId.HasValue && userId.HasValue)
            {
                await _chatRepository.AddMessageAsync(sessionId.Value, "User", userMessage);
            }

            string botResponse;
            switch (type)
            {
                case "smalltalk":
                    botResponse = await HandleSmallTalkAsync(userMessage);
                    break;

                case "error":
                    botResponse = await HandleErrorAsync(userMessage);
                    break;

                case "grammar_fix":
                    botResponse = await HandleGrammarFixAsync(userMessage);
                    break;

                case "answer_suggest":
                    botResponse = await HandleAnswerSuggestAsync(userMessage);
                    break;

                case "structure_review":
                    botResponse = await HandleStructureReviewAsync(userMessage);
                    break;                case "essay":
                    botResponse = await HandleEssayAsync(userMessage);
                    break;

                case "essay_with_vocabulary":
                    botResponse = await HandleEssayWithVocabularyAsync(userMessage);
                    break;

                case "ai_quiz":
                    botResponse = await HandleAIQuizAsync(userMessage);
                    break;

                default:
                    botResponse = "Hiện tại hệ thống chưa có dịch vụ đó.";
                    break;
            }            // Lưu tin nhắn của bot vào DB
            // Đối với essay_with_vocabulary, format lại thành text có cấu trúc: Essay + New Words
            if (sessionId.HasValue && userId.HasValue)
            {
                string messageToSave = botResponse;
                
                // Nếu là essay_with_vocabulary, parse và format lại
                if (type == "essay_with_vocabulary")
                {
                    try
                    {
                        var jsonDoc = System.Text.Json.JsonDocument.Parse(botResponse);
                        var essay = jsonDoc.RootElement.GetProperty("essay").GetString();
                        var vocabularyData = jsonDoc.RootElement.GetProperty("vocabularyData");
                        var vocabulary = vocabularyData.GetProperty("vocabulary");

                        var sb = new StringBuilder();
                        sb.AppendLine("📝 Essay:");
                        sb.AppendLine(essay);
                        sb.AppendLine();
                        sb.AppendLine("📚 New Words:");
                        
                        foreach (var vocab in vocabulary.EnumerateArray())
                        {
                            var word = vocab.GetProperty("word").GetString();
                            var meaning = vocab.GetProperty("meaning").GetString();
                            sb.AppendLine($"- {word}: {meaning}");
                        }

                        messageToSave = sb.ToString();
                    }
                    catch
                    {
                        // Nếu parse lỗi, lưu nguyên
                        messageToSave = botResponse;
                    }
                }
                
                await _chatRepository.AddMessageAsync(sessionId.Value, "Bot", messageToSave);
            }

            return botResponse;
        }

        // -------------------------------------------------------------
        // 1) Smalltalk
        // -------------------------------------------------------------
        private async Task<string> HandleSmallTalkAsync(string userMessage)
        {
            var prompt = $@"
Bạn là một gia sư tiếng Anh thân thiện, nói chuyện tự nhiên như người thật.
Mục tiêu là trò chuyện thoải mái, nhưng luôn khéo léo dẫn người dùng sang luyện tiếng Anh.

Cách trả lời:
- Ưu tiên tiếng Việt, có thể xen một câu tiếng Anh đơn giản nếu phù hợp
- Giữ giọng gần gũi, không quá học thuật
- Nếu người dùng nói chuyện bình thường, hãy phản hồi như trò chuyện và gợi ý nhẹ:
  + sửa câu tiếng Anh
  + thử nói lại bằng tiếng Anh
  + học một từ/cụm mới liên quan

Không làm:
- Không nhắc đến chính trị, tôn giáo, hay nội dung phạm pháp
- Không nói mình là AI hay hệ thống

Tin nhắn người dùng:
""{userMessage}""

Hãy trả lời như một gia sư đang chat với học viên:";
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
        }        // -------------------------------------------------------------
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
        }        // -------------------------------------------------------------
        // 7) Tạo bài văn với từ vựng có cấu trúc (trả về JSON)
        // -------------------------------------------------------------
        private async Task<string> HandleEssayWithVocabularyAsync(string userMessage)
        {
            // Bước 1: Tạo essay
            var essayPrompt = $@"
Bạn là công cụ viết văn tiếng Anh chuyên nghiệp.

Yêu cầu người dùng:
""{userMessage}""

Nhiệm vụ:
- Viết đoạn văn 150–250 từ bằng tiếng Anh.
- Chủ đề đúng 100% với yêu cầu.
- Văn phong: tự nhiên, học thuật, phù hợp học sinh – sinh viên.
- Sử dụng từ vựng academic và intermediate level.

Format trả về:
📘 **Essay:**
[Viết essay ở đây]

CHỈ TRẢ VỀ ESSAY THEO FORMAT TRÊN, KHÔNG CÓ GIẢI THÍCH HAY GHI CHÚ GÌ THÊM.
";

            var essay = await _gemini.GenerateAsync(essayPrompt);
            
            // Bước 2: Trích xuất từ vựng từ essay
            var vocabularyPrompt = $@"
Dưới đây là đoạn essay tiếng Anh:

{essay}

Nhiệm vụ:
- Trích xuất 8-12 từ vựng HAY NHẤT, HỮU ÍCH NHẤT từ đoạn văn trên.
- Ưu tiên từ vựng academic, intermediate, advanced level.
- KHÔNG lấy từ quá đơn giản như: is, am, are, the, a, an, in, on, at...
- Mỗi từ vựng cần có: từ, nghĩa tiếng Việt, và câu ví dụ từ essay.

Format trả về PHẢI là JSON hợp lệ như sau (KHÔNG có markdown, KHÔNG có ```json):
{{
  ""vocabulary"": [
    {{
      ""word"": ""từ vựng"",
      ""meaning"": ""nghĩa tiếng Việt"",
      ""example"": ""câu ví dụ từ essay có chứa từ này""
    }}
  ]
}}

CHỈ TRẢ VỀ JSON, KHÔNG CÓ TEXT NÀO KHÁC.
";

            var vocabularyJson = await _gemini.GenerateAsync(vocabularyPrompt);
            
            // Bước 3: Kết hợp essay và vocabulary thành JSON response
            var cleanVocabularyJson = vocabularyJson.Trim()
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result = $@"{{
  ""essay"": {System.Text.Json.JsonSerializer.Serialize(essay)},
  ""vocabularyData"": {cleanVocabularyJson}
}}";

            return result;
        }

        // -------------------------------------------------------------
        // 8) Tạo quiz AI từ vocabulary (trả về JSON)
        // -------------------------------------------------------------
        private async Task<string> HandleAIQuizAsync(string userMessage)
        {
            var prompt = $@"
Bạn là công cụ tạo câu hỏi trắc nghiệm tiếng Anh từ danh sách từ vựng.

Danh sách từ vựng (JSON):
{userMessage}

Nhiệm vụ:
- Tạo đúng 10 câu hỏi trắc nghiệm từ danh sách từ vựng trên.
- Mỗi câu hỏi dựa trên 1 hoặc kết hợp 2-3 từ để tạo ngữ cảnh TỰ NHIÊN.
- Câu hỏi phải có NGỮ CẢNH thực tế, không hỏi trực tiếp ""nghĩa của từ X là gì"".
- Mỗi câu có 4 đáp án (A, B, C, D), chỉ 1 đáp án đúng.
- Các đáp án sai phải hợp lý, không quá vô lý.

Ví dụ câu hỏi HAY:
- ""When you _____ something, you make it better or enhance it."" (improve)
- ""The company decided to _____ its operations to reduce costs."" (streamline)
- ""She felt _____ after hearing the good news."" (delighted)

Format trả về PHẢI là JSON hợp lệ như sau (KHÔNG có markdown, KHÔNG có ```json):
{{
  ""questions"": [
    {{
      ""question"": ""Câu hỏi ngữ cảnh tự nhiên"",
      ""options"": [
        ""A. Đáp án 1"",
        ""B. Đáp án 2"",
        ""C. Đáp án 3"",
        ""D. Đáp án 4""
      ],
      ""correctAnswer"": ""A"",
      ""explanation"": ""Giải thích ngắn gọn tại sao đáp án này đúng (tiếng Việt)"",
      ""relatedWords"": [""từ 1"", ""từ 2""]
    }}
  ]
}}

CHỈ TRẢ VỀ JSON, KHÔNG CÓ TEXT NÀO KHÁC.
";

            var quizJson = await _gemini.GenerateAsync(prompt);
            
            // Clean up JSON response
            var cleanQuizJson = quizJson.Trim()
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            return cleanQuizJson;
        }
    }
}
