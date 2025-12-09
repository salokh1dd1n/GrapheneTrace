using System;
using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.ViewModels
{
    public class CreateUserViewModel
    {
        [Required] [EmailAddress] public string Email { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        public string? Role { get; set; }

        [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
    }
}