using CoreSpatial;
using CoreSpatial.BasicGeometrys;
using ReadSpeedShpFile.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using static ReadSpeedShpFile.Common.Strings;
using static ReadSpeedShpFile.Common.CalculateGeo;

namespace ReadSpeedShpFile.Services
{
    class ReadShpFileV2Service
    {
        private static string shpPathInput;
        private static List<SpeedProviderUpLoadVm> lstSpeed;
        private static DataTable speedTable;
        private static int precisions = 10;

        public static bool CreatePointFromShpFile()
        {
            // Tiêu đề chương trình
            Console.WriteLine(titleString);

            // Lấy đường dẫn đến file
            Console.WriteLine(lblInpShpFile);
            GetShpPathInput();
            if (!HasShpPathInput())
            {
                Console.WriteLine(lblNoPath);
                return false;
            }    
               
            // Đọc dữ liệu từ shape file
            Console.WriteLine(lblReadFile);
            if (!ReadDataFromShpFile())
            {
                Console.WriteLine(lblReadFileFl);
                return false;
            }
            Console.WriteLine(lblReadFileSc);

            // Enter để tạo dữ liệu từ shape file để cập nhật vào Cơ sở dữ liệu
            Console.WriteLine(lblInpCreateDataFromShpFile);
            Console.ReadLine();
            Console.WriteLine(lblInProcess);

            if (!CreateSpeedTable())
            {
                Console.WriteLine(lblInpCreateDataFromShpFileToDb + lblSpace + lblFail);
                return false;
            }
            Console.WriteLine(lblInpCreateDataFromShpFileToDb + lblSpace + lblSuccess);

            // Enter để tiến hành Cập nhật dữ liệu vào Cơ sở dữ liệu
            Console.WriteLine(lblInpUpdDataToDBProcess);
            Console.ReadLine();
            Console.WriteLine(lblInProcess);

            if (!CreateSpeedLimitToDB())
            {
                Console.WriteLine(lblInpUpdDataToDB + lblSpace + lblFail);
                return false;
            }
            Console.WriteLine(lblInpUpdDataToDB + lblSpace + lblSuccess);

            Console.ReadLine();

            return true;
        }

        private static void GetShpPathInput()
        {
            Console.Write(lblPathFileIs);
            shpPathInput = Console.ReadLine();
        }

        private static bool HasShpPathInput()
        {
            if (string.IsNullOrEmpty(shpPathInput))
                return false;

            if (Path.GetExtension(shpPathInput) != lblDotShp)
                return false;

            if ((!File.Exists(shpPathInput)))
                //Console.WriteLine("Usage: ShapefileDemo <shapefile.shp>");
                return false;

            return true;
        }

        private static bool ReadDataFromShpFile()
        {
            try
            {
                lstSpeed = new List<SpeedProviderUpLoadVm>();
                IFeatureSet fs = FeatureSet.Open(shpPathInput);
                foreach (var fe in fs.Features)
                {
                    if (fe.GeometryType != GeometryType.MultiPolyLine)
                    {
                        Console.WriteLine(lblDatanoPolyline);
                        return false;
                    }

                    MultiPolyLine multiPolyLine = fe.Geometry.BasicGeometry as MultiPolyLine;

                    if (multiPolyLine == null)
                    {
                        Console.WriteLine(lblNoPolylineInShpFile);
                        return false;
                    }

                    // Dang quy dinh cot 34 chưa SegmentID
                    long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[34]);
                    //long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[colSegmendId]);

                    foreach (var line in multiPolyLine.PolyLines)
                    {
                        //float dis = CalculateDistance(line.Points[0].X, line.Points[0].Y, line.Points[line.Points.Count() - 1].X, line.Points[line.Points.Count() - 1].Y);
                        // Thêm điểm đầu
                        lstSpeed.Add(new SpeedProviderUpLoadVm()
                        {
                            Lat = Math.Round((decimal)line.Points[0].Y, precisions),
                            Lng = Math.Round((decimal)line.Points[0].X, precisions),
                            Position = $"S-{segmentID}", ProviderType = 1, SegmentID = segmentID
                        });

                        // Thêm điểm cuối
                        lstSpeed.Add(new SpeedProviderUpLoadVm()
                        {
                            Lat = Math.Round((decimal)line.Points[line.Points.Count() - 1].Y, precisions),
                            Lng = Math.Round((decimal)line.Points[line.Points.Count() - 1].X, precisions),
                            Position = $"E-{segmentID}", ProviderType = 1, SegmentID = segmentID
                        });

                        // Mặc định lấy 2 điểm ở bên trong, tính ra khoảng cách rồi mình chia làm 3 đoạn
                        float partPoint = (CalculateDistance(line.Points[0].Y, line.Points[0].X
                            , line.Points[line.Points.Count() - 1].Y, line.Points[line.Points.Count() - 1].X) / 2);

                        ClsPoint first = new ClsPoint() { Latitude = line.Points[0].Y, Longitude = line.Points[0].X };
                        ClsPoint second = new ClsPoint() { Latitude = line.Points[line.Points.Count() - 1].Y, Longitude = line.Points[line.Points.Count() - 1].X };
                        List<ClsPoint> lstPoint = GetVituralPoint(first, second, (int)partPoint);

                        // Thêm ở giữa 2 vị trí
                        lstSpeed.Add(new SpeedProviderUpLoadVm()
                        {
                            Lat = Math.Round((decimal)lstPoint[0].Latitude, precisions),
                            Lng = Math.Round((decimal)lstPoint[0].Longitude, precisions),
                            Position = $"M-{segmentID}-{0}",
                            ProviderType = 1,
                            MinSpeed = 1,
                            MaxSpeed = 1,
                            SegmentID = segmentID
                        });

                        lstSpeed.Add(new SpeedProviderUpLoadVm()
                        {
                            Lat = Math.Round((decimal)lstPoint[lstPoint.Count() - 1].Latitude, precisions),
                            Lng = Math.Round((decimal)lstPoint[lstPoint.Count() - 1].Longitude, precisions),
                            Position = $"M-{segmentID}-{1}",
                            ProviderType = 1,
                            SegmentID = segmentID
                        });

                        segmentID = 0;
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(lblErr + ex.ToString());
                return false;
            }

            return true;
        }

        private static bool CreateSpeedTable()
        {
            try
            {
                if (lstSpeed == null || (!lstSpeed.Any() || lstSpeed.Count() == 0))
                {
                    Console.WriteLine("Have no data to create SpeedTable");
                    return false;
                }

                speedTable = Common.Common.CreateTableSpeedLimit3Point();
                foreach (SpeedProviderUpLoadVm item in lstSpeed)
                {
                    speedTable.Rows.Add(Math.Round(item.Lat, precisions), Math.Round(item.Lng, precisions), item.ProviderType, item.Position,
                        0, 0,
                        false, item.SegmentID, DateTime.Now, null, "UploadFile", null, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(lblErr + ex.ToString());
                return false;
            }

            return true;
        }

        private static bool CreateSpeedLimitToDB()
        {
            if (speedTable == null || speedTable.Rows.Count < 0)
                //Console.WriteLine("Have no data to create in SpeedTable");
                return false;

            try
            {
                SqlConnection con;
                //con = new SqlConnection(@"Data Source=NC-CHANHNV\MSSQLSERVER2;Initial Catalog=SpeedWebAPI;Persist Security Info=True;User ID=sa;Password=1");
                con = new SqlConnection(connStrDev);
                con.Open();

                SqlCommand cmd = new SqlCommand("[dbo].[Ins_SpeedLimit]", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // Pass table Valued parameter to Store Procedure
                SqlParameter sqlParam = cmd.Parameters.AddWithValue("@SpeedLimit", speedTable);
                sqlParam.SqlDbType = SqlDbType.Structured;

                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(lblErr + ex.ToString());
                return false;
            }

            return true;
        }
    }
}
