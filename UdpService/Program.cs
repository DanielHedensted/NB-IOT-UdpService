using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using UdpService;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "UDP IOT service";       
    })
    
    .ConfigureServices(services =>
    {        
        services.AddHostedService<Worker>();      
    })   
    
    .Build();  

host.Run();

