using CoreSpatial;
using CoreSpatial.BasicGeometrys;
using CoreSpatial.CrsNs;
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
    class ShpFile
    {
        private static string? shpPathInput;
        private static string? directoryOutput;
        //private static List<SpeedProviderUpLoadVm> lstSpeed;
        private static List<Speed3PointUpLoadVm>? lstSpeed;
        private static List<Speed3PointUpLoadVm>? lstSpeed50;
        private static DataTable? speedTable;
        private static int precisions = 10;

        public static void GetShpPathInput()
        {
            Console.Write("\nShape file Path: ");
            shpPathInput = Console.ReadLine();
        }

        public static bool HasShpPathInput()
        {
            if (string.IsNullOrEmpty(shpPathInput))
                return false;

            if (Path.GetExtension(shpPathInput) != ".shp")
                return false;

            if ((!File.Exists(shpPathInput)))
                //Console.WriteLine("Usage: ShapefileDemo <shapefile.shp>");
                return false;

            return true;
        }

        public static void GetDirectoryOutput()
        {
            Console.Write("\nDirectory: ");
            directoryOutput = Console.ReadLine();

            if (!Directory.Exists(directoryOutput))
                Directory.CreateDirectory(directoryOutput);
        }

        public static bool GetLineDistance50LimitShp()
        {
            try
            {
                lstSpeed50 = new List<Speed3PointUpLoadVm>();
                IFeatureSet fs = FeatureSet.Open(shpPathInput);

                foreach (var fe in fs.Features)
                {
                    if (fe.GeometryType != GeometryType.MultiPolyLine)
                    {
                        Console.WriteLine("Data is not PolyLine");
                        return false;
                    }

                    MultiPolyLine? multiPolyLine = fe.Geometry.BasicGeometry as MultiPolyLine;

                    if (multiPolyLine == null)
                    {
                        Console.WriteLine("No have PolyLine in Data");
                        return false;
                    }

                    // Dang quy dinh cot 34 chưa SegmentID
                    long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[34]);

                    foreach (var line in multiPolyLine.PolyLines)
                    {
                        //if (segmentID == 0)
                        //    continue;
                        float dis = CalculateDistance(line.Points[0].X, line.Points[0].Y, line.Points[1].X, line.Points[1].Y);
                        if(dis < 51)
                            continue;

                        // Thêm điểm đầu
                        lstSpeed50.Add(new Speed3PointUpLoadVm()
                        {
                            Lat = Math.Round(line.Points[0].Y, precisions),
                            Lng = Math.Round(line.Points[0].X, precisions)
                            ,
                            Position = "S",
                            ProviderType = 1,
                            SegmentID = segmentID
                        });
                        // Thêm điểm cuối
                        lstSpeed50.Add(new Speed3PointUpLoadVm()
                        {
                            Lat = Math.Round(line.Points[1].Y, precisions),
                            Lng = Math.Round(line.Points[1].X, precisions),
                            Position = "E",
                            ProviderType = 1,
                            SegmentID = segmentID
                        });
                        segmentID = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex);
                return false;
            }

            return true;
        }


        public static bool ReadSpeedLimitShp()
        {
            try
            {
                lstSpeed = new List<Speed3PointUpLoadVm>();
                IFeatureSet fs = FeatureSet.Open(shpPathInput);

                foreach (var fe in fs.Features)
                {
                    if (fe.GeometryType != GeometryType.MultiPolyLine)
                    {
                        Console.WriteLine("Data is not PolyLine");
                        return false;
                    }

                    MultiPolyLine? multiPolyLine = fe.Geometry.BasicGeometry as MultiPolyLine;

                    if (multiPolyLine == null)
                    {
                        Console.WriteLine("No have PolyLine in Data");
                        return false;
                    }

                    // Dang quy dinh cot 34 chưa SegmentID
                    long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[34]);

                    foreach (var line in multiPolyLine.PolyLines)
                    {
                        //if (segmentID == 0)
                        //    continue;

                        // Thêm điểm đầu
                        lstSpeed.Add(new Speed3PointUpLoadVm()
                        {
                            Lat = Math.Round(line.Points[0].Y, precisions),
                            Lng = Math.Round(line.Points[0].X, precisions)
                            ,
                            Position = "S",
                            ProviderType = 1,
                            SegmentID = segmentID
                        });
                        // Thêm điểm cuối
                        lstSpeed.Add(new Speed3PointUpLoadVm()
                        {
                            Lat = Math.Round(line.Points[1].Y, precisions),
                            Lng = Math.Round(line.Points[1].X, precisions)
                            ,
                            Position = "E",
                            ProviderType = 1,
                            SegmentID = segmentID
                        });
                        segmentID = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex);
                return false;
            }

            return true;

            //Console.WriteLine($"Total: {lst.Count()} records");

            //// Create SpeedLimit Data Table
            // CreateSpeedLimitToDB();

            //Console.WriteLine("Enter to write file");
            //CreateSpeedShpFile(lst);
            //Console.WriteLine("Write file succes");
        }

        public static bool CreateSpeedTable()
        {
            try
            {
                if (lstSpeed == null || (!lstSpeed.Any() || lstSpeed.Count() == 0))
                {
                    Console.WriteLine("Have no data to create SpeedTable");
                    return false;
                }

                speedTable = Common.Common.CreateTableSpeedLimit3Point();
                foreach (Speed3PointUpLoadVm item in lstSpeed)
                {
                    speedTable.Rows.Add(item.Lat, item.Lng, item.ProviderType, 0, 0,
                        false, item.SegmentID, item.Position, DateTime.Now, null, "UploadFile", null, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex.ToString());
                return false;
            }

            return true;
        }

        public static bool CreateSpeedLimitToDB()
        {
            //if(!CreateSpeedTable())
            //    return false;

            if (speedTable == null)
            {
                Console.WriteLine("Have no data to create");
                return false;
            }

            try
            {
                // Create SpeedLimit Data Table
                //DataTable speedTable = Common.Common.CreateTableSpeedLimit3Point();
                //foreach (Speed3PointUpLoadVm item in lstSpeed)
                //{
                //    speedTable.Rows.Add(item.Lat, item.Lng, item.ProviderType, 0, 0,
                //        false, item.SegmentID, item.Position, DateTime.Now, null, "UploadFile", null, 0, 0);
                //}

                if (speedTable == null || speedTable.Rows.Count < 0)
                {
                    Console.WriteLine("Have no data to create in SpeedTable");
                    return false;
                }


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
                Console.WriteLine("Err: " + ex);
                return false;
            }

            return true;
        }

        public static bool CreateSpeedLimit3PointToDB()
        {
            if (speedTable == null || speedTable.Rows.Count < 0)
            {
                Console.WriteLine("Have no data to create in SpeedTable");
                return false;
            }

            try
            {
                // Create SpeedLimit Data Table
                //DataTable speedTable = Common.Common.CreateTableSpeedLimit3Point();
                //foreach (Speed3PointUpLoadVm item in lstSpeed)
                //{
                //    speedTable.Rows.Add(item.Lat, item.Lng, item.ProviderType, 0, 0,
                //        false, item.SegmentID, item.Position, DateTime.Now, null, "UploadFile", null, 0, 0);
                //}

                SqlConnection con;
                //con = new SqlConnection(@"Data Source=NC-CHANHNV\MSSQLSERVER2;Initial Catalog=SpeedWebAPI;Persist Security Info=True;User ID=sa;Password=1");
                con = new SqlConnection(connStrDev);
                con.Open();

                SqlCommand cmd = new SqlCommand("[dbo].[Ins_SpeedLimit]", con);
                //SqlCommand cmd = new SqlCommand("[dbo].[Ins_SpeedLimit3Point]", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // Pass table Valued parameter to Store Procedure
                SqlParameter sqlParam = cmd.Parameters.AddWithValue("@SpeedLimit", speedTable);
                //SqlParameter sqlParam = cmd.Parameters.AddWithValue("@SpeedLimit3Point", speedTable);
                sqlParam.SqlDbType = SqlDbType.Structured;

                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex);
                return false;
            }

            return true;
        }

        //string path = @"E:\Data_shp\QL1ATest\1A.shp";
        //List<SpeedProviderUpLoadVm> lst = new List<SpeedProviderUpLoadVm>();
        public static bool WriteToSpeedShpFilePoint()
        {
            if (string.IsNullOrEmpty(directoryOutput))
            {
                Console.WriteLine("No have Directory file output");
                return false;
            }

            if (lstSpeed == null || (!lstSpeed.Any() || lstSpeed.Count() == 0))
            {
                Console.WriteLine("Have no data to write");
                return false;
            }

            try
            {
                IFeatureSet fs = new FeatureSet(FeatureType.Point);

                var dataTable = new DataTable();
                dataTable.Columns.Add("SegmentID", typeof(long));
                dataTable.Columns.Add("X", typeof(double));//Lat
                dataTable.Columns.Add("Y", typeof(double));//Long
                dataTable.Columns.Add("MinSpeed", typeof(int));
                dataTable.Columns.Add("MaxSpeed", typeof(int));

                for (int i = 0; i < lstSpeed.Count; i++)
                {
                    //var point = new GeoPoint(Math.Round(lstSpeed[i].Lat, precisions), Math.Round( lstSpeed[i].Lng, precisions));
                    var point = new GeoPoint(Math.Round(lstSpeed[i].Lng, precisions), Math.Round(lstSpeed[i].Lat, precisions));
                    var feature1 = new Feature(new Geometry(point));
                    fs.Features.Add(feature1);

                    var row = dataTable.NewRow();
                    row[0] = lstSpeed[i].SegmentID;//SegmentID
                    row[1] = lstSpeed[i].Lat;
                    row[2] = lstSpeed[i].Lng;
                    row[3] = 0;//MinSpeed
                    row[4] = 70;//MaxSpeed

                    dataTable.Rows.Add(row);
                }

                fs.Crs = Crs.Wgs84Gcs;
                fs.AttrTable = dataTable;

                var fileBytes = fs.GetShapeFileBytes();

                using var shpFile = new FileStream(directoryOutput + @"\" + fileNameOut + ".shp", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var shxFile = new FileStream(directoryOutput + @"\" + fileNameOut + ".shx", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var dbfFile = new FileStream(directoryOutput + @"\" + fileNameOut + ".dbf", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var prjFile = new FileStream(directoryOutput + @"\" + fileNameOut + ".prj", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                shpFile.Write(fileBytes.ShpBytes);
                shxFile.Write(fileBytes.ShxBytes);
                dbfFile.Write(fileBytes.DbfBytes);
                prjFile.Write(fileBytes.PrjBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex);
                return false;
            }

            return true;

        }

    }
}
