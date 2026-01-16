using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TR.Connector.DTOs;
using TR.Connector.Http.Exceptions;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector.Http;

internal class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ApiClient(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(_httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
    }

    /// <summary>
    /// Настраивкает базовый URL и токен авторизации
    /// </summary>
    public void Configure(string baseUrl, string bearerToken)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be empty", nameof(baseUrl));

        _httpClient.BaseAddress = new Uri(baseUrl);

        if (!string.IsNullOrWhiteSpace(bearerToken))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                bearerToken
            );
    }

    /// <summary>
    /// GET запрос с десериализацией ответа
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint)
        where T : ApiResponse
    {
        try
        {
            _logger?.Debug($"GET: {endpoint}");

            var response = await _httpClient.GetAsync(endpoint);
            return await ProcessResponseAsync<T>(response, endpoint);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.Error($"GET {endpoint} failed: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// POST запрос
    /// </summary>
    public async Task<T> PostAsync<T>(string endpoint, object body)
        where T : ApiResponse
    {
        try
        {
            _logger?.Debug($"POST: {endpoint}");

            var json = JsonSerializer.Serialize(body, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            return await ProcessResponseAsync<T>(response, endpoint);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.Debug($"POST {endpoint} failed: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// PUT запрос (с телом или без)
    /// </summary>
    public async Task PutAsync(string endpoint, object body = null)
    {
        try
        {
            _logger?.Debug($"PUT {endpoint}");

            HttpContent? content = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, jsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            _logger?.Debug($"PUT {endpoint} failed: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// DELETE запрос
    /// </summary>
    public async Task DeleteAsync(string endpoint)
    {
        try
        {
            _logger?.Debug($"DELETE {endpoint}");

            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            _logger?.Debug($"DELETE {endpoint} failed: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Обработка HTTP ответа: проверка статуса и и success поля
    /// </summary>
    public async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, string endpoint)
        where T : ApiResponse
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"HTTP {response.StatusCode} for {endpoint}. Responce: {errorContent}"
            );
        }

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<T>(json, jsonOptions);

        if (result == null)
            throw new InvalidOperationException($"Failed to deserialize response for {endpoint}");

        if (result.Success == false)
        {
            var errorMessage = result.ErrorText ?? "Unknown API error";
            _logger?.Error($"API error on {endpoint}: {errorMessage}");
            throw new ApiException(errorMessage);
        }

        return result;
    }
}
