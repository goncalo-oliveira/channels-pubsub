using System.Text;
using System.Text.Json;
using Faactory.Channels;
using Faactory.Channels.Handlers;
using Faactory.Channels.Parcel;

internal sealed class MessageHandler : ChannelHandler<Message>
{
    public override Task ExecuteAsync( IChannelContext context, Message data )
    {
        // ignore messages without an id or content
        if ( ( data.Id == null ) || ( data.Content == null ) )
        {
            return Task.CompletedTask;
        }

        // output message
        var message = Encoding.UTF8.GetString( data.Content );
        var json = JsonSerializer.Serialize( new
        {
            data.Id,
            message
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        } );

        Console.WriteLine( json );

        return ( Task.CompletedTask );
    }
}
