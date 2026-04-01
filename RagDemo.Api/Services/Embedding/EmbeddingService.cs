using OpenAI.Embeddings;

namespace RagDemo.Api.Services;

public class EmbeddingService
{
    private readonly EmbeddingClient m_client;
    private readonly ILogger<EmbeddingService> m_logger;

    public EmbeddingService(IConfiguration config, ILogger<EmbeddingService> logger)
    {
        var apiKey = config["OpenAI:ApiKey"]!;

        m_client = new EmbeddingClient("text-embedding-3-small", apiKey);
        m_logger = logger;
    }

    public async Task<float[]> ToEmbeddingAsync(string text)
    {
        m_logger.LogDebug("Requesting embedding for {CharCount} character(s)", text.Length);

        // Converts to 1536-dimensional space vector
        // In this way, we can compare text against similar vectors
        var result = await m_client.GenerateEmbeddingAsync(text);

        return result.Value.ToFloats().ToArray();
    }
}