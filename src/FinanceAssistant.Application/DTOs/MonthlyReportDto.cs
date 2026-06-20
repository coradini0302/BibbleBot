namespace FinanceAssistant.Application.DTOs;

public record MonthlyReportDto(
    string UserName,
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    IReadOnlyList<TransactionDto> Transactions,
    IReadOnlyList<CategorySummaryDto> ExpensesByCategory,
    IReadOnlyList<CategorySummaryDto> IncomeByCategory
);
