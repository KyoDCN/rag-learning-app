using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using RagDemo.Contracts;

namespace RagDemo.App.Services;

public class RagApiClient
{
    private readonly HttpClient m_http;
    private readonly JsonSerializerOptions m_jsonOptions = new(JsonSerializerDefaults.Web);

    public RagApiClient(string baseUrl)
    {
        m_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        m_http.DefaultRequestHeaders.Add("X-Session-Id", Guid.NewGuid().ToString());
    }

    public Task AddDocumentsAsync(IEnumerable<AddDocumentRequest> documents, CancellationToken ct = default)
        => m_http.PostAsJsonAsync("api/rag/document/add", documents, ct);

    public Task LoadDocumentsAsync(IEnumerable<LoadDocumentRequest> documents, CancellationToken ct = default)
        => m_http.PostAsJsonAsync("api/rag/document/load", documents, ct);

    public Task ClearDocumentsAsync(CancellationToken ct = default)
        => m_http.DeleteAsync("api/rag/document/clear", ct);

    public async Task<VectorStoreStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var result = await m_http.GetFromJsonAsync<VectorStoreStatus>("api/rag/document/status", m_jsonOptions, ct);
        return result!;
    }

    public async Task<QueryResponse> QueryAsync(QueryRequest request, CancellationToken ct = default)
    {
        var response = await m_http.PostAsJsonAsync("api/rag/query", request, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QueryResponse>(m_jsonOptions, ct);
        return result!;
    }

    public async IAsyncEnumerable<QueryStreamResponse> QueryStreamAsync(
        QueryRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/rag/query/stream")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await m_http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var json = line["data: ".Length..];
            var update = JsonSerializer.Deserialize<QueryStreamResponse>(json, m_jsonOptions);

            if (update is not null)
                yield return update;
        }
    }
}
