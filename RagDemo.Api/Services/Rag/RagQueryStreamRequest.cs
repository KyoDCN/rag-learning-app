namespace RagDemo.Api.Services.Rag;

public record RagQueryStreamRequest(string Question, int? TopK = null, float? Threshold = null);
