namespace FinanceAssistant.Infrastructure.AI.OpenAI;

public class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
}
