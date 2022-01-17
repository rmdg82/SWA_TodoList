using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public class TodoDtoToAdd
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        public TodoDtoToAdd(string text)
        {
            Text = text;
        }
    }
}