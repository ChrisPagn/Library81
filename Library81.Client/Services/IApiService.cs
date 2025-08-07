// Library81.Client/Services/IApiService.cs
using Library81.Shared.ViewModels;

namespace Library81.Client.Services;

public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
}
