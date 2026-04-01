using Microsoft.AspNetCore.Mvc;
using RagDemo.Api.DTOs;
using RagDemo.Api.Services;
using RagDemo.Api.Services.Rag;

namespace RagDemo.Api.Controllers;

[ApiController]
[Route("api/rag")]
public class RagController(RagService rag) : ControllerBase
{
    private readonly RagService m_rag = rag;

    [HttpPost("document/add")]
    public async Task<IResult> AddDocumentAsync(List<AddDocumentRequest> request)
    {
        List<RagDocumentText> documents = request.Select(request => 
            new RagDocumentText(request.DocumentText, request.DocumentName)).ToList();

        await m_rag.AddDocumentsAsync(documents);
        
        return Results.Ok(new { message = "Documents added successfully" });
    }

    [HttpPost("document/load")]
    public async Task<IResult> LoadDocumentAsync(List<LoadDocumentRequest> request)
    {
        List<RagDocumentText> documents = request.Select(request => 
            new RagDocumentText(request.DocumentText, request.DocumentName)).ToList();

        await m_rag.LoadDocumentsAsync(documents);
        
        return Results.Ok(new { message = "Documents loaded successfully" });
    }

    [HttpDelete("document/clear")]
    public IResult ClearDocumentsAsync()
    {
        m_rag.ClearVectorStore();

        return Results.Ok("Successfully cleared Vector Store.");
    }

    [HttpGet("document/status")]
    public IResult GetDocumentStatusAsync()
    {
        return Results.Ok(m_rag.GetVectorStoreStatus());
    }

    [HttpPost("query")]
    public async Task<IResult> QueryAsync(QueryRequest request)
    {
        RagQueryRequest ragRequest = new(request.Question, request.TopK, request.Threshold);

        RagQueryResponse ragResponse = await m_rag.QueryAsync(ragRequest);

        return Results.Ok(ragResponse);
    }

    [HttpPost("query/stream")]
    public async Task<IResult> QueryStreamAsync(QueryStreamRequest request)
    {
        RagQueryStreamRequest ragRequest = new(request.Question, request.TopK, request.Threshold);

        return Results.ServerSentEvents(m_rag.QueryStreamAsync(ragRequest));
    }
}