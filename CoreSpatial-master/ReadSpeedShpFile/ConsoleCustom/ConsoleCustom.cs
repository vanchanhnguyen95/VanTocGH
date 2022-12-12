using System;
using System.Collections.Generic;
using System.Text;

namespace ReadSpeedShpFile.ConsoleCustom
{
    class ConsoleCustom
    {
        public static void WriteLineString(string label = "")
        {
            if(string.IsNullOrEmpty(label.Trim()))
                Console.WriteLine();
            Console.WriteLine(label);
        }

        public static void WriteString(string label = "")
        {
            if (string.IsNullOrEmpty(label.Trim()))
                Console.Write(label);
        }

        public static void ShowProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("[");
            Console.CursorLeft = 32;
            Console.Write("]");
            Console.CursorLeft = 1;
            float oneunit = 30.0f / total;

            //draw progress
            int position = 1;
            for (int i = 0; i < oneunit * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw strip bar
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    ");

        }
    }
}
