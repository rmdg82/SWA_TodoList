using System.Text.Json.Serialization;

namespace SharedLibrary.Dtos;

public class TodoDtoToAdd
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    public TodoDtoToAdd(string text)
    {
        Text = text;
    }
}
