using System.ComponentModel.DataAnnotations;

namespace RagDemo.Contracts;

public record LoadDocumentRequest([Required, MinLength(1)] string DocumentText, [Required, MinLength(1)] string DocumentName);
