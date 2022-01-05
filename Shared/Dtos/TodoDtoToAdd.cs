using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record struct TodoDtoToAdd
    {
        [JsonPropertyName("text")]
        public string Text { get; init; }

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; init; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; init; }

        public TodoDtoToAdd(string text)
        {
            Text = text;
            IsCompleted = false;
            CreatedAt = DateTime.Now.ToString("dd/MM/yyyy H:mm");
        }
    }
}