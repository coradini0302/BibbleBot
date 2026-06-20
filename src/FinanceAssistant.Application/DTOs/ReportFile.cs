namespace FinanceAssistant.Application.DTOs;

public record ReportFile(byte[] Content, string FileName, string MimeType);
