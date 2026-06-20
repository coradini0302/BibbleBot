using System.Text.Json;
using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Enums;
using global::OpenAI.Chat;
using Microsoft.Extensions.Logging;

namespace FinanceAssistant.Infrastructure.AI.OpenAI;

public class OpenAITransactionExtractionService : ITransactionExtractionService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAITransactionExtractionService> _logger;

    private const string SystemPrompt = """
        You are a financial transaction parser for a Brazilian personal finance app.
        Extract transaction details from the user's message in Brazilian Portuguese.

        Return ONLY a valid JSON object with exactly these fields:
        {
          "type": "Expense" or "Income",
          "category": one of the available categories,
          "description": "brief description in Portuguese",
          "amount": positive decimal number
        }

        Available categories:
        - Income: Salário, Freelance, Investimentos
        - Expense: Mercado, Alimentação, Combustível, Lazer, Pets, Casa, Saúde, Veículo, Outros

        Rules:
        1. "type" must be exactly "Expense" or "Income"
        2. "category" must be exactly one from the list above (case-sensitive, with accents)
        3. "description" should be concise, in Portuguese, max 100 characters
        4. "amount" must be a positive decimal number, no currency symbol
        5. If unsure about category, use "Outros" for expenses or "Salário" for income
        """;

    public OpenAITransactionExtractionService(ChatClient chatClient, ILogger<OpenAITransactionExtractionService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<TransactionExtractionResult> ExtractAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var completion = await _chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(SystemPrompt),
                    new UserChatMessage(text)
                ],
                new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                },
                cancellationToken
            );

            var json = completion.Value.Content[0].Text;
            _logger.LogDebug("OpenAI response: {Json}", json);

            return ParseResponse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao chamar OpenAI para texto: {Text}", text);
            return Failure(ex.Message);
        }
    }

    private static TransactionExtractionResult ParseResponse(string json)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<ExtractionDto>(json, options);

            if (dto is null)
                return Failure("Resposta vazia da IA.");

            if (dto.Amount <= 0)
                return Failure("Valor inválido retornado pela IA.");

            if (string.IsNullOrWhiteSpace(dto.Category))
                return Failure("Categoria não identificada.");

            var type = dto.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)
                ? TransactionType.Income
                : TransactionType.Expense;

            return new TransactionExtractionResult(true, type, dto.Category, dto.Description, dto.Amount);
        }
        catch (JsonException ex)
        {
            return Failure($"JSON inválido: {ex.Message}");
        }
    }

    public async Task<TransactionExtractionResult> ExtractFromImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            var imageData = BinaryData.FromBytes(imageBytes);
            var completion = await _chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(SystemPrompt),
                    new UserChatMessage(
                        ChatMessageContentPart.CreateTextPart("Analise este comprovante e extraia os dados da transação financeira."),
                        ChatMessageContentPart.CreateImagePart(imageData, "image/jpeg")
                    )
                ],
                new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                },
                cancellationToken
            );

            var json = completion.Value.Content[0].Text;
            _logger.LogDebug("OpenAI image response: {Json}", json);
            return ParseResponse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar imagem com OpenAI");
            return Failure(ex.Message);
        }
    }

    private static TransactionExtractionResult Failure(string error)
        => new(false, TransactionType.Expense, "Outros", string.Empty, 0, error);

    private record ExtractionDto(string Type, string Category, string Description, decimal Amount);
}
