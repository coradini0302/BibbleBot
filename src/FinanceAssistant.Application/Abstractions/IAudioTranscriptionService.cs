namespace FinanceAssistant.Application.Abstractions;

public interface IAudioTranscriptionService
{
    Task<string?> TranscribeAsync(byte[] audioBytes, CancellationToken cancellationToken = default);
}
