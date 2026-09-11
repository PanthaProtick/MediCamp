using System.ComponentModel.DataAnnotations;

namespace MediCamp.Models.Domain
{
    public class Hospital
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string District { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Upazila { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
