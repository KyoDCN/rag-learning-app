using System.ComponentModel.DataAnnotations;

namespace RagDemo.Api.DTOs;

public record QueryStreamRequest([Required, MinLength(1)] string Question, int? TopK = null, float? Threshold = null);
