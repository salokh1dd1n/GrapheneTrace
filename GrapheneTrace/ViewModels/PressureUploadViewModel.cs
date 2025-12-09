using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GrapheneTrace.ViewModels
{
    public class PressureUploadViewModel
    {
        [Required]
        [Display(Name = "Day of recording")]
        [DataType(DataType.Date)]
        public DateTime Day { get; set; }

        [Required]
        [Display(Name = "CSV file")]
        public IFormFile File { get; set; } = default!;
    }
}