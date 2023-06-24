using Faactory.Channels.Adapters;
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

builder.Services.AddChannels( channel =>
{
    channel.Configure( options =>
    {
        options.Port = 8327;
    } );

    /*
    channel services
    */
    //channel.AddIdleChannelService();
    channel.AddChannelService<BrokerChannelService>();

    /*
    input pipeline
    */
    channel.AddInputAdapter<ParcelDecoderAdapter>();
    channel.AddInputHandler<BrokerHandler>();

    /*
    output pipeline
    */
    channel.AddOutputAdapter<ParcelEncoderAdapter>();
} );

builder.Services.AddSingleton<Broker>();

var app = builder.Build();

app.Run();
