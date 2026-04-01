using Microsoft.AspNetCore.Mvc;
using RagDemo.Contracts;
using RagDemo.Api.Services;
using RagDemo.Api.Services.Rag;

namespace RagDemo.Api.Controllers;

[ApiController]
[Route("api/rag")]
public class RagController : ControllerBase
{
    private readonly RagService m_rag;

    public RagController(RagService rag)
    {
        m_rag = rag;
    }

    /// <summary>
    /// Adds documents to the session's existing vector store without clearing prior content.
    /// </summary>
    [HttpPost("document/add")]
    public async Task<IResult> AddDocumentAsync(List<AddDocumentRequest> request)
    {
        List<RagDocumentText> documents = request.Select(request => 
            new RagDocumentText(request.DocumentText, request.DocumentName)).ToList();

        await m_rag.AddDocumentsAsync(documents);
        
        return Results.Ok(new { message = "Documents added successfully" });
    }

    /// <summary>
    /// Replaces the session's entire vector store with the provided documents.
    /// </summary>
    [HttpPost("document/load")]
    public async Task<IResult> LoadDocumentAsync(List<LoadDocumentRequest> request)
    {
        List<RagDocumentText> documents = request.Select(request => 
            new RagDocumentText(request.DocumentText, request.DocumentName)).ToList();

        await m_rag.LoadDocumentsAsync(documents);
        
        return Results.Ok(new { message = "Documents loaded successfully" });
    }

    /// <summary>
    /// Clears all documents from the session's vector store.
    /// </summary>
    [HttpDelete("document/clear")]
    public IResult ClearDocumentsAsync()
    {
        m_rag.ClearVectorStore();

        return Results.Ok("Successfully cleared Vector Store.");
    }

    /// <summary>
    /// Returns the number of chunks currently stored in the session's vector store.
    /// </summary>
    [HttpGet("document/status")]
    public IResult GetDocumentStatusAsync()
    {
        return Results.Ok(m_rag.GetVectorStoreStatus());
    }

    /// <summary>
    /// Queries the session's vector store and returns a complete answer from the LLM.
    /// </summary>
    [HttpPost("query")]
    public async Task<IResult> QueryAsync(QueryRequest request)
    {
        RagQueryRequest ragRequest = new(request.Question, request.TopK, request.Threshold);

        RagQueryResponse ragResponse = await m_rag.QueryAsync(ragRequest);

        return Results.Ok(ragResponse);
    }

    /// <summary>
    /// Queries the session's vector store and streams the LLM response via Server-Sent Events.
    /// </summary>
    [HttpPost("query/stream")]
    public async Task<IResult> QueryStreamAsync(QueryStreamRequest request)
    {
        RagQueryStreamRequest ragRequest = new(request.Question, request.TopK, request.Threshold);

        return Results.ServerSentEvents(m_rag.QueryStreamAsync(ragRequest));
    }
}