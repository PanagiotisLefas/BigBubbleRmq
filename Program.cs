using System;
using BigBubble.Abstractions;
using BigBubble.App;
using BigBubble.Configuration;
using BigBubble.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BigBubble
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WindowWidth = 120;

            Console.SetWindowSize(163, 80);


            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception)
            {

                
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;
                    //RmqConfig configRm = new RmqConfig();
                    //config.GetSection("Rmq").Bind(config);

                    var connection = config.GetSection("Rmq").GetValue<string>("Host");

                    services.AddSingleton<IRmqConnectionFactory>(new RmqConnectionFactory(connection));
                    services.AddSingleton<IUsernameProvider, UsernameProvider>();
                    services.AddSingleton<Publisher>();
                    services.AddSingleton<Consumer>();
                    services.AddSingleton<BigBubbleClient>();
                    services.AddHostedService<Worker>();
                })
            ;
    }
}
