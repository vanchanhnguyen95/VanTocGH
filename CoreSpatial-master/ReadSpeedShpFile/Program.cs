using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static ReadSpeedShpFile.Common.Strings;
using static ReadSpeedShpFile.Controller.HandleShpFile;


namespace ReadSpeedShpFile
{
    internal static class Program
    {
        public static IConfigurationRoot? configuration;

        static int Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
           

            // Initialize serilog logger
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                 .MinimumLevel.Debug()
                 .Enrich.FromLogContext()
                 .CreateLogger();

            try
            {
                // Start!
                MainAsync(args).Wait();
                return 0;
            }
            catch
            {
                return 1;
            }
        }
        static async Task MainAsync(string[] args)
        {
            // Create service collection
            Log.Information("Creating service collection");
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            Log.Information("Building service provider");
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Print connection string to demonstrate configuration object is populated
            //Console.WriteLine(configuration.GetConnectionString("DataConnection"));
            //Console.ReadLine();

            try
            {
                Log.Information("Starting service");
                await serviceProvider.GetService<App>().Run();
                Log.Information("Ending service");

                //Console.WriteLine();
                //Console.WriteLine(titleString);
                //Console.WriteLine(lblChoose1);
                //Console.WriteLine(lblChoose2);

                //int iChoose;// index lựa chọn
                //do
                //{
                //    Console.Write(lblChoose1Or2);
                //} while (!int.TryParse(Console.ReadLine(), out iChoose));

                //Console.WriteLine($"Đã chọn:  {iChoose}");
                //Console.Clear();
                //if (iChoose != 1 && iChoose != 2)
                //    Environment.Exit(0);
                //// Đọc file
                //if (iChoose == 1)
                //{
                //    CreatePoint();
                //}
                //// Cập nhât vận tốc giới hạn, ghi ra shape file
                //else if (iChoose == 2)
                //{
                //    WriteShpFile();
                //}
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error running service");
                throw ex;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Add logging
            serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
            {
                builder
                    .AddSerilog(dispose: true);
            }));

            serviceCollection.AddLogging();

            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            // Add app
            serviceCollection.AddTransient<App>();
        }

    }
}
