namespace RagDemo.Api.Services.Rag;

public record RagQueryRequest(string Question, int? TopK = null, float? Threshold = null);
