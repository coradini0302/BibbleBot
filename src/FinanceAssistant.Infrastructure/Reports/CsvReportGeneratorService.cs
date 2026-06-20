using System.Globalization;
using System.Text;
using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Enums;

namespace FinanceAssistant.Infrastructure.Reports;

public class CsvReportGeneratorService : IReportGeneratorService
{
    public Task<ReportFile> GenerateAsync(MonthlyReportDto data, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Data;Tipo;Categoria;Descricao;Valor");

        foreach (var t in data.Transactions)
        {
            var tipo = t.Type == TransactionType.Expense ? "Despesa" : "Receita";
            var valor = t.Amount.ToString("F2", CultureInfo.InvariantCulture);
            var descricao = t.Description.Replace(";", ",");
            sb.AppendLine($"{t.CreatedAt:dd/MM/yyyy};{tipo};{t.CategoryName};{descricao};{valor}");
        }

        sb.AppendLine();
        sb.AppendLine($"Receitas;{data.TotalIncome.ToString("F2", CultureInfo.InvariantCulture)}");
        sb.AppendLine($"Despesas;{data.TotalExpense.ToString("F2", CultureInfo.InvariantCulture)}");
        sb.AppendLine($"Saldo;{data.Balance.ToString("F2", CultureInfo.InvariantCulture)}");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var monthName = new DateTime(data.Year, data.Month, 1).ToString("MMMM-yyyy");

        return Task.FromResult(new ReportFile(bytes, $"relatorio-{monthName}.csv", "text/csv"));
    }
}
