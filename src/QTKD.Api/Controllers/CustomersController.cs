using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;

    public CustomersController(ICustomerService customers)
    {
        _customers = customers;
    }

    /// <summary>Danh sách khách hàng.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CustomerDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _customers.GetAllAsync(ct));
    }

    /// <summary>Thông tin một khách hàng theo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(id, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    /// <summary>Tạo khách hàng.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request, CancellationToken ct)
    {
        if (!Enum.IsDefined(request.Status))
        {
            return BadRequest(new ProblemDetails { Title = "Customer status is invalid." });
        }

        var customer = await _customers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    /// <summary>Cập nhật tên và trạng thái khách hàng.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> Update(int id, UpdateCustomerRequest request, CancellationToken ct)
    {
        if (!Enum.IsDefined(request.Status))
        {
            return BadRequest(new ProblemDetails { Title = "Customer status is invalid." });
        }

        var customer = await _customers.UpdateAsync(id, request, ct);
        return customer is null ? NotFound() : Ok(customer);
    }
}
