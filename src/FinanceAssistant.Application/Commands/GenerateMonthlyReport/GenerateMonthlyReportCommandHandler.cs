using FinanceAssistant.Application.Abstractions;
using FinanceAssistant.Application.DTOs;
using FinanceAssistant.Domain.Enums;
using FinanceAssistant.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceAssistant.Application.Commands.GenerateMonthlyReport;

public class GenerateMonthlyReportCommandHandler : IRequestHandler<GenerateMonthlyReportCommand, ReportFile?>
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IReportGeneratorService _excelGenerator;
    private readonly IReportGeneratorService _csvGenerator;

    public GenerateMonthlyReportCommandHandler(
        IUserRepository userRepository,
        ITransactionRepository transactionRepository,
        [FromKeyedServices(ReportFormat.Excel)] IReportGeneratorService excelGenerator,
        [FromKeyedServices(ReportFormat.Csv)] IReportGeneratorService csvGenerator)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _excelGenerator = excelGenerator;
        _csvGenerator = csvGenerator;
    }

    public async Task<ReportFile?> Handle(GenerateMonthlyReportCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(request.TelegramUserId, cancellationToken);
        if (user is null) return null;

        var transactions = await _transactionRepository.GetByUserAndMonthAsync(
            user.Id, request.Year, request.Month, cancellationToken);

        if (transactions.Count == 0) return null;

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummaryDto(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Total)
            .ToList();

        var incomeByCategory = transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummaryDto(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Total)
            .ToList();

        var transactionDtos = transactions
            .Select(t => new TransactionDto(t.Id, t.Category.Name, t.Description, t.Amount, t.Type, t.CreatedAt))
            .ToList();

        var reportData = new MonthlyReportDto(
            user.Name,
            request.Year,
            request.Month,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            transactionDtos,
            expensesByCategory,
            incomeByCategory
        );

        var generator = user.ReportFormat == ReportFormat.Csv ? _csvGenerator : _excelGenerator;
        return await generator.GenerateAsync(reportData, cancellationToken);
    }
}
