using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Services.WebApi.Contracts.Users
{
    public class UserCreateRequestDto
    {
        public int? BusinessId { get; set; }

        [Range(1, int.MaxValue)]
        public int DocumentTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(80)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(72, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1)]
        public string Status { get; set; } = "A";
    }
}
