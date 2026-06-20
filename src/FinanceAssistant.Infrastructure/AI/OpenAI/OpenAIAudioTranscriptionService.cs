using FinanceAssistant.Application.Abstractions;
using Microsoft.Extensions.Logging;
using OpenAI.Audio;

namespace FinanceAssistant.Infrastructure.AI.OpenAI;

public class OpenAIAudioTranscriptionService : IAudioTranscriptionService
{
    private readonly AudioClient _audioClient;
    private readonly ILogger<OpenAIAudioTranscriptionService> _logger;

    public OpenAIAudioTranscriptionService(AudioClient audioClient, ILogger<OpenAIAudioTranscriptionService> logger)
    {
        _audioClient = audioClient;
        _logger = logger;
    }

    public async Task<string?> TranscribeAsync(byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            using var stream = new MemoryStream(audioBytes);
            var result = await _audioClient.TranscribeAudioAsync(
                stream,
                "audio.ogg",
                new AudioTranscriptionOptions { Language = "pt" },
                cancellationToken);

            return result.Value.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao transcrever audio");
            return null;
        }
    }
}
