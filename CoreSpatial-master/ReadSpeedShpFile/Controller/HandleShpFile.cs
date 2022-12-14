using System;
//using ReadFile = ReadSpeedShpFile.Services.ReadShpFileService;
//using WriteFile = ReadSpeedShpFile.Services.WriteShpFileService;
using ReadFile = ReadSpeedShpFile.Services.ReadShpFileV2Service;
using WriteFile = ReadSpeedShpFile.Services.WriteShpFileV2Service;

namespace ReadSpeedShpFile.Controller
{
    class HandleShpFile
    {
        public static void CreateDataSpeedFromShpFile()
        {
            ReadFile.CreateDataSpeedFromShpFile();
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static void WriteShpFile()
        {
            WriteFile.CreateShpFileFromShpFile();
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
