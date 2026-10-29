using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Business
{
    public class BusinessDto
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string Nit { get; set; } = string.Empty;

        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1)]
        public string Status { get; set; } = string.Empty;
    }
}
