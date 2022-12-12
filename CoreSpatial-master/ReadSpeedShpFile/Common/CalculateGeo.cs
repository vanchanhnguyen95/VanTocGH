using ReadSpeedShpFile.Models;
using System;
using System.Collections.Generic;

namespace ReadSpeedShpFile.Common
{
    class CalculateGeo
    {
        public static List<ClsPoint> GetVituralPoint(ClsPoint First, ClsPoint Second, int KhoangCachGiuaHaiDiem)
        {
            var ReturnValue = new List<ClsPoint>();
            float distance = CalculateDistance(First.Longitude, First.Latitude, Second.Longitude, Second.Latitude);
            int step = (int)Math.Round(distance * 2 / KhoangCachGiuaHaiDiem, 0);

            if (step < 2) step = 2;

            for (int i = 1; i < step; i++)
            {
                float r = KhoangCachGiuaHaiDiem * i / (2 * distance);

                if (r < 1)
                {
                    ClsPoint newPoint = new ClsPoint();
                    newPoint.Latitude = r * Second.Latitude + (1 - r) * First.Latitude;
                    newPoint.Longitude = r * Second.Longitude + (1 - r) * First.Longitude;

                    ReturnValue.Add(newPoint);
                }
            }

            return ReturnValue;
        }


        public static float CalculateDistance(double X1, double Y1, double X2, double Y2)
        {
            double P1X = X1 * (Math.PI / 180);
            double P1Y = Y1 * (Math.PI / 180);
            double P2X = X2 * (Math.PI / 180);
            double P2Y = Y2 * (Math.PI / 180);

            double Kc = 0;
            double Temp = 0;

            if (X1 == X2 && Y1 == Y2) return 0;

            Kc = P2X - P1X;
            Temp = Math.Cos(Kc);
            Temp = Temp * Math.Cos(P2Y);
            Temp = Temp * Math.Cos(P1Y);

            Kc = Math.Sin(P1Y);
            Kc = Kc * Math.Sin(P2Y);
            Temp = Temp + Kc;
            Kc = Math.Acos(Temp);
            Kc = Kc * 6376000;

            //Hieu chinh quang duong km gps so voi thuc te
            //Kc = Kc * 1.0566;

            return (float)Kc;
        }
    }
}
