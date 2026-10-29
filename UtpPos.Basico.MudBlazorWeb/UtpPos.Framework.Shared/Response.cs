namespace UtpPos.Framework.Shared
{
    public class Response<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public bool IsUnauthorized => StatusCode == 401;
        public bool IsForbidden => StatusCode == 403;
    }
}
