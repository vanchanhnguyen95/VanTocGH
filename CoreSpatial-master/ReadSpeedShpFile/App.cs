using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static ReadSpeedShpFile.Common.Strings;
using static ReadSpeedShpFile.Controller.HandleShpFile;

namespace ReadSpeedShpFile
{
    public class App
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<App> _logger;

        public App(IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<App>();
            _config = config;
        }

        public async Task Run()
        {
            //List<string> emailAddresses = _config.GetSection("EmailAddresses").Get<List<string>>();
            //foreach (string emailAddress in emailAddresses)
            //{
            //    _logger.LogInformation("Email address: {@EmailAddress}", emailAddress);
            //}

            Console.WriteLine();
            Console.WriteLine(titleString);
            Console.WriteLine(lblChoose1);
            Console.WriteLine(lblChoose2);

            int iChoose;// index lựa chọn
            do
            {
                Console.Write(lblChoose1Or2);
            } while (!int.TryParse(Console.ReadLine(), out iChoose));

            Console.WriteLine($"Đã chọn:  {iChoose}");
            Console.Clear();
            if (iChoose != 1 && iChoose != 2)
                Environment.Exit(0);
            // Đọc file
            if (iChoose == 1)
            {
                CreatePoint();
            }
            // Cập nhât vận tốc giới hạn, ghi ra shape file
            else if (iChoose == 2)
            {
                WriteShpFile();
            }
            Console.ReadLine();
        }
    }
}
