using Api.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Api.MappingProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Api.Models;
using System.IO;
using SharedLibrary.Dtos;
using SharedLibrary;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace Api.Tests
{
    public class WebApiTests
    {
        private const string _existingTodoId = "1";
        private const string _notExistingTodoId = "10";

        private readonly Mock<ITodoRepository> _mockedTodoRepository;
        private readonly ILogger<WebApi> _logger;
        private readonly IMapper _mapper;

        private readonly WebApi _webApi;

        public WebApiTests()
        {
            _mockedTodoRepository = new Mock<ITodoRepository>();
            _logger = new LoggerFactory().CreateLogger<WebApi>();
            var mapperProfile = new TodoProfile();
            var mapperConf = new MapperConfiguration(cfg => cfg.AddProfile(mapperProfile));
            _mapper = new Mapper(mapperConf);

            _webApi = new WebApi(_mockedTodoRepository.Object, _logger, _mapper);

            List<Todo> allTodos = new()
            {
                new Todo
                {
                    Id = "1",
                    IsCompleted = true,
                },
                new Todo
                {
                    Id = "2",
                    IsCompleted = false,
                },
            };

            _mockedTodoRepository.Setup(x => x.GetByQueryAsync(false)).ReturnsAsync(allTodos);
            _mockedTodoRepository.Setup(x => x.GetByQueryAsync(true)).ReturnsAsync(allTodos.Where(x => !x.IsCompleted).ToList());
            _mockedTodoRepository.Setup(x => x.GetByIdAsync(_existingTodoId)).ReturnsAsync(allTodos.FirstOrDefault(x => x.Id.Equals(_existingTodoId)));
            _mockedTodoRepository.Setup(x => x.GetByIdAsync(_notExistingTodoId)).ReturnsAsync(value: null);
            _mockedTodoRepository.Setup(x => x.UpdateAsync(_notExistingTodoId, It.IsAny<string>())).Throws(new CosmosException("", System.Net.HttpStatusCode.NotFound, 0, "", 0));
            _mockedTodoRepository.Setup(x => x.CompleteAsync(_existingTodoId)).Returns(Task.CompletedTask);
            _mockedTodoRepository.Setup(x => x.CompleteAsync(_notExistingTodoId)).Throws(new KeyNotFoundException());
            _mockedTodoRepository.Setup(x => x.DeleteAsync(_existingTodoId)).Returns(Task.CompletedTask);
            _mockedTodoRepository.Setup(x => x.DeleteAsync(_notExistingTodoId)).Throws(new CosmosException("", System.Net.HttpStatusCode.NotFound, 0, "", 0));
        }

        [Fact]
        public async Task ResetDb_WhenCalled_CallService()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Post);
            IActionResult actionResult = await _webApi.ResetDb(request);

            actionResult.Should().BeAssignableTo<OkResult>();
            _mockedTodoRepository.Verify(x => x.ResetDb(), Times.Once());
        }

        [Fact]
        public async Task GetTodos_WhenCalledWithNoQueryString_ReturnAllTodos()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

            OkObjectResult? result = (await _webApi.GetTodos(request)).Result as OkObjectResult;

            result.Should().NotBeNull();
            var todos = result!.Value as IEnumerable<TodoDto>;
            todos.Should().NotBeNull();
            todos.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTodos_WhenCalledWithQueryString_ReturnOnlyUncompleted()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: new QueryString("?onlyUncompleted=true"), body: null);

            OkObjectResult? result = (await _webApi.GetTodos(request))!.Result as OkObjectResult;

            result.Should().NotBeNull();
            var todos = result!.Value as IEnumerable<TodoDto>;
            todos.Should().NotBeNull();
            todos.Should().HaveCount(1);
            todos!.Single().IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetTodos_WhenCalledWithWrongQueryString_ReturnsAllTodos()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: new QueryString("?onlyUncompleted=asdasd"), body: null);

            OkObjectResult? result = (await _webApi.GetTodos(request))!.Result as OkObjectResult;

            result.Should().NotBeNull();
            var todos = result!.Value as IEnumerable<TodoDto>;
            todos.Should().NotBeNull().And.HaveCount(2);
        }

        [Fact]
        public async Task GetTodoById_WhenCalledWithExistingId_ReturnsTodo()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

            OkObjectResult? result = (await _webApi.GetTodoById(request, _existingTodoId)).Result as OkObjectResult;

            result.Should().NotBeNull();
            var todo = result!.Value as TodoDto;
            todo.Should().NotBeNull();
            todo!.Id.Should().Be(_existingTodoId);
            todo!.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task GetTodoById_WhenCalledWithNotExistingId_ReturnsNotFound()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

            var result = await _webApi.GetTodoById(request, _notExistingTodoId);

            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ActionResult<TodoDto>>().Which.Result.Should().BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task AddTodo_TodoTextTooLong_ReturnBadRequest()
        {
            TodoDtoToAdd todoDtoToAdd = new(new string('*', ValidationConstants.maxLengthOnAdd + 1));
            HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

            IActionResult result = await _webApi.AddTodo(request);

            result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddTodo_TodoTextNotTooLong_ReturnCreatedAtRoute()
        {
            TodoDtoToAdd todoDtoToAdd = new(new string('*', ValidationConstants.maxLengthOnAdd));
            HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

            IActionResult result = await _webApi.AddTodo(request);

            result.Should().NotBeNull().And.BeAssignableTo<CreatedAtRouteResult>();
            _mockedTodoRepository.Verify(x => x.AddAsync(It.IsAny<Todo>()), Times.Once());
        }

        [Fact]
        public async Task AddTodo_RepositoryThrowException_ReturnBadRequest()
        {
            TodoDtoToAdd todoDtoToAdd = new(new string('*', ValidationConstants.maxLengthOnAdd));
            HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));
            _mockedTodoRepository.Setup(x => x.AddAsync(It.IsAny<Todo>())).Throws<Exception>();

            IActionResult result = await _webApi.AddTodo(request);

            result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
            _mockedTodoRepository.Verify(x => x.AddAsync(It.IsAny<Todo>()), Times.Once());
        }

        [Fact]
        public async Task UpdateTodo_TodoTextTooLong_ReturnBadRequest()
        {
            TodoDtoToUpdate todoDtoToUpdate = new(new string('*', ValidationConstants.maxLengthOnAdd + 1));
            HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

            IActionResult result = await _webApi.UpdateTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateTodo_TodoTextNotTooLong_ReturnNoContent()
        {
            TodoDtoToUpdate todoDtoToUpdate = new(new string('*', ValidationConstants.maxLengthOnAdd));
            HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

            IActionResult result = await _webApi.UpdateTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<NoContentResult>();
            _mockedTodoRepository.Verify(x => x.UpdateAsync(_existingTodoId, todoDtoToUpdate.Text), Times.Once());
        }

        [Fact]
        public async Task UpdateTodo_CosmosExceptionNotFound_ReturnNotFoundResult()
        {
            TodoDtoToUpdate todoDtoToUpdate = new(new string('*', ValidationConstants.maxLengthOnAdd));
            HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

            IActionResult result = await _webApi.UpdateTodo(request, _notExistingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
            _mockedTodoRepository.Verify(x => x.UpdateAsync(_notExistingTodoId, todoDtoToUpdate.Text), Times.Once());
        }

        [Fact]
        public async Task UpdateTodo_CosmosExceptionBadRequest_ReturnBadRequestResult()
        {
            TodoDtoToUpdate todoDtoToUpdate = new(new string('*', ValidationConstants.maxLengthOnAdd));
            HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));
            _mockedTodoRepository.Setup(x => x.UpdateAsync(_existingTodoId, It.IsAny<string>())).Throws(new CosmosException("", System.Net.HttpStatusCode.BadRequest, 0, "", 0));

            IActionResult result = await _webApi.UpdateTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
            _mockedTodoRepository.Verify(x => x.UpdateAsync(_existingTodoId, todoDtoToUpdate.Text), Times.Once());
        }

        [Fact]
        public async Task UpdateTodo_KeyNotFoundException_ReturnNotFoundObjectResult()
        {
            TodoDtoToUpdate todoDtoToUpdate = new(new string('*', ValidationConstants.maxLengthOnAdd));
            HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));
            _mockedTodoRepository.Setup(x => x.UpdateAsync(_existingTodoId, It.IsAny<string>())).Throws(new KeyNotFoundException());

            IActionResult result = await _webApi.UpdateTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<NotFoundObjectResult>();
            _mockedTodoRepository.Verify(x => x.UpdateAsync(_existingTodoId, todoDtoToUpdate.Text), Times.Once());
        }

        [Fact]
        public async Task UpdateTodo_GenericException_ReturnBadRequestObjectResult()
        {
            TodoDtoToUpdate todoDtoToUpdate = new(new string('*', ValidationConstants.maxLengthOnAdd));
            HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));
            _mockedTodoRepository.Setup(x => x.UpdateAsync(_existingTodoId, It.IsAny<string>())).Throws(new Exception());

            IActionResult result = await _webApi.UpdateTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>();
            _mockedTodoRepository.Verify(x => x.UpdateAsync(_existingTodoId, todoDtoToUpdate.Text), Times.Once());
        }

        [Fact]
        public async Task CompleteTodo_WhenCalledOnExistingId_ReturnOkResult()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

            IActionResult result = await _webApi.CompleteTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<OkResult>();
            _mockedTodoRepository.Verify(x => x.CompleteAsync(_existingTodoId), Times.Once());
        }

        [Fact]
        public async Task CompleteTodo_WhenCalledOnNotExistingId_ReturnNotFoundResult()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

            IActionResult result = await _webApi.CompleteTodo(request, _notExistingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
            _mockedTodoRepository.Verify(x => x.CompleteAsync(_notExistingTodoId), Times.Once());
        }

        [Fact]
        public async Task CompleteTodo_GenericException_ReturnBadRequestObjectResult()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
            _mockedTodoRepository.Setup(x => x.CompleteAsync(_existingTodoId)).Throws(new Exception());

            IActionResult result = await _webApi.CompleteTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>();
            _mockedTodoRepository.Verify(x => x.CompleteAsync(_existingTodoId), Times.Once());
        }

        [Fact]
        public async Task DeleteTodo_WhenCalledOnExistingId_ReturnNoContentResult()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Delete, queryStrings: null, body: null);

            IActionResult result = await _webApi.DeleteTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<NoContentResult>();
            _mockedTodoRepository.Verify(x => x.DeleteAsync(_existingTodoId), Times.Once());
        }

        [Fact]
        public async Task DeleteTodo_WhenCalledOnNotExistingId_ReturnNotFoundResult()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

            IActionResult result = await _webApi.DeleteTodo(request, _notExistingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
            _mockedTodoRepository.Verify(x => x.DeleteAsync(_notExistingTodoId), Times.Once());
        }

        [Fact]
        public async Task DeleteTodo_GenericException_ReturnBadRequestResult()
        {
            HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
            _mockedTodoRepository.Setup(x => x.DeleteAsync(_existingTodoId)).Throws(new Exception());

            IActionResult result = await _webApi.DeleteTodo(request, _existingTodoId);

            result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
            _mockedTodoRepository.Verify(x => x.DeleteAsync(_existingTodoId), Times.Once());
        }

        [Fact]
        public void TestToFail()
        {
            Assert.True(false);
        }

        private static HttpRequest CreateHttpRequest(string method, QueryString? queryStrings = null, string? body = null)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Method = method;
            request.QueryString = queryStrings ?? QueryString.Empty;
            if (body != null)
            {
                var bytes = Encoding.UTF8.GetBytes(body);
                request.Body = new MemoryStream(bytes);
            }
            return request;
        }
    }
}