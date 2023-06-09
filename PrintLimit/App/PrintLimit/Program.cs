using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintLimit.Services.WindowServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .Build();
            host.Run();

            //Run product
            //program.windowService.CreateService();
            //program.windowService.StartService();
        }
    }
}
