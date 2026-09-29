using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.DTOs.Users
{
    public class UpdateUserRequest
    {
        public string Name { get; set; } = string.Empty;

        public UserRole Role { get; set; }
    }
}