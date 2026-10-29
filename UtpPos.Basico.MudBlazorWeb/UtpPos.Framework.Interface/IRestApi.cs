using UtpPos.Framework.Shared;

namespace UtpPos.Framework.Interface
{
    public interface IRestApi
    {
        Task<Response<TResponse>> GetAsync<TResponse>(string endpoint, bool authorized = false);
        Task<Response<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, bool authorized = false);
        Task<Response<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, bool authorized = false);
        Task<Response<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request, bool authorized = false);
        Task<Response<TResponse>> DeleteAsync<TResponse>(string endpoint, bool authorized = false);
    }
}
