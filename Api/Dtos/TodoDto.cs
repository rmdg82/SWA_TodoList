using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Api.Dtos
{
    public record TodoDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; init; }

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; init; }
    }
}