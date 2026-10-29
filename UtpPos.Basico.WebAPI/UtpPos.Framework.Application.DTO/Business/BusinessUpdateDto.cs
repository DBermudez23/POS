using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Business
{
    public class BusinessUpdateDto
    {
        [StringLength(20)]
        public string? Nit { get; set; }

        [StringLength(80)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }

        [DefaultValue(false)]
        public bool ClearPhone { get; set; }
    }
}
