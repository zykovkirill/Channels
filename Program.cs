// See https://aka.ms/new-console-template for more information
using System.Threading.Channels;

static async ValueTask ProduceAsync(ChannelWriter<ProductFeature> channelWriter, ProductFeature productFeature)
{

    for (int i = 0; i < 5; i++)
    {
        await channelWriter.WriteAsync(productFeature = productFeature with { Price = productFeature.Price + 1, ProductId = Guid.NewGuid(), Rating = productFeature.Rating + 1 });
        await Task.Delay(1000);
    }
    var complate = channelWriter.TryComplete();
    if(!complate)
    {
        Console.WriteLine("Канал закрыт");
    }
}

static async ValueTask ConsumerAsync( ChannelReader<ProductFeature> channelReader)
{
    while (await channelReader.WaitToReadAsync())
    {
        while(channelReader.TryRead(out ProductFeature productFeature))
        {
            Console.WriteLine(productFeature);
        }
    }

}


var channel = Channel.CreateUnbounded<ProductFeature>(new UnboundedChannelOptions
{
    SingleReader= false,
    SingleWriter= true,

});

var taskArray = new ValueTask[4];
var product = new ProductFeature(ProductId: Guid.NewGuid(), Rating: 5, Price: 1 );
taskArray[0] = ProduceAsync(channel, product);
taskArray[1] = ProduceAsync(channel, product);
taskArray[2] = ProduceAsync(channel, product);
taskArray[3] = ConsumerAsync(channel);


foreach (var item in taskArray)
{
   await item;
}


public readonly record struct ProductFeature(
    Guid ProductId,
    int Rating,
    int Price 
    );

