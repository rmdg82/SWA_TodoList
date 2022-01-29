using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedLibrary.Dtos
{
    public class TodoDtoToUpdate
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        public TodoDtoToUpdate(string text)
        {
            Text = text;
        }
    }
}