using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Business
{
    public class BusinessListFilterDto
    {
        [StringLength(20)]
        public string? Nit { get; set; }

        [StringLength(80)]
        public string? Name { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }
    }
}
