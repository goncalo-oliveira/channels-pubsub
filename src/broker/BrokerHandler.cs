using Faactory.Channels;
using Faactory.Channels.Handlers;
using Faactory.Channels.Parcel;

internal sealed class BrokerHandler : ChannelHandler<Message>
{
    private readonly Broker broker;

    public BrokerHandler( Broker broker )
    {
        this.broker = broker;
    }

    public override Task ExecuteAsync( IChannelContext context, Message data )
    {
        // ignore messages without an id
        if ( data.Id == null )
        {
            return Task.CompletedTask;
        }

        /*
        publish to topic:
        - publish:topic
        - pub:topic
        - :topic
        */
        if ( data.Id.StartsWith( "publish:" ) || data.Id.StartsWith( "pub:" ) || data.Id.StartsWith( ":" ) )
        {
            // we change the id to the topic, dropping the "publish:" prefix
            data.Id = data.Id.Substring( data.Id.IndexOf( ':' ) + 1 );

            return broker.WriteAsync( data );
        }

        /*
        subscribe to topic:
        - subscribe:topic
        - sub:topic
        - +:topic
        unsubscribe from topic:
        - unsubscribe:topic
        - unsub:topic
        - -:topic
        */
        if ( data.Id.StartsWith( "subscribe:" ) || data.Id.StartsWith( "sub:" ) || data.Id.StartsWith( "+:" ) )
        {
            var topic = data.Id.Substring( data.Id.IndexOf( ':' ) + 1 );

            broker.Subscribe( context.Channel.Id, topic );
        }
        // unsubscribe from topic
        else if ( data.Id.StartsWith( "unsubscribe:" ) || data.Id.StartsWith( "unsub:" ) || data.Id.StartsWith( "-:" ) )
        {
            var topic = data.Id.Substring( data.Id.IndexOf( ':' ) + 1 );

            broker.Unsubscribe( context.Channel.Id, topic );
        }

        /*
        anything else falls into oblivion
        */

        return ( Task.CompletedTask );
    }
}
