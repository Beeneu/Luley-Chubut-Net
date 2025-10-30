using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Luley_Integracion_Net.Models;

namespace Luley_Integracion_Net.Services;

public class HttpService(HttpClient httpClient, string baseUrl)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _baseUrl = baseUrl.TrimEnd('/');
    private string? _token = null;
    private readonly object _tokenLock = new();

    public async Task EnsureAuthenticatedAsync()
    {
        if (_token != null)
            return;

        lock (_tokenLock)
        {
            if (_token != null)
                return;
        }

        await LoginAsync();
    }

    private async Task LoginAsync()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[AUTH] Logging in...");
        Console.ResetColor();

        string url = $"{_baseUrl}/user/login";
        var payload = new
        {
            email = Environment.GetEnvironmentVariable("API_EMAIL"),
            password = Environment.GetEnvironmentVariable("API_PASSWORD"),
        };

        string json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(url, content);
        // response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();

        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

        if (loginResponse?.Response?.Token == null)
            throw new Exception("Failed to retrieve token from login response");

        lock (_tokenLock)
        {
            _token = loginResponse.Response.Token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _token
            );
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[AUTH] Login successful");
        Console.ResetColor();
    }

    private async Task<T?> ExecuteWithRetryAsync<T>(
        Func<Task<HttpResponseMessage>> request,
        string method,
        string url
    )
    {
        await EnsureAuthenticatedAsync();

        HttpResponseMessage response = await request();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[AUTH] Token expired, re-authenticating...");
            Console.ResetColor();

            lock (_tokenLock)
            {
                _token = null;
            }

            await LoginAsync();
            response = await request();
        }

        // response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        LogResponse(method, url, response.StatusCode, responseContent);

        return string.IsNullOrEmpty(responseContent)
            ? default
            : JsonSerializer.Deserialize<T>(responseContent);
    }

    private async Task ExecuteWithRetryAsync(
        Func<Task<HttpResponseMessage>> request,
        string method,
        string url
    )
    {
        await EnsureAuthenticatedAsync();

        HttpResponseMessage response = await request();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[AUTH] Token expired, re-authenticating...");
            Console.ResetColor();

            lock (_tokenLock)
            {
                _token = null;
            }

            await LoginAsync();
            response = await request();
        }

        // response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        LogResponse(method, url, response.StatusCode, responseContent);
    }

    private void LogResponse(string method, string url, HttpStatusCode statusCode, string content)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"[{method}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{url} ");

        Console.ForegroundColor =
            statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created
                ? ConsoleColor.Green
                : ConsoleColor.Red;
        Console.WriteLine($"({(int)statusCode} {statusCode})");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Response:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(content);
        Console.ResetColor();
        Console.WriteLine();
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        return await ExecuteWithRetryAsync<T>(() => _httpClient.GetAsync(url), "GET", url);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? payload = null)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        StringContent? content = null;

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await ExecuteWithRetryAsync<T>(
            () => _httpClient.PostAsync(url, content),
            "POST",
            url
        );
    }

    public async Task PostAsync(string endpoint, object? payload = null)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        StringContent? content = null;

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        await ExecuteWithRetryAsync(() => _httpClient.PostAsync(url, content), "POST", url);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? payload = null)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        StringContent? content = null;

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await ExecuteWithRetryAsync<T>(() => _httpClient.PutAsync(url, content), "PUT", url);
    }

    public async Task PutAsync(string endpoint, object? payload = null)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        StringContent? content = null;

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        await ExecuteWithRetryAsync(() => _httpClient.PutAsync(url, content), "PUT", url);
    }

    public async Task<T?> PatchAsync<T>(string endpoint, object? payload = null)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        StringContent? content = null;

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await ExecuteWithRetryAsync<T>(
            () => _httpClient.PatchAsync(url, content),
            "PATCH",
            url
        );
    }

    public async Task PatchAsync(string endpoint, object? payload = null)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        StringContent? content = null;

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        await ExecuteWithRetryAsync(() => _httpClient.PatchAsync(url, content), "PATCH", url);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";

        await ExecuteWithRetryAsync(() => _httpClient.DeleteAsync(url), "DELETE", url);

        return true;
    }
}
