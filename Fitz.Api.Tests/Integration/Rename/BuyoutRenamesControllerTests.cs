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
    public class BuyoutRenamesControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public BuyoutRenamesControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("BuyoutRenamesTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task BuyoutRenames_WithValidUserId_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var rename1 = new RenameModel
            {
                NewName = "TestName1",
                AffectedUserId = 111111111,
                RequestedUserId = 222222222,
                Days = 5,
                Cost = 100,
                Status = RenameStatus.Active,
                Timestamp = DateTime.UtcNow
            };

            var rename2 = new RenameModel
            {
                NewName = "TestName2",
                AffectedUserId = 111111111,
                RequestedUserId = 333333333,
                Days = 3,
                Cost = 80,
                Status = RenameStatus.Pending,
                Timestamp = DateTime.UtcNow
            };
            
            db.Renames.Add(rename1);
            db.Renames.Add(rename2);
            await db.SaveChangesAsync();

            var response = await _client.PostAsync("/api/rename/user/111111111/buyout", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
