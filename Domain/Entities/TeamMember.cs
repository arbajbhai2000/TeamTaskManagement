using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TeamMember : BaseEntity
    {
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
