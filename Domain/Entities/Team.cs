using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Team : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;

        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}