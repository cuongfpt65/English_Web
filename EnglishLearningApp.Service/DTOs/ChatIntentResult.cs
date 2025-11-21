using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERSP.Service.DTOs.Chatbot
{
    public class ChatIntentResult
    {
        public string Type { get; set; } = "unknown";
        // discount | trip | smalltalk | unknown

        // Cho trip
        public string? StartPoint { get; set; }
        public string? EndPoint { get; set; }
        public string? Status { get; set; } // pending, completed, ... nếu bạn dùng

        // Cho discount
        public bool? AskForListActive { get; set; } // có phải hỏi danh sách mã giảm giá đang áp dụng

        public string? RawJson { get; set; }  // để debug nếu cần
    }
}
