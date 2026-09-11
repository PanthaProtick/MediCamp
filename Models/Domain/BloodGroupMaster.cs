using System.ComponentModel.DataAnnotations;

namespace MediCamp.Models.Domain
{
    public class BloodGroupMaster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string GroupName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
