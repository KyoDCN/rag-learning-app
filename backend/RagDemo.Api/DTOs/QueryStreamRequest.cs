namespace RagDemo.Api.DTOs;

public record QueryStreamRequest(string Question, int? TopK = null, float? Threshold = null);
