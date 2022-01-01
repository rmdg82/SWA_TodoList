using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Api.Repositories;
using AutoMapper;
using Api.Models;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using Shared.Dtos;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Api
{
    public class WebApi
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ILogger<WebApi> _logger;
        private readonly IMapper _mapper;

        public WebApi(ITodoRepository todoRepository, ILogger<WebApi> logger, IMapper mapper)
        {
            _todoRepository = todoRepository ?? throw new ArgumentNullException(nameof(todoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [FunctionName("InitializeDb")]
        public async Task<IActionResult> InitializeDb([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "initDb")] HttpRequest req)
        {
            _logger.LogInformation("Initialize Db with fake data.");
            var result = await _todoRepository.InitializeCosmosDbDataIfEmpty();

            if (result)
            {
                return new OkResult();
            }

            return new NoContentResult();
        }

        [FunctionName("ResetDb")]
        public async Task<IActionResult> ResetDb([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resetDb")] HttpRequest req)
        {
            _logger.LogInformation("Reset Db with fake data.");
            await _todoRepository.ResetDb();

            return new OkResult();
        }

        [FunctionName("GetAllTodos")]
        public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodos([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] HttpRequest req)
        {
            var queryParams = req.QueryString;
            var getOnlyUncompleted = req.Query["onlyUncompleted"];
            _logger.LogInformation($"New request for {nameof(GetTodos)} with querystring [{queryParams}].");

            var todos = new List<Todo>();
            try
            {
                bool onlyUncompleted = Convert.ToBoolean(getOnlyUncompleted);

                if (onlyUncompleted)
                {
                    todos = (await _todoRepository.GetByQueryAsync(getOnlyUncompleted: true)).ToList();
                }
                else
                {
                    todos = (await _todoRepository.GetByQueryAsync()).ToList();
                }
            }
            catch (FormatException ex)
            {
                todos = (await _todoRepository.GetByQueryAsync()).ToList();
                _logger.LogInformation(ex.Message);
            }

            return new OkObjectResult(_mapper.Map<IEnumerable<TodoDto>>(todos));
        }

        [FunctionName("GetTodosById")]
        public async Task<ActionResult<Todo>> GetTodoById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{todoId}")] HttpRequest req, string todoId)
        {
            _logger.LogInformation($"New request for {nameof(GetTodoById)} with id [{todoId}].");

            var todo = await _todoRepository.GetByIdAsync(todoId);

            if (todo is null)
            {
                _logger.LogError($"Not found todo for {nameof(GetTodoById)} with id [{todoId}].");
                return new NotFoundResult();
            }

            return new OkObjectResult(_mapper.Map<TodoDto>(todo));
        }

        [FunctionName("AddTodo")]
        public async Task<IActionResult> AddTodo([HttpTrigger(AuthorizationLevel.Function, "post", Route = "todos")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"New request for {nameof(AddTodo)} with body [{requestBody}].");
            var todoToAddDto = JsonSerializer.Deserialize<TodoDtoToAdd>(requestBody);
            var todoToAdd = _mapper.Map<Todo>(todoToAddDto);
            todoToAdd.Id = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation($"Added Todo with id: {todoToAdd.Id} text: {todoToAdd.Text}");
                await _todoRepository.AddAsync(todoToAdd);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new BadRequestResult();
            }

            return new CreatedAtRouteResult(new { todoId = todoToAdd.Id }, todoToAdd);
        }

        [FunctionName("ToggleCompletion")]
        public async Task<IActionResult> ToggleCompletion([HttpTrigger(AuthorizationLevel.Function, "post", Route = "todos/{todoId}/toggle")] HttpRequest req, string todoId)
        {
            try
            {
                _logger.LogInformation($"Toggle completion for todo with id [{todoId}]");
                await _todoRepository.ToggleCompletionAsync(todoId);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkResult();
        }

        [FunctionName("DeleteTodo")]
        public async Task<IActionResult> DeleteTodo([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "todos/{todoId}")] HttpRequest req, string todoId)
        {
            _logger.LogInformation($"New request for {nameof(DeleteTodo)} with id [{todoId}].");

            try
            {
                await _todoRepository.DeleteAsync(todoId);
            }
            catch (CosmosException ex) when (ex.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new BadRequestResult();
            }

            return new NoContentResult();
        }

        [FunctionName("UpdateTodo")]
        public async Task<IActionResult> UpdateTodo([HttpTrigger(AuthorizationLevel.Function, "put", Route = "todos/{todoId}")] HttpRequest req, string todoId)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"New request for {nameof(UpdateTodo)} with id [{todoId}] and body [{requestBody}].");

            var todoToUpdateDto = JsonSerializer.Deserialize<TodoDtoToUpdate>(requestBody);
            var todoToAdd = _mapper.Map<Todo>(todoToUpdateDto);
            todoToAdd.Id = todoId;

            try
            {
                await _todoRepository.UpdateAsync(todoId, todoToAdd);
            }
            catch (CosmosException ex) when (ex.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new NotFoundResult();
            }
            catch (CosmosException ex) when (ex.StatusCode.Equals(HttpStatusCode.BadRequest))
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new BadRequestResult();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Catched exception: [{ex.Message}]");
                return new BadRequestResult();
            }

            return new NoContentResult();
        }
    }
}