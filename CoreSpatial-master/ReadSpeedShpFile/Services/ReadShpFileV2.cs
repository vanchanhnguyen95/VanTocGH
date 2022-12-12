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
    class ReadShpFileV2
    {
        private static string shpPathInput;
        private static string directoryOutput;
        //private static List<SpeedProviderUpLoadVm> lstSpeed;
        private static List<Speed3PointUpLoadVm> lstSpeed;
        private static List<Speed3PointUpLoadVm> lstSpeedAnylysOver50;// Danh sách chưa các điểm đầu, cuối có kc > 50
        private static List<long> lstSegmentID; // Danh sách Point > 50
        private static List<Speed3PointUpLoadVm> lstSpeedOver50; // Danh sách Point > 50
        private static List<Speed3PointUpLoadVm> lstSpeedUnder50; // Danh sách Point <= 50
        private static DataTable speedTable;
        private static int precisions = 10;
        public static int colSegmendId = 34;

        public static void GetShpPathInputV2()
        {
            Console.Write("\nShape file Path: ");
            shpPathInput = Console.ReadLine();
        }

        public static bool HasShpPathInputV2()
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

        public static void GetDirectoryOutputV2()
        {
            Console.Write("\nDirectory: ");
            directoryOutput = Console.ReadLine();

            if (!Directory.Exists(directoryOutput))
                Directory.CreateDirectory(directoryOutput);
        }

        /* Phân tích thành 2 danh sách Point:
         * 1. Những Point có khoảng cách <= 50 : lstSpeedUnder50
         * 2. Những Point có khoảng cách > 50 L: lstSpeedAnylysOver50
         */
        public static bool AnylysLimitShpFileV2()
        {
            try
            {
                lstSpeed = new List<Speed3PointUpLoadVm>();
                lstSpeedUnder50 = new List<Speed3PointUpLoadVm>();
                lstSpeedAnylysOver50 = new List<Speed3PointUpLoadVm>();
                lstSegmentID = new List<long>();
                IFeatureSet fs = FeatureSet.Open(shpPathInput);
                foreach (var fe in fs.Features)
                {
                    if (fe.GeometryType != GeometryType.MultiPolyLine)
                    {
                        Console.WriteLine("Data is not PolyLine");
                        return false;
                    }

                    MultiPolyLine multiPolyLine = fe.Geometry.BasicGeometry as MultiPolyLine;

                    if (multiPolyLine == null)
                    {
                        Console.WriteLine("No have PolyLine in Data");
                        return false;
                    }

                    // Dang quy dinh cot 34 chưa SegmentID
                    //long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[34]);
                    long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[colSegmendId]);

                   

                    foreach (var line in multiPolyLine.PolyLines)
                    {
                        //float dis = CalculateDistance(line.Envelope.MinX, line.Envelope.MinY, line.Envelope.MaxX, line.Envelope.MaxY);
                        float dis = CalculateDistance(line.Points[0].X, line.Points[0].Y, line.Points[line.Points.Count() - 1].X, line.Points[line.Points.Count() - 1].Y);

                        if (dis < 51)
                        {
                            // Thêm điểm đầu
                            //lstSpeedUnder50.Add(new Speed3PointUpLoadVm()
                            lstSpeed.Add(new Speed3PointUpLoadVm()
                            {
                                //Lat = Math.Round(line.Envelope.MinY, precisions),
                                //Lng = Math.Round(line.Envelope.MinX, precisions),
                                Lat = Math.Round(line.Points[0].Y, precisions),
                                Lng = Math.Round(line.Points[0].X, precisions),
                                Position = $"S-{segmentID}",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                            // Thêm điểm cuối
                            //lstSpeedUnder50.Add(new Speed3PointUpLoadVm()
                            lstSpeed.Add(new Speed3PointUpLoadVm()
                            {
                                //Lat = Math.Round(line.Envelope.MaxY, precisions),
                                //Lng = Math.Round(line.Envelope.MaxX, precisions),

                                Lat = Math.Round(line.Points[line.Points.Count() - 1].Y, precisions),
                                Lng = Math.Round(line.Points[line.Points.Count() - 1].X, precisions),
                                Position = $"E-{segmentID}",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                        }
                        else
                        {
                            // Thêm vào danh sách lstSegmentID
                            lstSegmentID.Add(segmentID);

                            // Thêm điểm đầu
                            lstSpeedAnylysOver50.Add(new Speed3PointUpLoadVm()
                            {
                                Lat = Math.Round(line.Points[0].Y, precisions),
                                Lng = Math.Round(line.Points[0].X, precisions),
                                Position = $"S-{segmentID}",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                            // Thêm điểm cuối
                            lstSpeedAnylysOver50.Add(new Speed3PointUpLoadVm()
                            {
                                Lat = Math.Round(line.Points[line.Points.Count() - 1].Y, precisions),
                                Lng = Math.Round(line.Points[line.Points.Count() - 1].X, precisions),
                                Position = $"E-{segmentID}",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                        }


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

      
        /*
         * Băm những điểm có kc > 50
         * params: lstSpeedOver50
         * return: danh sách những point được băm từ danh sách có Point > 50
         */
        public static bool GetlstSpeedOver50V2()
        {
            if (lstSegmentID == null || (!lstSegmentID.Any() || lstSegmentID.Count() == 0))
            {
                return true;
            }

            lstSpeedOver50 = new List<Speed3PointUpLoadVm>();
            Speed3PointUpLoadVm startPoint;
            Speed3PointUpLoadVm endPoint;

            try
            {
                for (int i = 0; i < lstSegmentID.Count(); i++)
                {
                    startPoint = lstSpeedAnylysOver50.Where(s => s.SegmentID == lstSegmentID[i] && s.Position == $"S-{lstSegmentID[i]}").FirstOrDefault();
                    endPoint = lstSpeedAnylysOver50.Where(s => s.SegmentID == lstSegmentID[i] && s.Position == $"E-{lstSegmentID[i]}").FirstOrDefault();

                    ClsPoint first = new ClsPoint() { Latitude = startPoint.Lat, Longitude = startPoint.Lng };
                    ClsPoint second = new ClsPoint() { Latitude = endPoint.Lat, Longitude = endPoint.Lng };
                    //List<ClsPoint> lstPoint = GetVituralPoint(first, second, 50000);
                    List<ClsPoint> lstPoint = GetVituralPoint(second, first, 50);
                    //float distance = CalculateDistance(line.Points[0].X, line.Points[0].Y, line.Points[1].X, line.Points[1].Y);
                    // Băm theo line cùng SegmentID

                    // Thêm điểm đầu
                    //lstSpeedOver50.Add(new Speed3PointUpLoadVm()
                    lstSpeed.Add(new Speed3PointUpLoadVm()
                    {
                        Lat = Math.Round(startPoint.Lat, precisions),
                        Lng = Math.Round(startPoint.Lng, precisions),
                        Position = $"S-{lstSegmentID[i]}",
                        ProviderType = 1,
                        SegmentID = lstSegmentID[i]
                    });

                    int iMiddle = 0;
                    // Thêm giữa, những điểm băm
                    foreach (ClsPoint item in lstPoint)
                    {
                        //lstSpeedOver50.Add(new Speed3PointUpLoadVm()
                        lstSpeed.Add(new Speed3PointUpLoadVm()
                        {
                            Lat = Math.Round(item.Latitude, precisions),
                            Lng = Math.Round(item.Longitude, precisions),
                            Position = $"M-{lstSegmentID[i]}-{iMiddle}",
                            ProviderType = 1,
                            SegmentID = lstSegmentID[i]
                        });
                        iMiddle++;
                    }
                    iMiddle = 0;

                    // Thêm điểm cuối
                    //lstSpeedOver50.Add(new Speed3PointUpLoadVm()
                    lstSpeed.Add(new Speed3PointUpLoadVm()
                    {
                        Lat = Math.Round(endPoint.Lat, precisions),
                        Lng = Math.Round(endPoint.Lng, precisions),
                        Position = $"E-{lstSegmentID[i]}",
                        ProviderType = 1,
                        SegmentID = lstSegmentID[i]
                    });

                    //lstPoint = null;
                    startPoint = null;
                    endPoint = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex.ToString());
                return false;
            }
            
            return true;
        }

        public static bool CreateSpeedTableV2()
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
                    speedTable.Rows.Add(item.Lat, item.Lng, item.ProviderType, item.Position,
                        0, 0,
                        false, item.SegmentID, DateTime.Now, null, "UploadFile", null, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex.ToString());
                return false;
            }

            return true;
        }

        public static bool CreateSpeedLimitToDBV2()
        {

            if (speedTable == null)
            {
                Console.WriteLine("Have no data to create");
                return false;
            }

            try
            {

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


        #region WriteFile
        public static bool AnylysLimitShpToWriteFileV2()
        {
            try
            {
                lstSpeedUnder50 = new List<Speed3PointUpLoadVm>();
                lstSpeedAnylysOver50 = new List<Speed3PointUpLoadVm>();
                lstSegmentID = new List<long>();
                IFeatureSet fs = FeatureSet.Open(shpPathInput);

                foreach (var fe in fs.Features)
                {
                    if (fe.GeometryType != GeometryType.MultiPolyLine)
                    {
                        Console.WriteLine("Data is not PolyLine");
                        return false;
                    }

                    MultiPolyLine multiPolyLine = fe.Geometry.BasicGeometry as MultiPolyLine;

                    if (multiPolyLine == null)
                    {
                        Console.WriteLine("No have PolyLine in Data");
                        return false;
                    }

                    // Dang quy dinh cot 34 chưa SegmentID
                    //long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[34]);
                    long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[colSegmendId]);

                    foreach (var line in multiPolyLine.PolyLines)
                    {
                        //var minX = line.Envelope.MinX;
                        //var minY = line.Envelope.MinY;
                        //var maxX = line.Envelope.MaxX;
                        //var maxY = line.Envelope.MaxY;

                        //if (segmentID == 0)
                        //    continue;
                        float dis = CalculateDistance(line.Envelope.MinX, line.Envelope.MinY, line.Envelope.MaxX, line.Envelope.MaxY);
                        if (dis < 51)
                        {
                            // Thêm điểm đầu
                            lstSpeedUnder50.Add(new Speed3PointUpLoadVm()
                            {
                                Lat = Math.Round(line.Envelope.MinY, precisions),
                                Lng = Math.Round(line.Envelope.MinX, precisions),
                                Position = "S",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                            // Thêm điểm cuối
                            lstSpeedUnder50.Add(new Speed3PointUpLoadVm()
                            {
                                Lat = Math.Round(line.Envelope.MaxY, precisions),
                                Lng = Math.Round(line.Envelope.MaxX, precisions),
                                Position = "E",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                        }
                        else
                        {
                            // Thêm vào danh sách lstSegmentID
                            lstSegmentID.Add(segmentID);

                            // Thêm điểm đầu
                            lstSpeedAnylysOver50.Add(new Speed3PointUpLoadVm()
                            {
                                Lat = Math.Round(line.Envelope.MinY, precisions),
                                Lng = Math.Round(line.Envelope.MinX, precisions),
                                Position = "S",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                            // Thêm điểm cuối
                            lstSpeedAnylysOver50.Add(new Speed3PointUpLoadVm()
                            {
                                Lat = Math.Round(line.Envelope.MaxY, precisions),
                                Lng = Math.Round(line.Envelope.MaxX, precisions),
                                Position = "E",
                                ProviderType = 1,
                                SegmentID = segmentID
                            });
                        }


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

        /*
         * Băm những điểm có kc > 50
         * params: lstSpeedOver50
         * return: danh sách những point được băm từ danh sách có Point > 50
         */
        public static bool GetlstSpeedOver50ToWrileFileV2()
        {
            if (lstSegmentID == null || (!lstSegmentID.Any() || lstSegmentID.Count() == 0))
            {
                return true;
            }

            lstSpeedOver50 = new List<Speed3PointUpLoadVm>();
            Speed3PointUpLoadVm startPoint;
            Speed3PointUpLoadVm endPoint;

            try
            {
                for (int i = 0; i < lstSegmentID.Count(); i++)
                {
                    startPoint = lstSpeedAnylysOver50.Where(s => s.SegmentID == lstSegmentID[i] && s.Position == "S").FirstOrDefault();
                    endPoint = lstSpeedAnylysOver50.Where(s => s.SegmentID == lstSegmentID[i] && s.Position == "E").FirstOrDefault();

                    ClsPoint first = new ClsPoint() { Latitude = startPoint.Lat, Longitude = startPoint.Lng };
                    ClsPoint second = new ClsPoint() { Latitude = endPoint.Lat, Longitude = endPoint.Lng };
                    List<ClsPoint> lstPoint = GetVituralPoint(first, second, 50);
                    //float distance = CalculateDistance(line.Points[0].X, line.Points[0].Y, line.Points[1].X, line.Points[1].Y);
                    // Băm theo line cùng SegmentID

                    // Thêm điểm đầu
                    lstSpeedOver50.Add(new Speed3PointUpLoadVm()
                    {
                        Lat = Math.Round(startPoint.Lat, precisions),
                        Lng = Math.Round(startPoint.Lng, precisions),
                        Position = "S",
                        ProviderType = 1,
                        SegmentID = lstSegmentID[i]
                    });

                    // Thêm giữa
                    foreach (ClsPoint item in lstPoint)
                    {
                        lstSpeedOver50.Add(new Speed3PointUpLoadVm()
                        { Lat = item.Latitude, Lng = item.Longitude, Position = $"M-{i}", ProviderType = 1, SegmentID = lstSegmentID[i] });
                    }

                    // Thêm điểm cuối
                    lstSpeedOver50.Add(new Speed3PointUpLoadVm()
                    {
                        Lat = Math.Round(endPoint.Lat, precisions),
                        Lng = Math.Round(endPoint.Lng, precisions),
                        Position = "E",
                        ProviderType = 1,
                        SegmentID = lstSegmentID[i]
                    });

                    lstPoint = null;
                    startPoint = null;
                    endPoint = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err: " + ex.ToString());
                return false;
            }

            return true;
        }

        #endregion

        public static bool CreateSpeedLimit3PointToDBV2()
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
        public static bool WriteToSpeedShpFilePointV2()
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
                dataTable.Columns.Add("Position", typeof(string));

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
                    row[5] = lstSpeed[i].Position;

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
