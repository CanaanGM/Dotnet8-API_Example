using Infrastructure.Services;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

public class ToDosController : CustomBaseController
{
    private readonly ITodoService _todoService;
    public ToDosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    /// <summary>
    /// Gets all the ToDos in the database, irrespective of the user or status
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    [HttpGet]
    
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await _todoService.GetAllAsync(cancellationToken));
}
