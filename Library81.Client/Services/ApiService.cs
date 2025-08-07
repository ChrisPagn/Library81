// Library81.Client/Services/ApiService.cs
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Library81.Shared.ViewModels;

namespace Library81.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/{endpoint}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return result ?? new ApiResponse<T> { Success = false, Message = "Erreur de désérialisation" };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Erreur HTTP: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"api/{endpoint}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return result ?? new ApiResponse<T> { Success = false, Message = "Erreur de désérialisation" };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Erreur HTTP: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/{endpoint}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return result ?? new ApiResponse<T> { Success = false, Message = "Erreur de désérialisation" };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Erreur HTTP: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/{endpoint}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return result ?? new ApiResponse<T> { Success = false, Message = "Erreur de désérialisation" };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Erreur HTTP: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}