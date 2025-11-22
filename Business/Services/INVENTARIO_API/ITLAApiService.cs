using Domain.Environment;
using Domain.INVENTARIO_API;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Business.Services.INVENTARIO_API
{
    public class INVENTARIOApiService
    {
        private readonly InventarioApiSetting _setting;
        private readonly HttpClient _httpClient;
        private String? _accessToken = null;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private readonly JsonSerializerOptions jsonOpts = new() { PropertyNameCaseInsensitive = true };
        public sealed record FilePayload(byte[] Bytes, string FileName, string ContentType);

        public INVENTARIOApiService(IOptions<InventarioApiSetting> inventarioSetting, HttpClient httpClient)
        {
            _setting = inventarioSetting.Value;
            _httpClient = httpClient;
        }

        public String WrapUrl(String path) => path.StartsWith("/") ? path : ("/" + path);

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return _accessToken;
            }

            await _tokenLock.WaitAsync();

            try
            {
                if (!string.IsNullOrEmpty(_accessToken))
                    return _accessToken!;

                var payload = new
                {
                    correo = _setting.Credential.Email,
                    clave = _setting.Credential.Password
                };

                using var req = new HttpRequestMessage(HttpMethod.Post, WrapUrl("/internal/auth/login"))
                {
                    Content = JsonContent.Create(payload)
                };

                using var resp = await _httpClient.SendAsync(req);
                resp.EnsureSuccessStatusCode();

                using var stream = await resp.Content.ReadAsStreamAsync();
                var tokenResp = await resp.Content.ReadFromJsonAsync<LoginResponse>(jsonOpts);

                if (tokenResp == null)
                {
                    throw new InvalidOperationException("INVENTARIO API. It does not retrieve the token response.");
                }

                _accessToken = tokenResp.Data.AccessToken;

                if (string.IsNullOrWhiteSpace(_accessToken))
                    throw new InvalidOperationException("INVENTARIO API. Access token does not found.");

                return _accessToken!;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
        {
            return await GetAsync<T>(path, null, ct);
        }

        public async Task<T?> GetAsync<T>(string path, IDictionary<string, string>? query = default, CancellationToken ct = default)
        {
            var token = await GetTokenAsync();
            String newPath = WrapUrl(path);

            if (query != null)
            {
                newPath = QueryHelpers.AddQueryString(newPath, query);
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, newPath);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Headers.Accept.ParseAdd("application/json");

            using var resp = await _httpClient.SendAsync(req, ct);

            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }

        public async Task<FilePayload> DownloadFileAsync(
            string path,
            IDictionary<string, string>? query = null,
            CancellationToken ct = default)
        {
            var token = await GetTokenAsync();
            String url = WrapUrl(path);

            if (query != null)
            {
                url = QueryHelpers.AddQueryString(url, query);
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Headers.Accept.ParseAdd("*/*");

            using var resp = await _httpClient.SendAsync(req, ct);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                _accessToken = null;
            }

            resp.EnsureSuccessStatusCode();

            var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
            var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = GetFileName(resp) ?? GuessNameFromUrl(url) ?? "download.bin";

            return new FilePayload(bytes, fileName, contentType);
        }

        private static string? GetFileName(HttpResponseMessage resp)
        {
            var cd = resp.Content.Headers.ContentDisposition;
            if (!string.IsNullOrWhiteSpace(cd?.FileNameStar)) return cd!.FileNameStar!.Trim('\"');
            if (!string.IsNullOrWhiteSpace(cd?.FileName)) return cd!.FileName!.Trim('\"');
            if (resp.Headers.TryGetValues("X-Filename", out var v)) return v.FirstOrDefault();
            return null;
        }

        private static string? GuessNameFromUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri)) return null;
            var path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString();
            var last = path.TrimEnd('/').Split('/').LastOrDefault();
            return string.IsNullOrWhiteSpace(last) ? null : last;
        }
    }
}
