using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contracts;

    public ContractsController(IContractService contracts)
    {
        _contracts = contracts;
    }

    /// <summary>Chi tiết một hợp đồng theo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDto>> GetById(int id, CancellationToken ct)
    {
        var contract = await _contracts.GetByIdAsync(id, ct);
        return contract is null ? NotFound() : Ok(contract);
    }

    /// <summary>Danh sách hợp đồng.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ContractDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ContractDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _contracts.GetAllAsync(ct));
    }

    /// <summary>Tạo hợp đồng ở trạng thái Draft.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContractDto>> Create(CreateContractRequest request, CancellationToken ct)
    {
        var result = await _contracts.CreateAsync(request, ct);
        if (result.Error is not null)
        {
            return new ActionResult<ContractDto>(ToError(result.Error.Value));
        }

        var contract = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
    }

    /// <summary>Cập nhật hợp đồng khi đang Draft hoặc RevisionRequested.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContractDto>> Update(int id, UpdateContractRequest request, CancellationToken ct)
    {
        var result = await _contracts.UpdateAsync(id, request, ct);
        if (result.Error is not null)
        {
            return new ActionResult<ContractDto>(ToError(result.Error.Value));
        }

        return Ok(result.Value);
    }

    /// <summary>Thêm một nội dung/tài liệu tham chiếu cho hợp đồng.</summary>
    [HttpPost("{id:int}/contents")]
    [ProducesResponseType(typeof(ContractContentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContractContentDto>> AddContent(
        int id,
        CreateContractContentRequest request,
        CancellationToken ct)
    {
        var result = await _contracts.AddContentAsync(id, request, ct);
        if (result.Error is not null)
        {
            return new ActionResult<ContractContentDto>(ToError(result.Error.Value));
        }

        var content = result.Value!;
        return CreatedAtAction(nameof(GetContentById), new { id, contentId = content.Id }, content);
    }

    /// <summary>Chi tiết một nội dung/tài liệu của hợp đồng.</summary>
    [HttpGet("{id:int}/contents/{contentId:int}")]
    [ProducesResponseType(typeof(ContractContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractContentDto>> GetContentById(
        int id,
        int contentId,
        CancellationToken ct)
    {
        var content = await _contracts.GetContentByIdAsync(id, contentId, ct);
        return content is null ? NotFound() : Ok(content);
    }

    /// <summary>Gửi hợp đồng Draft để trình duyệt.</summary>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContractDto>> Submit(int id, CancellationToken ct)
    {
        var result = await _contracts.SubmitAsync(id, ct);
        if (result.Error is not null)
        {
            return new ActionResult<ContractDto>(ToError(result.Error.Value));
        }

        return Ok(result.Value);
    }

    private ActionResult ToError(ContractOperationError error)
    {
        var title = error switch
        {
            ContractOperationError.InvalidStatus => "Contract cannot be changed in its current status.",
            ContractOperationError.CustomerNotFound => "The customer does not exist.",
            ContractOperationError.CustomerInactive => "The customer is not active.",
            ContractOperationError.DuplicateContractNo => "The contract number is already in use.",
            ContractOperationError.InvalidDateRange => "The contract date range is invalid.",
            ContractOperationError.MissingRequiredContent => "Add at least one content item before submitting.",
            _ => "The contract was not found."
        };

        var problem = new ProblemDetails { Title = title };
        return error switch
        {
            ContractOperationError.NotFound => NotFound(problem),
            ContractOperationError.InvalidStatus or ContractOperationError.DuplicateContractNo => Conflict(problem),
            _ => BadRequest(problem)
        };
    }
}
