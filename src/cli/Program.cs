using System.Text;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Parcel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder( args );

builder.Logging.ClearProviders()
    .AddSimpleConsole( options =>
    {
        options.SingleLine = true;
    } )
    ;

builder.Configuration.AddCommandLine( args );

builder.Services.AddClientChannelFactory( channel =>
{
    channel.Configure( options =>
    {
        var host = builder.Configuration["host"] ?? "localhost";
        var port = builder.Configuration["port"] ?? "8327";

        options.Host = host;
        options.Port = int.Parse( port );
    } );

    /*
    channel services
    */
    //channel.AddIdleChannelService();

    /*
    input pipeline
    */
    channel.AddInputAdapter<ParcelDecoderAdapter>();
    channel.AddInputHandler<MessageHandler>();

    /*
    output pipeline
    */
    channel.AddOutputAdapter<ParcelEncoderAdapter>();
} );

var app = builder.Build();

var factory = app.Services.GetRequiredService<IClientChannelFactory>();

var channel = await factory.CreateAsync();

while ( true )
{
    var line = Console.ReadLine();

    if ( string.IsNullOrEmpty( line ) )
    {
        continue;
    }

    if ( ( line == "exit" ) || ( line == "quit" ) )
    {
        break;
    }

    var idx = line.IndexOf( ' ' );
    if ( idx < 0 )
    {
        await channel.WriteAsync( new Message
        {
            Id = line
        } );
    }
    else
    {
        var id = line.Substring( 0, idx );
        var text = line.Substring( idx + 1 );

        await channel.WriteAsync( new Message
        {
            Id = id,
            ContentType = "text/plain",
            Content = Encoding.UTF8.GetBytes( text )
        } );
    }
}
