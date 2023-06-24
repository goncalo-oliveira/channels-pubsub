/*
This service starts when a channel connects and stops when the channel disconnects.
It's purpose is to notify the broker that a channel has connected or disconnected.
*/
using Faactory.Channels;

internal sealed class BrokerChannelService : IChannelService
{
    private readonly Broker broker;
    private string channelId = string.Empty;

    public BrokerChannelService( Broker broker )
    {
        this.broker = broker;
    }

    public void Dispose()
    { }

    public Task StartAsync( IChannel channel, CancellationToken cancellationToken = default )
    {
        // reserve the channel id
        channelId = channel.Id;

        broker.AddChannel( channel );

        return Task.CompletedTask;
    }

    public Task StopAsync( CancellationToken cancellationToken = default )
    {
        broker.RemoveChannel( channelId );

        return Task.CompletedTask;
    }
}
