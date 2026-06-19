using FinanceAssistant.Application.Services;
using FinanceAssistant.Domain.Enums;
using FluentAssertions;

namespace FinanceAssistant.UnitTests.Services;

public class TransactionParserServiceTests
{
    private readonly TransactionParserService _sut = new();

    // -------------------------------------------------------------------------
    // Padrão simples: "{keyword} {valor}"
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("mercado 52", "Mercado", 52)]
    [InlineData("supermercado 230", "Mercado", 230)]
    [InlineData("feira 45", "Mercado", 45)]
    public void TryParse_SimpleKeywordExpense_Mercado(string input, string expectedCategory, decimal expectedAmount)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Expense);
        result.CategoryName.Should().Be(expectedCategory);
        result.Amount.Should().Be(expectedAmount);
    }

    [Theory]
    [InlineData("gasolina 180", "Combustível", 180)]
    [InlineData("combustivel 200", "Combustível", 200)]
    [InlineData("etanol 150", "Combustível", 150)]
    [InlineData("posto 220", "Combustível", 220)]
    public void TryParse_SimpleKeywordExpense_Combustivel(string input, string expectedCategory, decimal expectedAmount)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Expense);
        result.CategoryName.Should().Be(expectedCategory);
        result.Amount.Should().Be(expectedAmount);
    }

    [Theory]
    [InlineData("salario 5000", "Salário", 5000)]
    [InlineData("salário 6500", "Salário", 6500)]
    public void TryParse_SimpleKeywordIncome_Salario(string input, string expectedCategory, decimal expectedAmount)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Income);
        result.CategoryName.Should().Be(expectedCategory);
        result.Amount.Should().Be(expectedAmount);
    }

    [Theory]
    [InlineData("ifood 35", "Alimentação", 35)]
    [InlineData("restaurante 80", "Alimentação", 80)]
    [InlineData("pizza 60", "Alimentação", 60)]
    public void TryParse_SimpleKeywordExpense_Alimentacao(string input, string expectedCategory, decimal expectedAmount)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Expense);
        result.CategoryName.Should().Be(expectedCategory);
        result.Amount.Should().Be(expectedAmount);
    }

    [Theory]
    [InlineData("freelance 1500", "Freelance", 1500)]
    [InlineData("freela 800", "Freelance", 800)]
    public void TryParse_SimpleKeywordIncome_Freelance(string input, string expectedCategory, decimal expectedAmount)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Income);
        result.CategoryName.Should().Be(expectedCategory);
        result.Amount.Should().Be(expectedAmount);
    }

    // -------------------------------------------------------------------------
    // Padrão com descrição: "{keyword} {valor} {descrição}"
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_KeywordWithDescription_UsesDescriptionAsText()
    {
        var result = _sut.TryParse("mercado 52 compras da semana");

        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Mercado");
        result.Amount.Should().Be(52);
        result.Description.Should().Be("Compras da semana");
    }

    [Fact]
    public void TryParse_KeywordWithDescription_CapitalizesFirstLetter()
    {
        var result = _sut.TryParse("gasolina 180 abastecimento semanal");

        result!.Description.Should().StartWith("A");
        result.Description.Should().Be("Abastecimento semanal");
    }

    // -------------------------------------------------------------------------
    // Padrão "gastei/paguei/comprei {valor} {descrição}"
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("gastei 180 abastecendo o carro")]
    [InlineData("paguei 180 no posto")]
    [InlineData("comprei 180 de gasolina")]
    public void TryParse_ExpensePattern_IsExpense(string input)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Expense);
        result.Amount.Should().Be(180);
    }

    [Fact]
    public void TryParse_GasteiComCombustivel_InfereCategoria()
    {
        var result = _sut.TryParse("gastei 220 abastecendo o carro");

        result!.CategoryName.Should().Be("Combustível");
        result.Description.Should().Be("Abastecendo o carro");
    }

    [Fact]
    public void TryParse_GasteiComAlmoco_InfereCategoria()
    {
        var result = _sut.TryParse("gastei 75 no almoço");

        result!.CategoryName.Should().Be("Alimentação");
    }

    [Fact]
    public void TryParse_GasteiSemContexto_FallbackParaOutros()
    {
        var result = _sut.TryParse("gastei 50");

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Expense);
        result.CategoryName.Should().Be("Outros");
        result.Amount.Should().Be(50);
    }

    // -------------------------------------------------------------------------
    // Padrão "recebi {valor}"
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_RecebeiSemContexto_FallbackParaSalario()
    {
        var result = _sut.TryParse("recebi 5000");

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Income);
        result.CategoryName.Should().Be("Salário");
        result.Amount.Should().Be(5000);
    }

    [Fact]
    public void TryParse_RecebeiComDescricao_UsaDescricao()
    {
        var result = _sut.TryParse("recebi 1500 de freelance");

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Income);
        result.Amount.Should().Be(1500);
        result.Description.Should().Be("De freelance");
    }

    [Fact]
    public void TryParse_RecebeiComContextoFreelance_InfereCategoria()
    {
        var result = _sut.TryParse("recebi 2000 de freela");

        result!.CategoryName.Should().Be("Freelance");
    }

    // -------------------------------------------------------------------------
    // Formatação de valores decimais
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("mercado 52,50", 52.50)]
    [InlineData("mercado 52.50", 52.50)]
    [InlineData("gasolina 180,00", 180.00)]
    [InlineData("gasolina 99,9", 99.9)]
    public void TryParse_ValoresDecimais_ParseiaCorretamente(string input, decimal expected)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.Amount.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Casos que NÃO devem ser parseados (retorna null → vai para IA)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("hoje abasteci o civic e gastei 220 reais")]
    [InlineData("almocei com minha namorada e gastei 75")]
    [InlineData("comprei peças para o carro por 350 reais")]
    [InlineData("texto sem número nenhum")]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_TextoComplexo_RetornaNull(string input)
    {
        var result = _sut.TryParse(input);

        result.Should().BeNull();
    }

    [Fact]
    public void TryParse_KeywordDesconhecida_RetornaNull()
    {
        var result = _sut.TryParse("xyzabc 100");

        result.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Case insensitive
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("MERCADO 52")]
    [InlineData("Mercado 52")]
    [InlineData("mErCaDo 52")]
    public void TryParse_CaseInsensitive_Funciona(string input)
    {
        var result = _sut.TryParse(input);

        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Mercado");
        result.Amount.Should().Be(52);
    }

    // -------------------------------------------------------------------------
    // Tipo correto é inferido (Income vs Expense)
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_KeywordDeReceita_RetornaTipoIncome()
    {
        var result = _sut.TryParse("investimento 500");

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Income);
        result.CategoryName.Should().Be("Investimentos");
    }

    [Fact]
    public void TryParse_KeywordDeDespesa_RetornaTipoExpense()
    {
        var result = _sut.TryParse("veterinario 350");

        result.Should().NotBeNull();
        result!.Type.Should().Be(TransactionType.Expense);
        result.CategoryName.Should().Be("Pets");
    }
}
