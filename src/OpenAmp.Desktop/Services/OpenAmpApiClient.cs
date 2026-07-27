using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using OpenAmp.Desktop.Models;

namespace OpenAmp.Desktop.Services;

public sealed class OpenAmpApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private string? _refreshToken;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public OpenAmpApiClient(string baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public Uri BaseAddress => _http.BaseAddress!;

    public async Task<AuthSession> LoginAsync(string identifier, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email = identifier, password });
        return await ReadAsync<AuthSession>(response);
    }

    public void SetSession(AuthSession session)
    {
        _refreshToken = session.RefreshToken;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
    }

    public Task<DesktopLookups> GetLookupsAsync() => GetAsync<DesktopLookups>("api/desktop/lookups");
    public Task<DashboardData> GetDashboardAsync() => GetAsync<DashboardData>("api/desktop/dashboard");
    public async Task<List<HallItem>> GetHallsAsync()
    {
        var halls = await GetAsync<List<HallItem>>("api/desktop/halls");
        foreach (var hall in halls)
        {
            hall.SlikaUrl = AbsoluteUrl(hall.SlikaUrl);
        }
        return halls;
    }
    public Task<List<EquipmentItem>> GetEquipmentAsync() => GetAsync<List<EquipmentItem>>("api/desktop/equipment");
    public Task<List<ArticleItem>> GetArticlesAsync() => GetAsync<List<ArticleItem>>("api/desktop/articles?lowStockOnly=false");
    public async Task<List<BandItem>> GetBandsAsync()
    {
        var bands = await GetAsync<List<BandItem>>("api/desktop/bands");
        foreach (var band in bands)
        {
            band.SlikaUrl = AbsoluteUrl(band.SlikaUrl);
        }
        return bands;
    }
    public Task<List<UserItem>> GetUsersAsync() => GetAsync<List<UserItem>>("api/desktop/users");

    public Task<BusinessReport> GetReportAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int? hallId,
        int? genreId) =>
        GetAsync<BusinessReport>(ReportPath("api/desktop/reports", fromUtc, toUtc, hallId, genreId));

    public Task<byte[]> DownloadReportPdfAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int? hallId,
        int? genreId) =>
        GetBytesAsync(ReportPath("api/desktop/reports/pdf", fromUtc, toUtc, hallId, genreId));

    public Task<List<ReservationItem>> GetReservationsAsync(DateTime fromUtc, DateTime toUtc) =>
        GetAsync<List<ReservationItem>>(
            $"api/desktop/reservations?fromUtc={Uri.EscapeDataString(fromUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}&toUtc={Uri.EscapeDataString(toUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}");

    public Task<HallItem> SaveHallAsync(int? id, object request) =>
        id.HasValue ? PutAsync<HallItem>($"api/desktop/halls/{id}", request) : PostAsync<HallItem>("api/desktop/halls", request);

    public Task DeactivateHallAsync(int id) => DeleteAsync($"api/desktop/halls/{id}");

    public Task<EquipmentItem> SaveEquipmentAsync(int? id, object request) =>
        id.HasValue ? PutAsync<EquipmentItem>($"api/desktop/equipment/{id}", request) : PostAsync<EquipmentItem>("api/desktop/equipment", request);

    public Task<EquipmentItem> ReportServiceAsync(int id, string description) =>
        PostAsync<EquipmentItem>($"api/desktop/equipment/{id}/services", new { opisKvara = description });

    public Task<EquipmentItem> CompleteServiceAsync(int id, int serviceId, object request) =>
        PutAsync<EquipmentItem>($"api/desktop/equipment/{id}/services/{serviceId}", request);

    public Task<ArticleItem> SaveArticleAsync(int? id, object request) =>
        id.HasValue ? PutAsync<ArticleItem>($"api/desktop/articles/{id}", request) : PostAsync<ArticleItem>("api/desktop/articles", request);

    public Task<ReservationItem> SaveReservationAsync(int? id, object request) =>
        id.HasValue ? PutAsync<ReservationItem>($"api/desktop/reservations/{id}", request) : PostAsync<ReservationItem>("api/desktop/reservations", request);

    public Task<BandItem> SaveBandAsync(int id, object request) =>
        PutAsync<BandItem>($"api/desktop/bands/{id}", request);

    public Task<UserItem> SaveUserAsync(int id, object request) =>
        PutAsync<UserItem>($"api/desktop/users/{id}", request);

    public async Task UploadHallImageAsync(int hallId, string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(stream);
        file.Headers.ContentType = new MediaTypeHeaderValue(ContentType(filePath));
        form.Add(file, "file", Path.GetFileName(filePath));
        form.Add(new StringContent("Fotografija sale"), "alternativeText");
        var response = await _http.PostAsync($"api/images/halls/{hallId}", form);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await RefreshAccessTokenAsync())
        {
            response.Dispose();
            stream.Position = 0;
            using var retryForm = new MultipartFormDataContent();
            var retryFile = new StreamContent(stream);
            retryFile.Headers.ContentType = new MediaTypeHeaderValue(ContentType(filePath));
            retryForm.Add(retryFile, "file", Path.GetFileName(filePath));
            retryForm.Add(new StringContent("Fotografija sale"), "alternativeText");
            response = await _http.PostAsync($"api/images/halls/{hallId}", retryForm);
        }
        await EnsureSuccessAsync(response);
    }

    private async Task<T> GetAsync<T>(string path)
    {
        var response = await _http.GetAsync(path);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await RefreshAccessTokenAsync())
        {
            response.Dispose();
            response = await _http.GetAsync(path);
        }
        return await ReadAsync<T>(response);
    }

    private async Task<byte[]> GetBytesAsync(string path)
    {
        var response = await _http.GetAsync(path);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await RefreshAccessTokenAsync())
        {
            response.Dispose();
            response = await _http.GetAsync(path);
        }
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<T> PostAsync<T>(string path, object request)
    {
        var response = await _http.PostAsJsonAsync(path, request, _json);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await RefreshAccessTokenAsync())
        {
            response.Dispose();
            response = await _http.PostAsJsonAsync(path, request, _json);
        }
        return await ReadAsync<T>(response);
    }

    private async Task<T> PutAsync<T>(string path, object request)
    {
        var response = await _http.PutAsJsonAsync(path, request, _json);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await RefreshAccessTokenAsync())
        {
            response.Dispose();
            response = await _http.PutAsJsonAsync(path, request, _json);
        }
        return await ReadAsync<T>(response);
    }

    private async Task DeleteAsync(string path)
    {
        var response = await _http.DeleteAsync(path);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await RefreshAccessTokenAsync())
        {
            response.Dispose();
            response = await _http.DeleteAsync(path);
        }
        await EnsureSuccessAsync(response);
    }

    private async Task<bool> RefreshAccessTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_refreshToken))
        {
            return false;
        }
        await _refreshLock.WaitAsync();
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/refresh", new { refreshToken = _refreshToken }, _json);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            var session = await response.Content.ReadFromJsonAsync<AuthSession>(_json);
            if (session is null)
            {
                return false;
            }
            SetSession(session);
            return true;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_json)
            ?? throw new InvalidOperationException("API je vratio prazan odgovor.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        var raw = await response.Content.ReadAsStringAsync();
        try
        {
            var problem = JsonSerializer.Deserialize<JsonElement>(raw);
            if (problem.TryGetProperty("detail", out var detail))
            {
                throw new InvalidOperationException(detail.GetString() ?? "API zahtjev nije uspio.");
            }
        }
        catch (JsonException)
        {
        }
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(raw) ? $"API greška {(int)response.StatusCode}." : raw);
    }

    private static string ContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg"
    };

    private static string ReportPath(
        string endpoint,
        DateTime fromUtc,
        DateTime toUtc,
        int? hallId,
        int? genreId)
    {
        var query = $"fromUtc={Uri.EscapeDataString(fromUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}"
            + $"&toUtc={Uri.EscapeDataString(toUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}";
        if (hallId.HasValue)
        {
            query += $"&hallId={hallId.Value}";
        }
        if (genreId.HasValue)
        {
            query += $"&genreId={genreId.Value}";
        }
        return $"{endpoint}?{query}";
    }

    private string? AbsoluteUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            return value;
        }
        return new Uri(BaseAddress, value.TrimStart('/')).ToString();
    }

    public void Dispose()
    {
        _http.Dispose();
        _refreshLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
