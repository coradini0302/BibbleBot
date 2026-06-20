using ClosedXML.Excel;
using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Enums;

namespace FinanceAssistant.Infrastructure.Reports;

public class ExcelReportGeneratorService : IReportGeneratorService
{
    public Task<ReportFile> GenerateAsync(MonthlyReportDto data, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();

        AddResumoSheet(workbook, data);
        AddTransacoesSheet(workbook, data);
        AddCategoriaSheet(workbook, data.ExpensesByCategory, "Gastos por Categoria");
        AddCategoriaSheet(workbook, data.IncomeByCategory, "Receitas por Categoria");

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);

        var monthName = new DateTime(data.Year, data.Month, 1).ToString("MMMM-yyyy");
        var fileName = $"relatorio-{monthName}.xlsx";

        return Task.FromResult(new ReportFile(
            ms.ToArray(),
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ));
    }

    private static void AddResumoSheet(XLWorkbook wb, MonthlyReportDto data)
    {
        var ws = wb.Worksheets.Add("Resumo");
        var monthLabel = new DateTime(data.Year, data.Month, 1).ToString("MMMM/yyyy");

        ws.Cell("A1").Value = $"Relatorio Financeiro - {monthLabel}";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;
        ws.Range("A1:B1").Merge();

        ws.Cell("A3").Value = "Receitas";
        ws.Cell("B3").Value = data.TotalIncome;
        ws.Cell("B3").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("B3").Style.Font.FontColor = XLColor.DarkGreen;

        ws.Cell("A4").Value = "Despesas";
        ws.Cell("B4").Value = data.TotalExpense;
        ws.Cell("B4").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("B4").Style.Font.FontColor = XLColor.DarkRed;

        ws.Cell("A5").Value = "Saldo";
        ws.Cell("B5").Value = data.Balance;
        ws.Cell("B5").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("B5").Style.Font.Bold = true;
        ws.Cell("B5").Style.Font.FontColor = data.Balance >= 0 ? XLColor.DarkGreen : XLColor.DarkRed;

        ws.Column("A").Width = 20;
        ws.Column("B").Width = 20;
    }

    private static void AddTransacoesSheet(XLWorkbook wb, MonthlyReportDto data)
    {
        var ws = wb.Worksheets.Add("Transacoes");

        ws.Cell("A1").Value = "Data";
        ws.Cell("B1").Value = "Tipo";
        ws.Cell("C1").Value = "Categoria";
        ws.Cell("D1").Value = "Descricao";
        ws.Cell("E1").Value = "Valor";

        var header = ws.Range("A1:E1");
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var t in data.Transactions)
        {
            ws.Cell(row, 1).Value = t.CreatedAt.ToString("dd/MM/yyyy");
            ws.Cell(row, 2).Value = t.Type == TransactionType.Expense ? "Despesa" : "Receita";
            ws.Cell(row, 3).Value = t.CategoryName;
            ws.Cell(row, 4).Value = t.Description;
            ws.Cell(row, 5).Value = t.Amount;
            ws.Cell(row, 5).Style.NumberFormat.Format = "R$ #,##0.00";

            if (t.Type == TransactionType.Expense)
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
            else
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;

            row++;
        }

        ws.Columns().AdjustToContents();
    }

    private static void AddCategoriaSheet(XLWorkbook wb, IReadOnlyList<CategorySummaryDto> items, string sheetName)
    {
        var ws = wb.Worksheets.Add(sheetName);

        ws.Cell("A1").Value = "Categoria";
        ws.Cell("B1").Value = "Total";
        ws.Range("A1:B1").Style.Font.Bold = true;
        ws.Range("A1:B1").Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        foreach (var item in items)
        {
            ws.Cell(row, 1).Value = item.CategoryName;
            ws.Cell(row, 2).Value = item.Total;
            ws.Cell(row, 2).Style.NumberFormat.Format = "R$ #,##0.00";
            row++;
        }

        ws.Columns().AdjustToContents();
    }
}
