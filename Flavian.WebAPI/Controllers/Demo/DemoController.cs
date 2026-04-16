using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Flavian.Application.Dto.Request.Demos;
using Flavian.Application.Services.Interfaces.Demos;

namespace Flavian.WebAPI.Controllers.Demo;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/demo")]
public class DemoController(IDemoService service) : ControllerBase
{
    private readonly IDemoService _service = service;

    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        var response = await _service.GetAllAsync(pageNumber, pageSize);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _service.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] DemoRequest request)
    {
        var response = await _service.AddAsync(request);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid id, [FromBody] DemoRequest request)
    {
        var response = await _service.UpdateAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _service.DeleteAsync(id);
        return Ok(response);
    }
}
