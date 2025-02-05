using System;
using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDbContext _dbContext;

    public BidPlacedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        System.Console.WriteLine("--> Consuming bid placed");
        var bid = context.Message;
        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if(auction.CurrentHighBid == null || 
            bid.BidStatus.Contains("Accepted") &&
            bid.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = bid.Amount;
            await _dbContext.SaveChangesAsync();
        }
    }
}
