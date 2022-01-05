using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record struct TodoDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("completedAt")]
        public string? CompletedAt { get; set; }
    }
}