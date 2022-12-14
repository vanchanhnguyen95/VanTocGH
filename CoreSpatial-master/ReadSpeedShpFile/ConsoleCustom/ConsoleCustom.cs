using System;

namespace ReadSpeedShpFile.ConsoleCustom
{
    class ConsoleCustom
    {

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

        public static void ProgressBarCiz(int sol, int ust, int deger, int isaret, ConsoleColor color)
        {
            char[] symbol = new char[5] { '\u25A0', '\u2592', '\u2588', '\u2551', '\u2502' };
            int maxBarSize = Console.BufferWidth - 1;
            int barSize = deger;
            decimal f = 1;
            if (barSize + sol > maxBarSize)
            {
                barSize = maxBarSize - (sol + 5); // first 5 character "%100 "
                f = (decimal)deger / (decimal)barSize;
            }
            Console.CursorVisible = false;
            Console.ForegroundColor = color;
            Console.SetCursorPosition(sol + 5, ust);
            for (int i = 0; i < barSize + 1; i++)
            {
                System.Threading.Thread.Sleep(10);
                Console.Write(symbol[isaret]);
                Console.SetCursorPosition(sol, ust);
                Console.Write("%" + (i * f).ToString("0,0"));
                Console.SetCursorPosition(sol + 5 + i, ust);
            }
            Console.ResetColor();
        }
    }
}
