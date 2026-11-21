using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public sealed class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostProduct_Returns201_AndGetByLocation_Returns200()
    {
        // Arrange
        var newProduct = new SaveProductRequest(
            Sku: $"TEST-{Guid.NewGuid():N}"[..12],
            Name: "Cement M400",
            Unit: "bag",
            Quantity: 40);

        // Act 1: POST -> 201 Created
        var postResponse = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert 1
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var createdDto = await postResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(createdDto);
        Assert.Equal(newProduct.Sku!.ToUpperInvariant(), createdDto.Sku);
        Assert.Equal(newProduct.Name, createdDto.Name);

        // Act 2: GET за отриманим Location -> 200 OK
        var getResponse = await _client.GetAsync(postResponse.Headers.Location);

        // Assert 2
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedDto = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(fetchedDto);
        Assert.Equal(createdDto.Id, fetchedDto.Id);
        Assert.Equal(newProduct.Quantity, fetchedDto.Quantity);
    }

    [Fact]
    public async Task PostDuplicateSku_Returns409Conflict()
    {
        var sku = $"DUP-{Guid.NewGuid():N}"[..12];
        var request = new SaveProductRequest(sku, "Brick", "pcs", 100);

        var first = await _client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task PostInvalidProduct_Returns400BadRequest()
    {
        var invalid = new SaveProductRequest("", "X", "", -5);

        var response = await _client.PostAsJsonAsync("/api/products", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}