// ============================================================
// RulesetManagementController Integration Tests
// Location: /tests/RulesetEngine.IntegrationTests/RulesetManagementControllerTests.cs
// ============================================================

using Microsoft.AspNetCore.Mvc.Testing;
using RulesetEngine.Domain.Model;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace RulesetEngine.IntegrationTests;

public class RulesetManagementControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public RulesetManagementControllerTests(
        CustomWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5002") // or your actual server URL
        });
        _output = output;
    }

    [Fact]
    public async Task GET_Rulesets_ReturnsOK_AndRulesetList()
    {
        // Act
        var response = await _client.GetAsync("/api/rulesets");
        var content = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Response: {content}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rulesets = await response.Content.ReadFromJsonAsync<List<RulesetDto>>();
        Assert.NotNull(rulesets);
        Assert.NotEmpty(rulesets);
        Assert.Contains(rulesets, r => r.name == "Ruleset Two");
    }

    [Fact]
    public async Task GET_RulesetById_ExistingId_ReturnsRuleset()
    {
        // Arrange - First get all rulesets to find an ID
        var getAllResponse = await _client.GetAsync("/api/rulesets");
        var rulesets = await getAllResponse.Content.ReadFromJsonAsync<List<RulesetDto>>();
        var existingId = rulesets.First().rulesetId;

        // Act
        var response = await _client.GetAsync($"/api/rulesets/{existingId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ruleset = await response.Content.ReadFromJsonAsync<RulesetDto>();
        Assert.NotNull(ruleset);
        Assert.Equal(existingId, ruleset.rulesetId);
    }

    [Fact]
    public async Task GET_RulesetById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/rulesets/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_InvalidateCache_ReturnsOK()
    {
        // Act
        var response = await _client.PostAsync("/api/rulesets/cache/invalidate", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private record RulesetDto(
        int rulesetId,
        string name,
        string description,
        bool isActive,
        int priority
    );
}

public class EndToEndWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public EndToEndWorkflowTests(
        CustomWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5002") // or your actual server URL
        });
        _output = output;
    }

    [Fact]
    public async Task CompleteWorkflow_ViewRulesets_EvaluateOrder_CheckLogs()
    {
        _output.WriteLine("=== Starting End-to-End Workflow Test ===");

        // Step 1: Check health
        _output.WriteLine("\n1. Checking API health...");
        var healthResponse = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        _output.WriteLine("   ✓ API is healthy");

        // Step 2: Get all rulesets
        _output.WriteLine("\n2. Retrieving rulesets...");
        var rulesetsResponse = await _client.GetAsync("/api/rulesets");
        Assert.Equal(HttpStatusCode.OK, rulesetsResponse.StatusCode);
        var rulesetsContent = await rulesetsResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"   ✓ Found rulesets: {rulesetsContent.Substring(0, Math.Min(100, rulesetsContent.Length))}...");

        // Step 3: Evaluate an order (low quantity)
        _output.WriteLine("\n3. Evaluating order with low quantity...");
        var order1 = CreateOrder("E2E001", "99990", 10);
        var eval1Response = await _client.PostAsJsonAsync("/api/evaluate", order1);
        Assert.Equal(HttpStatusCode.OK, eval1Response.StatusCode);
        var eval1Result = await eval1Response.Content.ReadFromJsonAsync<EvaluationResponse>();
        _output.WriteLine($"   ✓ Order routed to: {eval1Result.productionPlant}");
        Assert.Equal("US", eval1Result.productionPlant);

        // Step 4: Evaluate an order (high quantity)
        _output.WriteLine("\n4. Evaluating order with high quantity...");
        var order2 = CreateOrder("E2E002", "99999", 30);
        var eval2Response = await _client.PostAsJsonAsync("/api/evaluate", order2);
        Assert.Equal(HttpStatusCode.OK, eval2Response.StatusCode);
        var eval2Result = await eval2Response.Content.ReadFromJsonAsync<EvaluationResponse>();
        _output.WriteLine($"   ✓ Order routed to: {eval2Result.productionPlant}");
        Assert.Equal("KGL", eval2Result.productionPlant);

        // Step 5: Evaluate order with no match
        _output.WriteLine("\n5. Evaluating order with unknown publisher...");
        var order3 = CreateOrder("E2E003", "99998", 10);
        var eval3Response = await _client.PostAsJsonAsync("/api/evaluate", order3);
        Assert.Equal(HttpStatusCode.OK, eval3Response.StatusCode);
        var eval3Result = await eval3Response.Content.ReadFromJsonAsync<EvaluationResponse>();
        _output.WriteLine($"   ✓ Result: {(eval3Result.matched ? "Matched" : "No match")}");
        Assert.False(eval3Result.matched);

        // Step 6: Invalidate cache
        _output.WriteLine("\n6. Invalidating cache...");
        var cacheResponse = await _client.PostAsync("/api/rulesets/cache/invalidate", null);
        Assert.Equal(HttpStatusCode.OK, cacheResponse.StatusCode);
        _output.WriteLine("   ✓ Cache invalidated");

        _output.WriteLine("\n=== End-to-End Workflow Completed Successfully ===");
    }

    private static Order CreateOrder(string orderId, string publisher, int quantity)
    {
        return new Order
        {
            OrderId = orderId,
            PublisherNumber = publisher,
            PublisherName = "Test Publisher",
            OrderMethod = "POD",
            Shipments = new List<Shipment>
            {
                new() { ShipTo = new ShipTo { IsoCountry = "US" } }
            },
            Items = new List<Item>
            {
                new()
                {
                    Sku = $"SKU-{orderId}",
                    PrintQuantity = quantity,
                    Components = new List<Component>
                    {
                        new() { Code = "Cover", Attributes = new ComponentAttributes { BindTypeCode = "PB" } }
                    }
                }
            }
        };
    }

    private record EvaluationResponse(
        bool matched,
        string productionPlant,
        string matchedRuleset,
        string matchedRule,
        string reason,
        int evaluationTimeMs
    );
}