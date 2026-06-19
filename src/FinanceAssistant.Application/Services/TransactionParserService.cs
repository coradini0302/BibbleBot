using System.Globalization;
using System.Text.RegularExpressions;
using FinanceAssistant.Domain.Enums;

namespace FinanceAssistant.Application.Services;

public class TransactionParserService
{
    // "{keyword} {valor}" ou "{keyword} {valor} {descrição}"
    private static readonly Regex SimplePattern =
        new(@"^(\w+)\s+(\d+(?:[.,]\d{1,2})?)(?:\s+(.+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "gastei/paguei/comprei {valor} ..."
    private static readonly Regex ExpensePattern =
        new(@"^(?:gastei|paguei|comprei)\s+(\d+(?:[.,]\d{1,2})?)(?:\s+(.+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "recebi {valor} ..."
    private static readonly Regex IncomePattern =
        new(@"^recebi\s+(\d+(?:[.,]\d{1,2})?)(?:\s+(.+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly (string[] Keywords, string Category, TransactionType Type)[] KeywordMap =
    [
        (["mercado", "supermercado", "feira"], "Mercado", TransactionType.Expense),
        (["gasolina", "combustivel", "combustível", "etanol", "diesel", "posto", "abastec"], "Combustível", TransactionType.Expense),
        (["salario", "salário"], "Salário", TransactionType.Income),
        (["ifood", "restaurante", "almoco", "almoço", "jantar", "lanche", "pizza", "hamburguer", "hambúrguer", "comida", "refeicao", "refeição", "almocei", "jantei", "lanchei"], "Alimentação", TransactionType.Expense),
        (["cinema", "netflix", "spotify", "show", "bar", "balada", "lazer", "jogo", "games"], "Lazer", TransactionType.Expense),
        (["veterinario", "veterinário", "petshop", "pet", "vet", "luke"], "Pets", TransactionType.Expense),
        (["aluguel", "condominio", "condomínio", "agua", "luz", "energia", "internet", "casa"], "Casa", TransactionType.Expense),
        (["farmacia", "farmácia", "medico", "médico", "consulta", "remedio", "remédio", "saude", "saúde", "hospital", "dentista"], "Saúde", TransactionType.Expense),
        (["freelance", "freela"], "Freelance", TransactionType.Income),
        (["investimento", "investimentos", "dividendo", "dividendos", "rendimento", "rendimentos"], "Investimentos", TransactionType.Income),
        (["carro", "veiculo", "veículo", "automovel", "automóvel", "oficina", "mecanico", "mecânico", "peca", "peça", "civic"], "Veículo", TransactionType.Expense),
    ];

    public ParsedTransaction? TryParse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var normalized = text.Trim().ToLowerInvariant();

        // "recebi {valor} ..."
        var incomeMatch = IncomePattern.Match(normalized);
        if (incomeMatch.Success)
        {
            var amount = ParseDecimal(incomeMatch.Groups[1].Value);
            var raw = incomeMatch.Groups[2].Success ? incomeMatch.Groups[2].Value : string.Empty;
            var category = InferCategoryName(normalized, TransactionType.Income, "Salário");
            var description = string.IsNullOrWhiteSpace(raw) ? category : Capitalize(raw);
            return new ParsedTransaction(TransactionType.Income, category, description, amount);
        }

        // "gastei/paguei/comprei {valor} ..."
        var expenseMatch = ExpensePattern.Match(normalized);
        if (expenseMatch.Success)
        {
            var amount = ParseDecimal(expenseMatch.Groups[1].Value);
            var raw = expenseMatch.Groups[2].Success ? expenseMatch.Groups[2].Value : string.Empty;
            var category = InferCategoryName(normalized, TransactionType.Expense, "Outros");
            var description = string.IsNullOrWhiteSpace(raw) ? category : Capitalize(raw);
            return new ParsedTransaction(TransactionType.Expense, category, description, amount);
        }

        // "{keyword} {valor}" ou "{keyword} {valor} {descrição}"
        var simpleMatch = SimplePattern.Match(normalized);
        if (simpleMatch.Success)
        {
            var keyword = simpleMatch.Groups[1].Value;
            var amount = ParseDecimal(simpleMatch.Groups[2].Value);
            var entry = FindByKeyword(keyword);
            if (entry is not null)
            {
                var description = simpleMatch.Groups[3].Success
                    ? Capitalize(simpleMatch.Groups[3].Value)
                    : entry.Category;
                return new ParsedTransaction(entry.Type, entry.Category, description, amount);
            }
        }

        return null;
    }

    private string InferCategoryName(string text, TransactionType expectedType, string fallback)
    {
        foreach (var (keywords, category, type) in KeywordMap)
        {
            if (type == expectedType && keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return category;
        }
        return fallback;
    }

    private KeywordEntry? FindByKeyword(string keyword)
    {
        foreach (var (keywords, category, type) in KeywordMap)
        {
            if (keywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
                return new KeywordEntry(category, type);
        }
        return null;
    }

    private static decimal ParseDecimal(string value) =>
        decimal.Parse(value.Replace(",", "."), CultureInfo.InvariantCulture);

    private static string Capitalize(string text) =>
        string.IsNullOrWhiteSpace(text) ? text : char.ToUpper(text[0]) + text[1..];

    private record KeywordEntry(string Category, TransactionType Type);
}

public record ParsedTransaction(
    TransactionType Type,
    string CategoryName,
    string Description,
    decimal Amount
);
