using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Users
{
    public class UpdateProfileRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}