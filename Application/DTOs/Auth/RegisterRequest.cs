using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Please enter a valid email address."
        )]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}