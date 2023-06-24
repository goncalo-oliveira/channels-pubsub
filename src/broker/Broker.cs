using System.Collections.Concurrent;
using Faactory.Channels;
using Faactory.Channels.Parcel;
using Microsoft.Extensions.Logging;

internal sealed class Broker
{
    private readonly ILogger logger;
    private readonly ConcurrentDictionary<string, IChannel> channels = new ConcurrentDictionary<string, IChannel>();
    private readonly ConcurrentDictionary<string, ConcurrentList<string>> topics = new ConcurrentDictionary<string, ConcurrentList<string>>();

    public Broker( ILoggerFactory loggerFactory )
    {
        logger = loggerFactory.CreateLogger<Broker>();
    }

    public void AddChannel( IChannel channel )
    {
        channels.TryAdd( channel.Id, channel );

        logger.LogInformation( $"Channel connected: {channel.Id}" );
    }

    public void RemoveChannel( string channelId )
    {
        channels.TryRemove( channelId, out _ );

        logger.LogInformation( $"Channel disconnected: {channelId}" );

        // unsubscribe from all topics
        foreach ( var topic in topics )
        {
            topic.Value.TryRemove( channelId );
        }
    }

    public void Subscribe( string channelId, string topic )
    {
        if ( !channels.ContainsKey( channelId ) )
        {
            // unknown channel
            logger.LogWarning( $"Unable to subscribe with unknown channel: {channelId}" );

            return;
        }

        // add topic if not already present
        if ( !topics.TryGetValue( topic, out var subscribers ) )
        {
            subscribers = new ConcurrentList<string>();

            topics.TryAdd( topic, subscribers );
        }

        // subscribe
        if ( !subscribers.Contains( channelId ) )
        {
            logger.LogInformation( $"Subscribed to topic: {channelId} -> {topic}" );

            subscribers.TryAdd( channelId );
        }
    }

    public void Unsubscribe( string channelId, string topic )
    {
        if ( !channels.ContainsKey( channelId ) )
        {
            // unknown channel
            logger.LogWarning( $"Unable to unsubscribe with unknown channel: {channelId}" );
            return;
        }

        if ( !topics.TryGetValue( topic, out var subscribers ) )
        {
            // topic not found
            return;
        }

        // unsubscribe
        logger.LogInformation( $"Unsubscribed from topic: {channelId} -> {topic}" );
        subscribers.TryRemove( channelId );

        // remove topic if no more subscribers
        if ( !subscribers.Any() )
        {
            logger.LogInformation( $"Topic removed: {topic}" );

            topics.TryRemove( topic, out _ );
        }
    }

    public Task WriteAsync( Message message )
    {
        // ignore messages without an id (no topic)
        if ( message.Id == null )
        {
            return Task.CompletedTask;
        }

        if ( !topics.TryGetValue( message.Id, out var subscribers ) )
        {
            // no subscribers
            return Task.CompletedTask;
        }

        // no subscribers
        if ( !subscribers.Any() )
        {
            return Task.CompletedTask;
        }

        // write to all subscribers (make a copy to avoid iterating over a modified collection)
        var tasks = subscribers.ToArray().Select( channelId =>
        {
            if ( !channels.TryGetValue( channelId, out var channel ) )
            {
                // unknown channel
                return Task.CompletedTask;
            }

            /*
            if channel is already closed, we won't even try to write to it
            but we'll remove it from the list of subscribers
            */
            if ( channel.IsClosed )
            {
                subscribers.TryRemove( channelId );

                return Task.CompletedTask;
            }

            /*
            if the channel closes while we're writing to it
            we'll remove it from the list of subscribers
            */
            return new Func<Task>( async () =>
            {
                try
                {
                    await channel.WriteAsync( message );
                }
                catch ( Exception )
                {
                    subscribers.TryRemove( channelId );
                }
            } )();
        } )
        .ToArray();

        return Task.WhenAll( tasks );
    }
}
