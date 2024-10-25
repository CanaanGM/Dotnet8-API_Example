// Ignore Spelling: Todo sqlite

using Core.Models;

using Infrastructure.DatabaseContexts;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface ITodoService
{
    /// <summary>
    /// gets all the todos in the database
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>either an empty collection or a filled one with todos</returns>
    Task<ICollection<ToDo>> GetAllAsync(CancellationToken cancellationToken);
}

public class TodoService : ITodoService
{
    private readonly SqliteContext _sqliteContext;

    public TodoService(SqliteContext sqliteContext)
    {
        _sqliteContext = sqliteContext;
    }
    public async Task<ICollection<ToDo>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _sqliteContext.ToDoes.ToListAsync(cancellationToken);
    }
}
