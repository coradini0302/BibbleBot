namespace FinanceAssistant.Application.DTOs;

public record MonthlySummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance
);
