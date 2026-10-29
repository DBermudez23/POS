using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Product
{
    public sealed class ProductUpdateDto
    {
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres.")]
        public string? Name { get; set; }

        [StringLength(100, ErrorMessage = "La descripción debe tener máximo 100 caracteres.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La categoría debe ser válida.")]
        public int? CategoryId { get; set; }

        [StringLength(500, ErrorMessage = "La imagen debe tener máximo 500 caracteres.")]
        public string? Image { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "999.99",
            ErrorMessage = "El porcentaje de IVA no puede ser negativo.",
            ParseLimitsInInvariantCulture = true)]
        public decimal? VatPercentage { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "999.99",
            ErrorMessage = "El porcentaje de margen de ganancia no puede ser negativo.",
            ParseLimitsInInvariantCulture = true)]
        public decimal? ProfitMarginPercentage { get; set; }

        [StringLength(100, ErrorMessage = "El identificador externo debe tener máximo 100 caracteres.")]
        public string? ExternalId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El negocio debe ser válido.")]
        public int? BusinessId { get; set; }

        [StringLength(1, ErrorMessage = "El estado debe tener máximo 1 carácter.")]
        [RegularExpression("^[AI]$", ErrorMessage = "El estado debe ser A o I.")]
        public string? Status { get; set; }

        public bool ClearImage { get; set; }

        public bool ClearExternalId { get; set; }

        public bool ClearBusiness { get; set; }
    }
}
