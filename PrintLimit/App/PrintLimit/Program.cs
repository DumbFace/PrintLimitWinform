using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintLimit.Services.WindowServices;
using Serilog;

namespace PrintLimit
{
    public class Program
    {
        private readonly IWindowService windowService;
        public Program()
        {
            windowService = new WindowService();
        }

        static void Main(string[] args)
        {
            var program = new Program();
            ////Debug 
            IHost host = Host.CreateDefaultBuilder(args)
                 .UseWindowsService()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<MyBackgroundApp>();
                })
                .UseSerilog()
                .Build();

            //Register Background Service
            program.windowService.CreateService();
            program.windowService.StartService();

            host.Run();


        }
    }
}
