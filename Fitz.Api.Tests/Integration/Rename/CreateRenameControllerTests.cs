using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using AccountModel = Fitz.Features.Accounts.Models.Account;
using RenameModel = Fitz.Features.Rename.Models.Renames;

namespace Fitz.Api.Tests.Integration.Rename
{
    public class CreateRenameControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public CreateRenameControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("CreateRenameTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task CreateRename_WithValidRequest_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var affectedUser = new AccountModel
            {
                Id = 111111111,
                Username = "AffectedUser",
                Beer = 100,
                LifetimeBeer = 500,
                Favorability = 50,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };

            var requestedUser = new AccountModel
            {
                Id = 222222222,
                Username = "RequestedUser",
                Beer = 1000,
                LifetimeBeer = 2000,
                Favorability = 50,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            db.Accounts.Add(affectedUser);
            db.Accounts.Add(requestedUser);
            await db.SaveChangesAsync();

            var request = new
            {
                NewName = "NewName",
                AffectedUserId = 111111111UL,
                RequestedUserId = 222222222UL,
                Days = 5
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/rename", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateRename_WithInvalidUserId_ReturnsNotFound()
        {
            var request = new
            {
                NewName = "NewName",
                AffectedUserId = 999999999UL,
                RequestedUserId = 888888888UL,
                Days = 5
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/rename", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
