using System;
using System.Text;
using static ReadSpeedShpFile.Common.Strings;
using static ReadSpeedShpFile.Controller.HandleShpFile;

namespace ReadSpeedShpFile
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            int iChoose;// index lựa chọn

            Console.WriteLine();
            Console.WriteLine(titleString);
            Console.WriteLine(lblChoose1);
            Console.WriteLine(lblChoose2);

            do
            {
                Console.Write(lblChoose1Or2);
            } while (!int.TryParse(Console.ReadLine(), out iChoose));

            Console.WriteLine($"Đã chọn:  {iChoose}");
            Console.Clear();
            if (iChoose != 1 && iChoose != 2)
                Environment.Exit(0);
            // Đọc file
            if(iChoose == 1)
            {
                CreatePoint();
            }
            // Cập nhât vận tốc giới hạn, ghi ra shape file
            else if(iChoose == 2)
            {
                WriteShpFile();
            }
            Console.ReadLine();
        }

    }
}
