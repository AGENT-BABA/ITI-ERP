using System.Text.Json;
using ClosedXML.Excel;
using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.TradeMaster;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ITI.ERP.Shared.Constants;

namespace ITI.ERP.Application.Services;

public class TradeMasterService : ITradeMasterService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    private static readonly string[] RequiredHeaders = { "TradeCode", "TradeName", "TotalSeats", "DurationMonths" };

    public TradeMasterService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TradeMasterPreviewDto>> PreviewImportAsync(Stream fileStream, string fileName, Guid? instituteIdOverride = null, CancellationToken ct = default)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = instituteIdOverride ?? _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<TradeMasterPreviewDto>.Failure("Institute not found. Please specify an institute.");

        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(fileStream);
        }
        catch
        {
            return Result<TradeMasterPreviewDto>.Failure("Invalid Excel file. Please upload a valid .xlsx file.");
        }

        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
            return Result<TradeMasterPreviewDto>.Failure("The Excel file contains no worksheets.");

        var headerErrors = ValidateHeaders(worksheet);
        if (headerErrors.Count > 0)
            return Result<TradeMasterPreviewDto>.Failure(headerErrors);

        var rows = new List<TradeMasterRowDto>();
        var errors = new List<TradeMasterValidationError>();
        var tradeCodesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tradeNamesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var tradeCode = GetCellString(worksheet, row, 1);
            var tradeName = GetCellString(worksheet, row, 2);
            var totalSeatsStr = GetCellString(worksheet, row, 3);
            var durationMonthsStr = GetCellString(worksheet, row, 4);

            if (string.IsNullOrWhiteSpace(tradeCode) && string.IsNullOrWhiteSpace(tradeName) &&
                string.IsNullOrWhiteSpace(totalSeatsStr) && string.IsNullOrWhiteSpace(durationMonthsStr))
                continue;

            var rowDto = new TradeMasterRowDto
            {
                RowNumber = row,
                TradeCode = tradeCode,
                TradeName = tradeName,
            };

            if (string.IsNullOrWhiteSpace(tradeCode))
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TradeCode", ErrorMessage = "Trade Code is required." });
            else if (tradeCode.Length > 20)
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TradeCode", ErrorMessage = "Trade Code must be 20 characters or less." });
            else if (!tradeCodesInFile.Add(tradeCode))
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TradeCode", ErrorMessage = $"Duplicate Trade Code '{tradeCode}' found in file." });

            if (string.IsNullOrWhiteSpace(tradeName))
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TradeName", ErrorMessage = "Trade Name is required." });
            else if (tradeName.Length > 200)
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TradeName", ErrorMessage = "Trade Name must be 200 characters or less." });
            else if (!tradeNamesInFile.Add(tradeName))
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TradeName", ErrorMessage = $"Duplicate Trade Name '{tradeName}' found in file." });

            if (string.IsNullOrWhiteSpace(totalSeatsStr) || !int.TryParse(totalSeatsStr, out var totalSeats))
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TotalSeats", ErrorMessage = "Total Seats must be a valid integer." });
            else if (totalSeats < 1 || totalSeats > 1000)
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "TotalSeats", ErrorMessage = "Total Seats must be between 1 and 1000." });
            else
                rowDto.TotalSeats = totalSeats;

            if (string.IsNullOrWhiteSpace(durationMonthsStr) || !int.TryParse(durationMonthsStr, out var durationMonths))
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "DurationMonths", ErrorMessage = "Duration (Months) must be a valid integer." });
            else if (durationMonths < 1 || durationMonths > 48)
                errors.Add(new TradeMasterValidationError { RowNumber = row, Field = "DurationMonths", ErrorMessage = "Duration (Months) must be between 1 and 48." });
            else
                rowDto.DurationMonths = durationMonths;

            rows.Add(rowDto);
        }

        if (rows.Count == 0)
            return Result<TradeMasterPreviewDto>.Failure("The Excel file contains no data rows.");

        var duplicateCodesQuery = _context.Trades
            .Where(t => !t.IsDeleted &&
                        tradeCodesInFile.Contains(t.Code));

        if (!isSuperAdmin)
            duplicateCodesQuery = duplicateCodesQuery.Where(t => t.InstituteId == instituteId.Value);

        var duplicateCodesInDb = await duplicateCodesQuery
            .Select(t => t.Code)
            .ToListAsync(ct);

        foreach (var code in duplicateCodesInDb)
        {
            var affectedRows = rows.Where(r => r.TradeCode == code).ToList();
            foreach (var r in affectedRows)
                errors.Add(new TradeMasterValidationError { RowNumber = r.RowNumber, Field = "TradeCode", ErrorMessage = $"Trade Code '{code}' already exists in this session." });
        }

        var duplicateNamesQuery = _context.Trades
            .Where(t => !t.IsDeleted &&
                        tradeNamesInFile.Contains(t.Name));

        if (!isSuperAdmin)
            duplicateNamesQuery = duplicateNamesQuery.Where(t => t.InstituteId == instituteId.Value);

        var duplicateNamesInDb = await duplicateNamesQuery
            .Select(t => t.Name)
            .ToListAsync(ct);

        foreach (var name in duplicateNamesInDb)
        {
            var affectedRows = rows.Where(r => r.TradeName == name).ToList();
            foreach (var r in affectedRows)
                errors.Add(new TradeMasterValidationError { RowNumber = r.RowNumber, Field = "TradeName", ErrorMessage = $"Trade Name '{name}' already exists in this session." });
        }

        var errorRowNumbers = errors.Select(e => e.RowNumber).ToHashSet();
        var validRows = rows.Where(r => !errorRowNumbers.Contains(r.RowNumber)).ToList();

        var preview = new TradeMasterPreviewDto
        {
            TotalRows = rows.Count,
            ValidRows = validRows.Count,
            InvalidRows = errorRowNumbers.Count,
            Rows = rows,
            Errors = errors,
            FileName = fileName
        };

        return Result<TradeMasterPreviewDto>.Success(preview);
    }

    public async Task<Result<TradeMasterImportResultDto>> ConfirmImportAsync(TradeMasterPreviewDto preview, Guid? instituteIdOverride = null, CancellationToken ct = default)
    {
        var instituteId = instituteIdOverride ?? _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<TradeMasterImportResultDto>.Failure("Institute not found.");

        var errorRowNumbers = preview.Errors.Select(e => e.RowNumber).ToHashSet();
        var validRows = preview.Rows.Where(r => !errorRowNumbers.Contains(r.RowNumber)).ToList();

        if (validRows.Count == 0)
            return Result<TradeMasterImportResultDto>.Failure("No valid rows to import.");

        var trades = validRows.Select(r => new Trade
        {
            InstituteId = instituteId.Value,
            Code = r.TradeCode.Trim(),
            Name = r.TradeName.Trim(),
            TotalSeats = r.TotalSeats,
            DurationInMonths = r.DurationMonths,
            IsDeleted = false
        }).ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.Trades.AddRangeAsync(trades, ct);
            await _context.SaveChangesAsync(ct);

            var history = new TradeImportHistory
            {
                InstituteId = instituteId.Value,
                FileName = preview.FileName ?? "unknown.xlsx",
                ImportDate = DateTime.UtcNow,
                NumberOfTradesImported = trades.Count,
                Status = ImportStatus.Success,
                ValidationErrors = preview.Errors.Count > 0 ? JsonSerializer.Serialize(preview.Errors) : null
            };
            await _context.TradeImportHistories.AddAsync(history, ct);
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<TradeMasterImportResultDto>.Success(new TradeMasterImportResultDto
        {
            Success = true,
            TradesImported = trades.Count,
            Message = $"{trades.Count} trade(s) imported successfully."
        });
    }

    public Task<Result<byte[]>> DownloadTemplateAsync(CancellationToken ct)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("TradeMaster");

        for (int i = 0; i < RequiredHeaders.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = RequiredHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 30;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 18;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(Result<byte[]>.Success(stream.ToArray()));
    }

    public async Task<Result<PaginatedList<TradeMasterHistoryDto>>> GetImportHistoryAsync(PaginationRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        if (instituteId is null)
            return Result<PaginatedList<TradeMasterHistoryDto>>.Failure("Institute not found.");

        IQueryable<TradeImportHistory> query = _context.TradeImportHistories
            .AsNoTracking()
            .OrderByDescending(t => t.ImportDate);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == instituteId!.Value);

        var paginatedList = await PaginatedList<TradeMasterHistoryDto>.CreateAsync(
            query.Select(t => new TradeMasterHistoryDto
            {
                Id = t.Id,
                FileName = t.FileName,
                ImportedByName = t.CreatedBy.HasValue
                    ? _context.Users.Where(u => u.Id == t.CreatedBy.Value).Select(u => u.Username).FirstOrDefault() ?? "Unknown"
                    : "System",
                ImportDate = t.ImportDate,
                NumberOfTradesImported = t.NumberOfTradesImported,
                Status = t.Status,
                ValidationErrors = t.ValidationErrors
            }),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<TradeMasterHistoryDto>>.Success(paginatedList);
    }

    private static List<string> ValidateHeaders(IXLWorksheet worksheet)
    {
        var errors = new List<string>();
        var firstRow = worksheet.Row(1);
        var actualHeaders = new List<string>();

        for (int col = 1; col <= (firstRow.LastCellUsed()?.Address.ColumnNumber ?? 0); col++)
        {
            actualHeaders.Add(firstRow.Cell(col).GetString().Trim());
        }

        foreach (var header in RequiredHeaders)
        {
            if (!actualHeaders.Any(h => string.Equals(h, header, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"Missing required column: '{header}'.");
        }

        return errors;
    }

    private static string GetCellString(IXLWorksheet worksheet, int row, int col)
    {
        return worksheet.Cell(row, col).GetString().Trim();
    }
}
