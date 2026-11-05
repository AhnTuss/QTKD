using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/pricelists")]
public class PriceListsController : ControllerBase
{
    private readonly IPriceListService _priceLists;

    public PriceListsController(IPriceListService priceLists)
    {
        _priceLists = priceLists;
    }

    /// <summary>Danh sách bảng giá và các hạng mục.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PriceListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PriceListDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _priceLists.GetAllAsync(ct));
    }

    /// <summary>Chi tiết một bảng giá.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PriceListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceListDto>> GetById(int id, CancellationToken ct)
    {
        var priceList = await _priceLists.GetByIdAsync(id, ct);
        return priceList is null ? NotFound() : Ok(priceList);
    }

    /// <summary>Tạo bảng giá.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PriceListDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceListDto>> Create(CreatePriceListRequest request, CancellationToken ct)
    {
        var result = await _priceLists.CreateAsync(request, ct);
        var priceList = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id = priceList.Id }, priceList);
    }

    /// <summary>Danh sách hạng mục của một bảng giá.</summary>
    [HttpGet("{id:int}/items")]
    [ProducesResponseType(typeof(List<PriceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PriceItemDto>>> GetItems(int id, CancellationToken ct)
    {
        var items = await _priceLists.GetItemsAsync(id, ct);
        return items is null ? NotFound() : Ok(items);
    }

    /// <summary>Thêm một hạng mục vào bảng giá.</summary>
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(PriceItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PriceItemDto>> AddItem(
        int id,
        CreatePriceItemRequest request,
        CancellationToken ct)
    {
        var result = await _priceLists.AddItemAsync(id, request, ct);
        if (result.Error is not null)
        {
            var problem = new ProblemDetails
            {
                Title = result.Error == PriceOperationError.NotFound
                    ? "The price list does not exist."
                    : "This service code already exists in the price list."
            };

            return new ActionResult<PriceItemDto>(result.Error == PriceOperationError.NotFound
                ? NotFound(problem)
                : Conflict(problem));
        }

        var item = result.Value!;
        return CreatedAtAction(nameof(GetItems), new { id }, item);
    }
}