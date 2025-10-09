using SIC.Frontend.Repositories;
using SIC.Shared.Response;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SIC.Frontend.Repositories;

public class Repository : IRepository
{
    private readonly HttpClient _httpClient;

    private JsonSerializerOptions _jsonDefaultOptions => new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public Repository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseWrapper<object>> GetAsync(string url)
    {
        var responseHTTP = await _httpClient.GetAsync(url);
        return new HttpResponseWrapper<object>(null, !responseHTTP.IsSuccessStatusCode, responseHTTP);
    }

    public async Task<HttpResponseWrapper<T>> GetAsync<T>(string url)
    {
        var responseHttp = await _httpClient.GetAsync(url);

        if (responseHttp.IsSuccessStatusCode)
        {
            if (responseHttp.StatusCode == HttpStatusCode.NoContent)
            {
                return new HttpResponseWrapper<T>(default, false, responseHttp);
            }

            var content = await responseHttp.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return new HttpResponseWrapper<T>(default, false, responseHttp);
            }

            // Solo deserializamos si hay contenido
            var response = await UnserializeAnswerAsync<T>(responseHttp);
            return new HttpResponseWrapper<T>(response, false, responseHttp);
        }

        return new HttpResponseWrapper<T>(default, true, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> PostAsync<T>(string url, T model)
    {
        var messageJson = JsonSerializer.Serialize(model);
        var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PostAsync(url, messageContent);
        return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp); ;
    }

    public async Task<HttpResponseWrapper<TActionResponse>> PostAsync<T, TActionResponse>(string url, T model)
    {
        var messageJson = JsonSerializer.Serialize(model);
        var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PostAsync(url, messageContent);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswerAsync<TActionResponse>(responseHttp);
            return new HttpResponseWrapper<TActionResponse>(response, false, responseHttp);
        }
        return new HttpResponseWrapper<TActionResponse>(default, true, responseHttp);
    }

    public async Task<HttpResponseWrapper<object>> DeleteAsync<T>(string url)
    {
        var responseHttp = await _httpClient.DeleteAsync(url);
        return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp); ;
    }

    public async Task<HttpResponseWrapper<object>> PutAsync<T>(string url, T model)
    {
        var messageJson = JsonSerializer.Serialize(model);
        var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PutAsync(url, messageContent);
        return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp);
    }

    public async Task<HttpResponseWrapper<TActionResponse>> PutAsync<T, TActionResponse>(string url, T model)
    {
        var messageJson = JsonSerializer.Serialize(model);
        var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var responseHttp = await _httpClient.PutAsync(url, messageContent);
        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswerAsync<TActionResponse>(responseHttp);
            return new HttpResponseWrapper<TActionResponse>(response, false, responseHttp);
        }
        return new HttpResponseWrapper<TActionResponse>(default, true, responseHttp);
    }

    private async Task<T> UnserializeAnswerAsync<T>(HttpResponseMessage responseHttp)
    {
        var response = await responseHttp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(response, _jsonDefaultOptions)!;
    }

    public async Task<byte[]> GetFileAsync(string url)
    {
        var responseHttp = await _httpClient.GetAsync(url);
        if (responseHttp.IsSuccessStatusCode)
        {
            return await responseHttp.Content.ReadAsByteArrayAsync();
        }
        return Array.Empty<byte>();
    }

    public async Task<HttpResponseWrapper<TActionResponse>> UploadFileAsync<T, TActionResponse>(
     string url, Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        content.Add(fileContent, "file", fileName);

        var responseHttp = await _httpClient.PostAsync(url, content);

        if (responseHttp.IsSuccessStatusCode)
        {
            var response = await UnserializeAnswerAsync<TActionResponse>(responseHttp);
            return new HttpResponseWrapper<TActionResponse>(response, false, responseHttp);
        }

        return new HttpResponseWrapper<TActionResponse>(default!, true, responseHttp);
    }
}