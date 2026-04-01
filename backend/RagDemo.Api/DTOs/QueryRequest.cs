namespace RagDemo.Api.DTOs;

public record QueryRequest(string Question, int? TopK = null, float? Threshold = null);
