using Microsoft.EntityFrameworkCore;
using TodoAPI.Models;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.IncludeFields = true;
});


builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<TodoDB>(opt => opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
app.MapGet("/", () => Results.Json(new Todo
{
    Name = "Walk dog",
    IsComplete = false
}, options));

app.MapGet("/todoitems", async (TodoDB db) =>
 await db.Todos.Select(x => new TodoItemDTO(x)).ToListAsync());
app.MapGet("/todoitems/{id}", async (int id, TodoDB db) =>
 await db.Todos.FindAsync(id)
 is Todo todo
 ? Results.Ok(new TodoItemDTO(todo))
 : Results.NotFound());
app.MapPost("/todoitems", async (TodoItemDTO todoItemDTO, TodoDB db) =>
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
});
app.MapPut("/todoitems/{id}", async (int id, TodoItemDTO todoItemDTO, TodoDB db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/todoitems/{id}", async (int id, TodoDB db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(new TodoItemDTO(todo));
    }
    return Results.NotFound();
});
app.Run();