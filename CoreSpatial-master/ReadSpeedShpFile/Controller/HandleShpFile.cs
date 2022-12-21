using ReadSpeedShpFile.Common;
using System;
//using ReadFile = ReadSpeedShpFile.Services.ReadShpFileService;
//using WriteFile = ReadSpeedShpFile.Services.WriteShpFileService;
using ReadFile = ReadSpeedShpFile.Services.ReadShpFileV3Service;
//using ReadFile = ReadSpeedShpFile.Services.ReadShpFileV2Service;
//using WriteFile = ReadSpeedShpFile.Services.WriteShpFileV2Service;
using WriteFile = ReadSpeedShpFile.Services.WriteShpFileV3Service;

namespace ReadSpeedShpFile.Controller
{
    class HandleShpFile
    {

        public static void CreateDataSpeedFromShpFile(SpeedConfig speedConfig)
        {
            ReadFile.CreateDataSpeedFromShpFile(speedConfig);
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static void WriteShpFile(SpeedConfig speedConfig)
        {
            WriteFile.CreateShpFileFromShpFile(speedConfig);
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
