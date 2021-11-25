using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models
{
    public record Todo
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsCompleted { get; set; }
    }
}