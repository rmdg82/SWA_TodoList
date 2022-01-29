using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Dtos;

namespace Client.Components
{
    public partial class UpdateTodoDialog
    {
        private const int _maxLengthNewTodo = 20;
        private Func<string, string?> ValidationFunc { get; set; } = CheckMaxLength;

        [CascadingParameter]
        public MudDialogInstance? MudDialogInstance { get; set; }

        [Parameter]
        public TodoDto TodoDto { get; set; } = new TodoDto();

        public void Submit()
        {
            MudDialogInstance!.Close(DialogResult.Ok(TodoDto));
        }

        public void Cancel()
        {
            MudDialogInstance!.Cancel();
        }

        private static string? CheckMaxLength(string ch)
        {
            if (!string.IsNullOrWhiteSpace(ch) && ch.Length > _maxLengthNewTodo)
            {
                return $"Max {_maxLengthNewTodo} characters";
            }

            return null;
        }
    }
}