using System.ComponentModel.DataAnnotations;

namespace RagDemo.Api.DTOs;

public record AddDocumentRequest([Required, MinLength(1)] string DocumentText, [Required, MinLength(1)] string DocumentName);
