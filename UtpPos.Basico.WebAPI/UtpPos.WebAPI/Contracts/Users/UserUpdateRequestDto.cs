using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Services.WebApi.Contracts.Users
{
    public class UserUpdateRequestDto
    {
        public int? BusinessId { get; set; }
        public int? DocumentTypeId { get; set; }

        [StringLength(20)]
        public string? DocumentNumber { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(80)]
        public string? Email { get; set; }

        [StringLength(72, MinimumLength = 6)]
        public string? Password { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }

        public bool ClearBusinessId { get; set; }
        public bool ClearPhone { get; set; }
    }
}
