namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleCancelResult
    {
        public int SaleId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
