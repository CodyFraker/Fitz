using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Xunit;
using RenameModel = Fitz.Features.Rename.Models.Renames;

namespace Fitz.Api.Tests.Integration.Rename
{
    public class GetRenamesControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetRenamesControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("GetRenamesTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetRenames_WithoutFilter_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var rename = new RenameModel
            {
                NewName = "TestName",
                AffectedUserId = 111111111,
                RequestedUserId = 222222222,
                Days = 5,
                Cost = 100,
                Status = RenameStatusEnum.Active,
                Timestamp = DateTime.UtcNow
            };
            
            db.Renames.Add(rename);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/rename");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetRenames_WithStatusFilter_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var rename = new RenameModel
            {
                NewName = "TestName",
                AffectedUserId = 111111111,
                RequestedUserId = 222222222,
                Days = 5,
                Cost = 100,
                Status = RenameStatusEnum.Active,
                Timestamp = DateTime.UtcNow
            };
            
            db.Renames.Add(rename);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/rename?status=Active");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
