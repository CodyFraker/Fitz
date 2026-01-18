using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;
using AccountModel = Fitz.Features.Accounts.Models.Account;

namespace Fitz.Api.Tests.Integration.Users
{
    public class GetUsersControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetUsersControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BotContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<BotContext>(options =>
                    {
                        options.UseInMemoryDatabase("GetUsersTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetUsers_WithValidAuth_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var account1 = new AccountModel
            {
                Id = 111111111,
                Username = "UserOne",
                Beer = 100,
                LifetimeBeer = 500,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            var account2 = new AccountModel
            {
                Id = 222222222,
                Username = "UserTwo",
                Beer = 200,
                LifetimeBeer = 600,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            db.Accounts.Add(account1);
            db.Accounts.Add(account2);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.UsersResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(2, apiResponse.Data.Users.Count);
        }

        [Fact]
        public async Task GetUsers_WithoutAuth_ReturnsUnauthorized()
        {
            var clientWithoutAuth = _factory.CreateClient();
            var response = await clientWithoutAuth.GetAsync("/api/users");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WithPagination_ReturnsCorrectPage()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            for (int i = 1; i <= 10; i++)
            {
                var account = new AccountModel
                {
                    Id = (ulong)(100000000 + i),
                    Username = $"User{i:D2}",
                    Beer = 100,
                    LifetimeBeer = 500,
                    CreatedDate = DateTime.UtcNow,
                    LastSeenDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow
                };
                db.Accounts.Add(account);
            }
            
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/users?page=2&pageSize=3");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.UsersResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(3, apiResponse.Data.Users.Count);
            Assert.Equal(2, apiResponse.Data.Page);
            Assert.Equal(3, apiResponse.Data.PageSize);
            Assert.Equal(10, apiResponse.Data.TotalCount);
            Assert.Equal(4, apiResponse.Data.TotalPages);
        }

        [Fact]
        public async Task GetUsers_WithQuery_ReturnsFilteredResults()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var account1 = new AccountModel
            {
                Id = 111111111,
                Username = "Alice",
                Beer = 100,
                LifetimeBeer = 500,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            var account2 = new AccountModel
            {
                Id = 222222222,
                Username = "Bob",
                Beer = 200,
                LifetimeBeer = 600,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            var account3 = new AccountModel
            {
                Id = 333333333,
                Username = "AliceSmith",
                Beer = 300,
                LifetimeBeer = 700,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            db.Accounts.Add(account1);
            db.Accounts.Add(account2);
            db.Accounts.Add(account3);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/users?query=Alice");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.UsersResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(2, apiResponse.Data.Users.Count);
            Assert.Equal(2, apiResponse.Data.TotalCount);
            Assert.All(apiResponse.Data.Users, u => Assert.Contains("Alice", u.Username, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetUsers_WithQueryAndPagination_ReturnsCorrectResults()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            for (int i = 1; i <= 10; i++)
            {
                var account = new AccountModel
                {
                    Id = (ulong)(100000000 + i),
                    Username = i <= 5 ? $"TestUser{i}" : $"OtherUser{i}",
                    Beer = 100,
                    LifetimeBeer = 500,
                    CreatedDate = DateTime.UtcNow,
                    LastSeenDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow
                };
                db.Accounts.Add(account);
            }
            
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/users?query=Test&page=2&pageSize=2");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.UsersResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(2, apiResponse.Data.Users.Count);
            Assert.Equal(2, apiResponse.Data.Page);
            Assert.Equal(2, apiResponse.Data.PageSize);
            Assert.Equal(5, apiResponse.Data.TotalCount);
            Assert.Equal(3, apiResponse.Data.TotalPages);
            Assert.All(apiResponse.Data.Users, u => Assert.Contains("Test", u.Username, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetUsers_WithInvalidPage_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("/api/users?page=0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WithInvalidPageSize_ReturnsBadRequest()
        {
            var response1 = await _client.GetAsync("/api/users?pageSize=0");
            var response2 = await _client.GetAsync("/api/users?pageSize=101");

            Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }
    }
}
