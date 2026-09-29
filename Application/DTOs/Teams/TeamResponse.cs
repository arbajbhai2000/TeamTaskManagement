using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Teams
{
    public class TeamResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CreatedById { get; set; }

        public string CreatedByName { get; set; } = string.Empty;

        public int MemberCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}