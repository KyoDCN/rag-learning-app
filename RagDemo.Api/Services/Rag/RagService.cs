using System.ClientModel;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using OpenAI.Chat;
using RagDemo.Api.Stores;
using RagDemo.Contracts;
using RagDemo.Api.Managers;
using RagDemo.Api.Services.Rag;
using RagDemo.Api.HttpContexts;

namespace RagDemo.Api.Services;

public class RagService
{
    private readonly ILogger<RagService> m_logger;
    private readonly EmbeddingService m_embeddingService;
    private readonly UserSessionManager m_userSessionManager;
    private readonly IUserSessionHttpContext m_userSessionHttpContext;
    private readonly ChatClient m_chatClient;

    public RagService(
        ILogger<RagService> logger,
        EmbeddingService embedding, 
        UserSessionManager userSession, 
        IUserSessionHttpContext userSessionHttpContext, 
        IConfiguration config)
    {
        m_logger = logger;
        m_embeddingService = embedding;
        m_userSessionManager = userSession;
        m_userSessionHttpContext = userSessionHttpContext;
        m_chatClient = new("gpt-4o-mini", config["OpenAI:ApiKey"]!);
    }

    public VectorStoreStatus GetVectorStoreStatus()
    {
        SessionId sessionId = m_userSessionHttpContext.SessionId;

        UserSession? userSession = m_userSessionManager.GetSession(sessionId);

        return new VectorStoreStatus(userSession?.VectorStore.Count ?? 0);
    }

    public void ClearVectorStore()
    {
        SessionId sessionId = m_userSessionHttpContext.SessionId;

        m_userSessionManager.RemoveSession(sessionId);

        m_logger.LogInformation("Vector store cleared for session {SessionId}", sessionId);
    }

    public async Task AddDocumentsAsync(List<RagDocumentText> documents)
    {
        SessionId sessionId = m_userSessionHttpContext.SessionId;

        UserSession userSession = m_userSessionManager.GetOrAddSession(sessionId);

        m_logger.LogInformation("Adding {DocumentCount} document(s) for session {SessionId}", documents.Count, sessionId);

        IReadOnlyList<DocumentChunk> new_chunks = await ToDocumentChunk(documents);

        userSession.VectorStore.AddRange(new_chunks);

        m_logger.LogInformation("Added {ChunkCount} chunk(s) to vector store for session {SessionId}", new_chunks.Count, sessionId);
    }

    public async Task LoadDocumentsAsync(List<RagDocumentText> documents)
    {
        SessionId sessionId = m_userSessionHttpContext.SessionId;

        UserSession userSession = m_userSessionManager.GetOrAddSession(sessionId);

        m_logger.LogInformation("Loading {DocumentCount} document(s) for session {SessionId}", documents.Count, sessionId);

        IReadOnlyList<DocumentChunk> new_chunks = await ToDocumentChunk(documents);

        userSession.VectorStore.ReplaceAll(new_chunks);

        m_logger.LogInformation("Vector store replaced with {ChunkCount} chunk(s) for session {SessionId}", new_chunks.Count, sessionId);
    }

    private async Task<IReadOnlyList<DocumentChunk>> ToDocumentChunk(List<RagDocumentText> documents)
    {
        ConcurrentBag<DocumentChunk> new_chunks = [];

        SemaphoreSlim semaphore = new(5, 5);

        IEnumerable<Task> tasks = documents.Select(document =>
        {
            return Task.Run(async () =>
            {
                await semaphore.WaitAsync();

                try
                {
                    (string text, string documentName) = document;

                    // Make the chunk size scale with the imported text length
                    // Too large, and it will zero out empty spaces
                    int wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                    // Caps the floor and ceiling size of our chunk
                    int chunkSize = Math.Clamp(wordCount / 5, 25, 100);
                    int overlap = chunkSize / 5;

                    IReadOnlyList<string> chunks = ChunkText(text, chunkSize, overlap);

                    foreach (string chunk in chunks)
                    {
                        float[] embedding = await m_embeddingService.ToEmbeddingAsync(chunk);
                        new_chunks.Add(new DocumentChunk(chunk, documentName, embedding));
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
        });

        await Task.WhenAll(tasks);

        return new_chunks.ToList().AsReadOnly();
    }

    public async Task<RagQueryResponse> QueryAsync(RagQueryRequest request)
    {
        float[] questionEmbedding = await m_embeddingService.ToEmbeddingAsync(request.Question);

        SessionId sessionId = m_userSessionHttpContext.SessionId;

        UserSession userSession = m_userSessionManager.GetOrAddSession(sessionId);

        IReadOnlyList<DocumentChunk> relevantChunks = userSession.VectorStore.FindSimilar(questionEmbedding, request.TopK, request.Threshold);

        string context = string.Join("\n\n---\n\n", relevantChunks.Select(c => c.Text));

        string systemPrompt = """
            You are a helpful assistant that answers questions based strictly on
            the provided context. If the answer is not in the context, say
            "I couldn't find that in the document." Do not make up information.
            """;

        string userPrompt = $"""
            Context from the document:
            {context}

            Question: {request.Question}

            Answer based only on the context above:
            """;

        List<ChatMessage> messages = new()
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        ClientResult<ChatCompletion> response = await m_chatClient.CompleteChatAsync(messages);
        string answer = response.Value.Content[0].Text;

        return new(answer, relevantChunks.Select(c => c.Text).ToList().AsReadOnly());
    }

    public async IAsyncEnumerable<RagQueryStreamResponse> QueryStreamAsync(RagQueryStreamRequest request)
    {
        float[] questionEmbedding = await m_embeddingService.ToEmbeddingAsync(request.Question);

        SessionId sessionId = m_userSessionHttpContext.SessionId;

        UserSession userSession = m_userSessionManager.GetOrAddSession(sessionId);

        IReadOnlyList<DocumentChunk> relevantChunks = userSession.VectorStore.FindSimilar(questionEmbedding, request.TopK, request.Threshold);
        string context = string.Join("\n\n---\n\n", relevantChunks.Select(c => c.Text));

        string systemPrompt = """
            You are a helpful assistant that answers questions based strictly on
            the provided context. If the answer is not in the context, say
            "I couldn't find that in the document." Do not make up information.
            """;

        string userPrompt = $"""
            Context from the document:
            {context}

            Question: {request.Question}

            Answer based only on the context above:
            """;

        List<ChatMessage> messages = new()
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        IReadOnlyList<string> relevantChunkText = relevantChunks.Select(c => c.Text).ToList().AsReadOnly();

        bool isFirst = true;

        await foreach (StreamingChatCompletionUpdate? response in m_chatClient.CompleteChatStreamingAsync(messages))
        {
            if (response is null) continue;
            if (response.ContentUpdate.Count == 0) continue;

            string delta = response.ContentUpdate[0].Text;
            IReadOnlyList<string>? sourceChunks = isFirst ? relevantChunkText : null;

            yield return new(delta, sourceChunks);

            isFirst = false;
        }
    }

    // Splits long text into overlapping chunks
    // Overlapping ensures we don't lose context at chunk boundaries
    private static ReadOnlyCollection<string> ChunkText(string text, int chunkSize, int overlap)
    {
        string[] words  = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        List<string> chunks = [];

        for (int i=0; i<words.Length; i+=(chunkSize - overlap))
        {
            string chunk = string.Join(' ', words.Skip(i).Take(chunkSize));

            if (!string.IsNullOrWhiteSpace(chunk))
                chunks.Add(chunk);

            if (i + chunkSize >= words.Length)
                break;
        }

        return chunks.AsReadOnly();
    }

}