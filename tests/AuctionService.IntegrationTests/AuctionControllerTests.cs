using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionControllerTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        // arrange

        // act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

        // assert
        Assert.Equal(3, response?.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidIdShouldReturnAuction()
    {
        // arrange


        // act
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");

        // assert
        Assert.Equal("GT", response.Model);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidIdShouldReturn404()
    {
        // arrange


        // act
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuidShouldReturn400()
    {
        // arrange


        // act
        var response = await _httpClient.GetAsync($"api/auctions/invalid-guid");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturn401()
    {
        // arrange
        var auction = new CreateAuctionDto{Make = "test"};

        // act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturn201()
    {
        // arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

        // assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuction_ShouldReturn400()
    {
        // arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        auction.Make = null; // invalid auction

        // act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithAuth_ShouldReturn201()
    {
        // arrange
        var auction = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        UpdateAuctionDto updateAuctionDto = new UpdateAuctionDto
        {
            Make = "testMakeUpdate",
            Model = "testModelUpdate",
            Color = "testColorUpdate",
            Mileage = auction.Mileage + 10,
            Year = auction.Year + 10
        };        

        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}" , updateAuctionDto);

        // assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedAuction = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");
        Assert.Equal("testMakeUpdate", updatedAuction.Make);
        Assert.Equal("testModelUpdate", updatedAuction.Model);
        Assert.Equal("testColorUpdate", updatedAuction.Color);
        Assert.Equal(auction.Mileage + 10, updatedAuction.Mileage);
        Assert.Equal(auction.Year + 10, updatedAuction.Year);
        Assert.NotEqual(auction, updatedAuction);
    }

        [Fact]
    public async Task UpdateAuction_WithValidDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange
        var auction = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("not-bob"));

        UpdateAuctionDto updateAuctionDto = new UpdateAuctionDto
        {
            Make = "testMakeUpdate",
            Model = "testModelUpdate",
            Color = "testColorUpdate",
            Mileage = auction.Mileage + 10,
            Year = auction.Year + 10
        };        

        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}" , updateAuctionDto);

        // assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(db);

        return Task.CompletedTask;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    private CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            Color = "test",
            ImageUrl = "test",
            Mileage = 10,
            Year = 10
        };
    }
}
