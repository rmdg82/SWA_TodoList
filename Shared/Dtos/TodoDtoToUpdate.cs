using System.Text.Json.Serialization;

namespace SharedLibrary.Dtos;

public class TodoDtoToUpdate
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    public TodoDtoToUpdate(string text)
    {
        Text = text;
    }
}
