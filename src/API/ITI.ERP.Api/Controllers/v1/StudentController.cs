using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Student;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/students")]
[ApiVersion("1.0")]
[Authorize]
public class StudentController : BaseApiController
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Student.View)]
    [ProducesResponseType(typeof(PaginatedList<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetStudentsAsync(
            new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm },
            cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("archived")]
    [Authorize(Policy = Permissions.Student.View)]
    [ProducesResponseType(typeof(PaginatedList<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArchivedStudents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetArchivedStudentsAsync(
            new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm },
            cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.Student.View)]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _studentService.GetStudentByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Student.Create)]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentService.CreateStudentAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.Student.Edit)]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentService.UpdateStudentAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/transfer")]
    [Authorize(Policy = Permissions.Student.Transfer)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransferStudent(Guid id, [FromBody] TransferStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentService.TransferStudentAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/batch")]
    [Authorize(Policy = Permissions.Student.Transfer)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeBatch(Guid id, [FromBody] ChangeBatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentService.ChangeBatchAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = Permissions.Student.Edit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStudentStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentService.ChangeStatusAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/archive")]
    [Authorize(Policy = Permissions.Student.Archive)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ArchiveStudent(Guid id, [FromQuery] string reason, CancellationToken cancellationToken)
    {
        var result = await _studentService.ArchiveStudentAsync(id, reason, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Student.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudent(Guid id, [FromBody] DeleteStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentService.DeleteStudentAsync(id, request.Reason, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id}/unarchive")]
    [Authorize(Policy = Permissions.Student.Archive)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnarchiveStudent(Guid id, [FromBody] UnarchiveStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentService.UnarchiveStudentAsync(id, request.Reason, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id}/photo")]
    [Authorize(Policy = Permissions.Student.Photo)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await _studentService.UploadPhotoAsync(id, stream, file.FileName, cancellationToken);
        return HandleResult(result);
    }
}
