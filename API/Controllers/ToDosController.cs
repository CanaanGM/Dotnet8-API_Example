using Infrastructure.Services;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ToDosController : CustomBaseController
{
    private readonly ITodoService _todoService;
    public ToDosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await _todoService.GetAllAsync(cancellationToken));
}
