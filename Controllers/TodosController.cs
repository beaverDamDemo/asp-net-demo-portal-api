using AspNetDemoPortalAPI.Data;
using AspNetDemoPortalAPI.Dto;
using AspNetDemoPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetDemoPortalAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly DemoPortalContext _context;
        public TodosController(DemoPortalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUserTodos()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var todos = _context.Todos
                .Where(t => t.UserId == userId)
                .Select(t => new TodoDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Completed = t.Completed,
                    CreatedAt = t.CreatedAt
                })
                .ToList();

            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodoById(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var todo = await _context.Todos.FindAsync(id);

            if (todo == null || todo.UserId != userId)
            {
                return NotFound();
            }

            return Ok(todo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo(CreateTodoDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var todo = new Todo
            {
                Name = dto.Name,
                Description = dto.Description,
                Completed = false, // ensure default if needed
                UserId = userId
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            // Return 201 Created with the new todo in the body
            // and a Location header pointing to the resource
            return CreatedAtAction(
                nameof(GetTodoById),
                new { id = todo.Id },
                todo
            );
        }

        [HttpPut("{id}/completed")]
        public async Task<IActionResult> UpdateTodoCompleted(int id, UpdateTodoCompletedDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null || todo.UserId != userId)
                return NotFound();

            todo.Completed = dto.Completed;

            await _context.SaveChangesAsync();
            return Ok(todo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var todo = await _context.Todos.FindAsync(id);

            if (todo == null || todo.UserId != userId)
                return NotFound();

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
