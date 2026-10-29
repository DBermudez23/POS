using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Product
{
    public sealed class ProductListFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El negocio debe ser válido.")]
        public int? BusinessId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La categoría debe ser válida.")]
        public int? CategoryId { get; set; }

        [StringLength(1, ErrorMessage = "El estado debe tener máximo 1 carácter.")]
        [RegularExpression("^[AI]$", ErrorMessage = "El estado debe ser A o I.")]
        public string? Status { get; set; }

        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres.")]
        public string? Name { get; set; }

        [StringLength(100, ErrorMessage = "El identificador externo debe tener máximo 100 caracteres.")]
        public string? ExternalId { get; set; }
    }
}