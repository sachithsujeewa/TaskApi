using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskApi.Models
{
    public class TodoTask
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public bool IsDone { get; set; }
        public bool IsRemoved { get; set; }
    }
}
