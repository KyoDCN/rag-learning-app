using System.ComponentModel.DataAnnotations;

namespace RagDemo.Contracts;

public record AddDocumentRequest([Required, MinLength(1)] string DocumentText, [Required, MinLength(1)] string DocumentName);
