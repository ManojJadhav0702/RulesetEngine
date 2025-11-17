// ============================================================
// Integration Tests
// Location: /tests/RulesetEngine.IntegrationTests/
// ============================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;
// removed: using Microsoft.VisualStudio.TestPlatform.TestHost;
using RulesetEngine.Data;
using RulesetEngine.Domain.Model;
using System;
using System.Linq; // Add this using directive at the top if not present
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RulesetEngine.IntegrationTests
{

    // Explicitly reference the application's Program in the global namespace to avoid
    // the TestHost Program type collision.
    public class OrderEvaluationApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public OrderEvaluationApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost:5002") // or your actual server URL
            });
        }


        [Fact]
        public async Task EvaluateOrder_ValidOrder_ReturnsCorrectPlant()
        {
            // Arrange
            var order = new Order
            {
                OrderId = "1245101",
                PublisherNumber = "99990",
                PublisherName = "BookWorld Ltd",
                OrderMethod = "POD",
                Shipments = new List<Shipment>
                {
                    new() { ShipTo = new ShipTo { IsoCountry = "US" } }
                },
                Items = new List<Item>
                {
                    new()
                    {
                        Sku = "PB-001",
                        PrintQuantity = 10,
                        Components = new List<Component>
                        {
                            new()
                            {
                                Code = "Cover",
                                Attributes = new ComponentAttributes { BindTypeCode = "PB" }
                            }
                        }
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/evaluate", order);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<EvaluationResponse>();
            Assert.NotNull(result);
            Assert.True(result.matched);
            Assert.Equal("US", result.productionPlant);
            Assert.Equal("Ruleset One", result.matchedRuleset);
        }

        [Fact]
        public async Task EvaluateOrder_HighQuantity_ReturnsKGL()
        {
            // Arrange
            var order = new Order
            {
                OrderId = "TEST002",
                PublisherNumber = "99999",
                PublisherName= "BookWorld Ltd",
                OrderMethod = "POD",
                Shipments = new List<Shipment>
                {
                    new() { ShipTo = new ShipTo { IsoCountry = "US" } }
                },
                Items = new List<Item>
                {
                    new()
                    {
                        Sku = "PB-001",
                        PrintQuantity = 25,
                        Components = new List<Component>
                        {
                            new()
                            {
                                Code = "Cover",
                                Attributes = new ComponentAttributes { BindTypeCode = "PB" }
                            }
                        }
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/evaluate", order);

            // Assert
            var result = await response.Content.ReadFromJsonAsync<EvaluationResponse>();
            Assert.NotNull(result);
            Assert.True(result.matched);
            Assert.Equal("KGL", result.productionPlant);
        }

        [Fact]
        public async Task EvaluateOrder_NoMatch_ReturnsNotMatched()
        {
            // Arrange
            var order = new Order
            {
                OrderId = "TEST003",
                PublisherNumber = "11111", // Non-existent publisher
                PublisherName = "BookWorld Ltd",
                OrderMethod = "POD",
                Shipments = new List<Shipment>
                {
                    new() { ShipTo = new ShipTo { IsoCountry = "US" } }
                },
                Items = new List<Item>
                {
                    new()
                    {
                        PrintQuantity = 10,
                        Sku = "PB-001",
                        Components = new List<Component>
                        {
                            new()
                            {
                                Code = "Cover",
                                Attributes = new ComponentAttributes { BindTypeCode = "PB" }
                            }
                        }
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/evaluate", order);

            // Assert
            var result = await response.Content.ReadFromJsonAsync<EvaluationResponse>();
            Assert.NotNull(result);
            Assert.False(result.matched);
            Assert.Null(result.productionPlant);
        }

        [Fact]
        public async Task EvaluateOrder_NullOrder_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/evaluate", (Order)null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task EvaluateOrder_MissingOrderId_ReturnsBadRequest()
        {
            // Arrange
            var order = new Order
            {
                PublisherNumber = "99999",
                OrderMethod = "POD"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/evaluate", order);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Health_ReturnsHealthy()
        {
            // Act
            var response = await _client.GetAsync("/api/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("healthy", content);
        }

        private class EvaluationResponse
        {
            public bool matched { get; set; }
            public string productionPlant { get; set; }
            public string matchedRuleset { get; set; }
            public string matchedRule { get; set; }
            public string reason { get; set; }
            public int evaluationTimeMs { get; set; }
        }
    }
}