// Ignore Spelling: Todos todo

using Infrastructure.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomBaseController : ControllerBase
{
    private readonly ITodoService _todoService;

    public CustomBaseController(ITodoService todoService)
    {
        _todoService = todoService;
    }


    [HttpGet("todos")]
    public async Task<IActionResult> GetAllTodosAsync(CancellationToken cancellationToken)
    {
        return Ok(await _todoService.GetAllAsync(cancellationToken));
    }

}
