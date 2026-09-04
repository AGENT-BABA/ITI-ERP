using System.Text.Json;
using ClosedXML.Excel;
using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.StudentImport;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ITI.ERP.Shared.Constants;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ITI.ERP.Application.Services;

public class StudentImportService : IStudentImportService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITradeAccessService _tradeAccess;

    private static readonly string[] RequiredHeaders =
    {
        "Application Id Display",
        "Candidate Name",
        "Gender",
        "DOB",
        "Mobile No",
        "Allotted Category",
        "Allotted Round",
        "Admitted Date Time"
    };

    public StudentImportService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITradeAccessService tradeAccess)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tradeAccess = tradeAccess;
    }

    public async Task<Result<StudentImportPreviewDto>> PreviewImportAsync(Guid tradeId, Stream fileStream, string fileName, Guid? batchId, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null)
            return Result<StudentImportPreviewDto>.Failure("Institute not found.");

        if (academicSessionId is null)
            return Result<StudentImportPreviewDto>.Failure("Academic Session not found.");

        var tradeAccessResult = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!tradeAccessResult.IsSuccess)
            return Result<StudentImportPreviewDto>.Failure(tradeAccessResult.Error!);

        var trade = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.InstituteId == instituteId.Value && !t.IsDeleted, ct);
        if (trade is null)
            return Result<StudentImportPreviewDto>.Failure("Trade not found or does not belong to your institute.");

        var institute = await _context.Institutes.FindAsync(instituteId.Value);
        var session = await _context.AcademicSessions.FindAsync(academicSessionId.Value);

        ISheet sheet;
        try
        {
            IWorkbook workbook;
            using (var stream = fileStream)
            {
                workbook = WorkbookFactory.Create(stream);
            }
            sheet = workbook.GetSheetAt(0);
        }
        catch
        {
            return Result<StudentImportPreviewDto>.Failure("Invalid Excel file. Please upload a valid .xlsx or .xls file.");
        }

        if (sheet is null)
            return Result<StudentImportPreviewDto>.Failure("The Excel file contains no worksheets.");

        var headerErrors = ValidateHeaders(sheet);
        if (headerErrors.Count > 0)
            return Result<StudentImportPreviewDto>.Failure(headerErrors);

        var headerMap = BuildHeaderMap(sheet);

        var rows = new List<StudentImportRowDto>();
        var errors = new List<StudentImportValidationError>();
        var admissionNumbersInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rollNumbersInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lastRow = sheet.LastRowNum + 1;
        for (int row = 1; row < lastRow; row++)
        {
            var applicationId = GetCellString(sheet, row, headerMap["Application Id Display"]);
            var candidateName = GetCellString(sheet, row, headerMap["Candidate Name"]);
            var gender = GetCellString(sheet, row, headerMap["Gender"]);
            var dob = GetCellString(sheet, row, headerMap["DOB"]);
            var mobileNo = GetCellString(sheet, row, headerMap["Mobile No"]);
            var allottedCategory = GetCellString(sheet, row, headerMap["Allotted Category"]);
            var allottedRound = GetCellString(sheet, row, headerMap["Allotted Round"]);
            var admittedDateTime = GetCellString(sheet, row, headerMap["Admitted Date Time"]);

            if (string.IsNullOrWhiteSpace(applicationId) && string.IsNullOrWhiteSpace(candidateName) &&
                string.IsNullOrWhiteSpace(gender) && string.IsNullOrWhiteSpace(dob) &&
                string.IsNullOrWhiteSpace(mobileNo))
                continue;

            var rowDto = new StudentImportRowDto
            {
                RowNumber = row,
                ApplicationIdDisplay = applicationId,
                CandidateName = candidateName,
                Gender = gender,
                DOB = dob,
                MobileNo = mobileNo,
                AllottedCategory = allottedCategory,
                AllottedRound = allottedRound,
                AdmittedDateTime = admittedDateTime
            };

            if (string.IsNullOrWhiteSpace(applicationId))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Application Id Display", ErrorMessage = "Application Id Display is required." });
            else if (applicationId.Length > 20)
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Application Id Display", ErrorMessage = "Application Id Display must be 20 characters or less." });
            else if (!admissionNumbersInFile.Add(applicationId))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Application Id Display", ErrorMessage = $"Duplicate Application Id Display '{applicationId}' found in file." });

            if (string.IsNullOrWhiteSpace(candidateName))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Candidate Name", ErrorMessage = "Candidate Name is required." });
            else if (candidateName.Length > 100)
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Candidate Name", ErrorMessage = "Candidate Name must be 100 characters or less." });

            if (string.IsNullOrWhiteSpace(gender))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Gender", ErrorMessage = "Gender is required." });
            else
            {
                var genderLower = gender.Trim().ToLowerInvariant();
                if (genderLower == "male") rowDto.ParsedGender = 0;
                else if (genderLower == "female") rowDto.ParsedGender = 1;
                else if (genderLower == "other") rowDto.ParsedGender = 2;
                else errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Gender", ErrorMessage = "Invalid gender. Use Male, Female, or Other." });
            }

            if (string.IsNullOrWhiteSpace(dob))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "DOB", ErrorMessage = "Date of Birth is required." });
            else if (DateTime.TryParseExact(dob, new[] { "dd-MM-yyyy", "dd/MM/yyyy", "MM-dd-yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "dd-MM-yy", "dd/MM/yy" },
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDob))
            {
                if (parsedDob >= DateTime.Today)
                    errors.Add(new StudentImportValidationError { RowNumber = row, Field = "DOB", ErrorMessage = "Date of Birth must be in the past." });
                else if (parsedDob < new DateTime(1950, 1, 1))
                    errors.Add(new StudentImportValidationError { RowNumber = row, Field = "DOB", ErrorMessage = "Date of Birth must be on or after 01/01/1950." });
                else
                    rowDto.ParsedDOB = parsedDob;
            }
            else
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "DOB", ErrorMessage = "Invalid date format for Date of Birth." });

            if (string.IsNullOrWhiteSpace(mobileNo))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Mobile No", ErrorMessage = "Mobile No is required." });
            else if (!System.Text.RegularExpressions.Regex.IsMatch(mobileNo, @"^\d{10}$"))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Mobile No", ErrorMessage = "Mobile No must be exactly 10 digits." });

            if (string.IsNullOrWhiteSpace(allottedCategory))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Allotted Category", ErrorMessage = "Allotted Category is required." });
            else if (allottedCategory.Length > 50)
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Allotted Category", ErrorMessage = "Allotted Category must be 50 characters or less." });

            if (string.IsNullOrWhiteSpace(allottedRound))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Allotted Round", ErrorMessage = "Allotted Round is required." });
            else if (allottedRound.Length > 20)
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Allotted Round", ErrorMessage = "Allotted Round must be 20 characters or less." });

            if (string.IsNullOrWhiteSpace(admittedDateTime))
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Admitted Date Time", ErrorMessage = "Admitted Date Time is required." });
            else if (DateTime.TryParse(admittedDateTime, out var parsedAdmitted))
                rowDto.ParsedAdmittedDateTime = parsedAdmitted;
            else
                errors.Add(new StudentImportValidationError { RowNumber = row, Field = "Admitted Date Time", ErrorMessage = "Invalid date/time format for Admitted Date Time." });

            rows.Add(rowDto);
        }

        if (rows.Count == 0)
            return Result<StudentImportPreviewDto>.Failure("The Excel file contains no data rows.");

        var admissionNumberList = admissionNumbersInFile.ToList();
        var existingStudents = await _context.Students
            .IgnoreQueryFilters()
            .Where(s => s.InstituteId == instituteId.Value &&
                        s.AcademicSessionId == academicSessionId.Value &&
                        admissionNumberList.Contains(s.AdmissionNumber))
            .Select(s => new { s.AdmissionNumber, s.IsDeleted })
            .ToListAsync(ct);

        var activeAdmissionNumbers = existingStudents
            .Where(e => !e.IsDeleted)
            .Select(e => e.AdmissionNumber)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var admNo in activeAdmissionNumbers)
        {
            var affectedRows = rows.Where(r => r.ApplicationIdDisplay == admNo).ToList();
            foreach (var r in affectedRows)
                errors.Add(new StudentImportValidationError { RowNumber = r.RowNumber, Field = "Application Id Display", ErrorMessage = $"Application Id Display '{admNo}' already exists in this institute/session." });
        }

        var errorRowNumbers = errors.Select(e => e.RowNumber).ToHashSet();
        var validRows = rows.Where(r => !errorRowNumbers.Contains(r.RowNumber)).ToList();

        var preview = new StudentImportPreviewDto
        {
            TotalRows = rows.Count,
            ValidRows = validRows.Count,
            InvalidRows = errorRowNumbers.Count,
            Rows = rows,
            Errors = errors,
            FileName = fileName,
            TradeId = tradeId,
            BatchId = _tradeAccess.IsTradeHead ? _tradeAccess.EffectiveBatchId : batchId,
            TradeName = trade.Name,
            InstituteName = institute?.Name ?? string.Empty,
            SessionYear = session?.SessionYear ?? string.Empty
        };

        return Result<StudentImportPreviewDto>.Success(preview);
    }

    public async Task<Result<StudentImportResultDto>> ConfirmImportAsync(StudentImportPreviewDto preview, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null)
            return Result<StudentImportResultDto>.Failure("Institute not found.");

        if (academicSessionId is null)
            return Result<StudentImportResultDto>.Failure("Academic Session not found.");

        var tradeAccessResult = await _tradeAccess.ValidateTradeAccessAsync(preview.TradeId, ct);
        if (!tradeAccessResult.IsSuccess)
            return Result<StudentImportResultDto>.Failure(tradeAccessResult.Error!);

        // Override batchId from server-side — never trust client-supplied batchId
        Guid? effectiveBatchId;
        if (_tradeAccess.IsTradeHead)
        {
            effectiveBatchId = _tradeAccess.EffectiveBatchId
                ?? throw new InvalidOperationException("TradeHead must have an assigned batch.");
        }
        else
        {
            effectiveBatchId = preview.BatchId;
            // Validate batch belongs to this institute if provided
            if (effectiveBatchId.HasValue)
            {
                var batchValid = await _context.Batches.AnyAsync(
                    b => b.Id == effectiveBatchId.Value
                        && b.InstituteId == instituteId.Value
                        && !b.IsDeleted, ct);
                if (!batchValid)
                    return Result<StudentImportResultDto>.Failure("Invalid batch for this institute.");
            }
        }

        var trade = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == preview.TradeId && t.InstituteId == instituteId.Value && !t.IsDeleted, ct);
        if (trade is null)
            return Result<StudentImportResultDto>.Failure("Trade not found or does not belong to your institute.");

        var errorRowNumbers = preview.Errors.Select(e => e.RowNumber).ToHashSet();
        var validRows = preview.Rows.Where(r => !errorRowNumbers.Contains(r.RowNumber)).ToList();

        if (validRows.Count == 0)
            return Result<StudentImportResultDto>.Failure("No valid rows to import.");

        var admissionNumbersInFile = validRows
            .Select(r => r.ApplicationIdDisplay.Trim())
            .ToList();

        var existingStudents = await _context.Students
            .IgnoreQueryFilters()
            .Where(s => s.InstituteId == instituteId.Value &&
                        s.AcademicSessionId == academicSessionId.Value &&
                        admissionNumbersInFile.Contains(s.AdmissionNumber))
            .ToListAsync(ct);

        var existingByAdmissionNumber = existingStudents.ToDictionary(s => s.AdmissionNumber, StringComparer.OrdinalIgnoreCase);

        var activeDuplicates = existingByAdmissionNumber
            .Where(kvp => !kvp.Value.IsDeleted)
            .Select(kvp => kvp.Key)
            .ToList();

        if (activeDuplicates.Count > 0)
        {
            var duplicateList = string.Join(", ", activeDuplicates.Take(5));
            var suffix = activeDuplicates.Count > 5 ? $" and {activeDuplicates.Count - 5} more" : "";
            return Result<StudentImportResultDto>.Failure(
                $"Import failed. The following Admission Numbers already exist: {duplicateList}{suffix}. Please re-preview the file to get updated validation.");
        }

        if (trade.TotalSeats > 0)
        {
            var currentStudentCount = await _context.Students
                .CountAsync(s => s.TradeId == preview.TradeId &&
                    s.AcademicSessionId == academicSessionId.Value &&
                    s.Status == StudentStatus.Active, ct);

            if (currentStudentCount + validRows.Count > trade.TotalSeats)
                return Result<StudentImportResultDto>.Failure(
                    $"No vacant seats available. Trade has {trade.TotalSeats} seats, {currentStudentCount} already occupied, and you are trying to import {validRows.Count} students.");
        }

        var students = new List<Student>();
        var restoredCount = 0;
        var now = DateTime.UtcNow;

        foreach (var r in validRows)
        {
            var admissionNumber = r.ApplicationIdDisplay.Trim();

            if (existingByAdmissionNumber.TryGetValue(admissionNumber, out var existing))
            {
                existing.TradeId = preview.TradeId;
                existing.BatchId = effectiveBatchId;
                existing.FirstName = r.CandidateName.Trim();
                existing.LastName = ".";
                existing.DateOfBirth = r.ParsedDOB!.Value;
                existing.Gender = (Gender)r.ParsedGender!.Value;
                existing.Phone = r.MobileNo.Trim();
                existing.CasteCategory = r.AllottedCategory.Trim();
                existing.AllottedRound = r.AllottedRound.Trim();
                existing.AdmissionNumber = admissionNumber;
                existing.RollNumber = admissionNumber;
                existing.AdmissionDate = r.ParsedAdmittedDateTime!.Value;
                existing.Status = StudentStatus.Active;
                existing.StatusReason = "Imported via Excel";
                existing.IsDeleted = false;
                existing.UpdatedAt = now;
                restoredCount++;
            }
            else
            {
                students.Add(new Student
                {
                    InstituteId = instituteId.Value,
                    AcademicSessionId = academicSessionId.Value,
                    TradeId = preview.TradeId,
                    BatchId = effectiveBatchId,
                    FirstName = r.CandidateName.Trim(),
                    LastName = ".",
                    DateOfBirth = r.ParsedDOB!.Value,
                    Gender = (Gender)r.ParsedGender!.Value,
                    Phone = r.MobileNo.Trim(),
                    CasteCategory = r.AllottedCategory.Trim(),
                    AllottedRound = r.AllottedRound.Trim(),
                    AdmissionNumber = admissionNumber,
                    RollNumber = admissionNumber,
                    AdmissionDate = r.ParsedAdmittedDateTime!.Value,
                    AnnualIncome = 0,
                    IsPhysicallyHandicapped = false,
                    Status = StudentStatus.Active,
                    StatusReason = "Imported via Excel",
                    Address = ".",
                    FatherName = ".",
                    MotherName = ".",
                    GuardianPhone = "0000000000",
                    AadharNumber = "000000000000",
                    IsDeleted = false
                });
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.Students.AddRangeAsync(students, ct);
            await _context.SaveChangesAsync(ct);

            var history = new StudentImportHistory
            {
                InstituteId = instituteId.Value,
                TradeId = preview.TradeId,
                AcademicSessionId = academicSessionId.Value,
                FileName = preview.FileName ?? "unknown.xlsx",
                ImportDate = DateTime.UtcNow,
                NumberOfStudentsImported = validRows.Count,
                Status = ImportStatus.Success,
                ValidationErrors = preview.Errors.Count > 0 ? JsonSerializer.Serialize(preview.Errors) : null
            };
            await _context.StudentImportHistories.AddAsync(history, ct);
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            await transaction.RollbackAsync(ct);
            return Result<StudentImportResultDto>.Failure(
                "Import failed due to duplicate Admission Numbers. Some students may have already been imported. Please check the Students list and re-preview the file.");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<StudentImportResultDto>.Success(new StudentImportResultDto
        {
            Success = true,
            StudentsImported = validRows.Count,
            Message = restoredCount > 0
                ? $"{students.Count} student(s) imported and {restoredCount} previously deleted student(s) restored successfully."
                : $"{validRows.Count} student(s) imported successfully."
        });
    }

    public Task<Result<byte[]>> DownloadTemplateAsync(CancellationToken ct)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("StudentImport");

        for (int i = 0; i < RequiredHeaders.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = RequiredHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        worksheet.Column(1).Width = 22;
        worksheet.Column(2).Width = 28;
        worksheet.Column(3).Width = 12;
        worksheet.Column(4).Width = 16;
        worksheet.Column(5).Width = 16;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 18;
        worksheet.Column(8).Width = 22;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(Result<byte[]>.Success(stream.ToArray()));
    }

    public async Task<Result<PaginatedList<StudentImportHistoryDto>>> GetImportHistoryAsync(PaginationRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null)
            return Result<PaginatedList<StudentImportHistoryDto>>.Failure("Institute not found.");

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<StudentImportHistory> query = _context.StudentImportHistories
            .AsNoTracking()
            .Include(h => h.Trade)
            .OrderByDescending(h => h.ImportDate);

        if (!isSuperAdmin)
            query = query.Where(h => h.InstituteId == instituteId!.Value);

        var paginatedList = await PaginatedList<StudentImportHistoryDto>.CreateAsync(
            query.Select(h => new StudentImportHistoryDto
            {
                Id = h.Id,
                FileName = h.FileName,
                ImportedByName = h.CreatedBy.HasValue
                    ? _context.Users.Where(u => u.Id == h.CreatedBy.Value).Select(u => u.Username).FirstOrDefault() ?? "Unknown"
                    : "System",
                ImportDate = h.ImportDate,
                NumberOfStudentsImported = h.NumberOfStudentsImported,
                Status = h.Status,
                ValidationErrors = h.ValidationErrors,
                TradeName = h.Trade.Name
            }),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<StudentImportHistoryDto>>.Success(paginatedList);
    }

    private static List<string> ValidateHeaders(ISheet sheet)
    {
        var errors = new List<string>();
        var firstRow = sheet.GetRow(0);
        if (firstRow is null)
        {
            errors.Add("The Excel file has no header row.");
            return errors;
        }

        var actualHeaders = new List<string>();
        for (int col = 0; col <= firstRow.LastCellNum; col++)
        {
            var cell = firstRow.GetCell(col);
            if (cell != null)
                actualHeaders.Add(cell.ToString().Trim());
        }

        foreach (var header in RequiredHeaders)
        {
            if (!actualHeaders.Any(h => string.Equals(h, header, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"Missing required column: '{header}'.");
        }

        return errors;
    }

    private static Dictionary<string, int> BuildHeaderMap(ISheet sheet)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var firstRow = sheet.GetRow(0);
        if (firstRow is null) return map;

        for (int col = 0; col <= firstRow.LastCellNum; col++)
        {
            var cell = firstRow.GetCell(col);
            if (cell != null)
            {
                var header = cell.ToString().Trim();
                if (!string.IsNullOrEmpty(header))
                    map[header] = col;
            }
        }

        return map;
    }

    private static string GetCellString(ISheet sheet, int row, int col)
    {
        var sheetRow = sheet.GetRow(row);
        if (sheetRow is null) return string.Empty;
        var cell = sheetRow.GetCell(col);
        if (cell is null) return string.Empty;

        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue.Trim(),
            CellType.Numeric => NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(cell)
                ? DateTime.FromOADate(cell.NumericCellValue).ToString("dd-MM-yyyy")
                : cell.NumericCellValue.ToString(),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            CellType.Formula => cell.CachedFormulaResultType == CellType.String
                ? cell.StringCellValue.Trim()
                : NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(cell)
                    ? DateTime.FromOADate(cell.NumericCellValue).ToString("dd-MM-yyyy")
                    : cell.NumericCellValue.ToString(),
            _ => string.Empty
        };
    }
}
