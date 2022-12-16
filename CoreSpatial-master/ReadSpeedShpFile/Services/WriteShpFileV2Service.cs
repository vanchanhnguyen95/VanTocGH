using CoreSpatial;
using CoreSpatial.BasicGeometrys;
using CoreSpatial.CrsNs;
using Microsoft.Extensions.Configuration;
using ReadSpeedShpFile.Common;
using ReadSpeedShpFile.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using static ReadSpeedShpFile.Common.CalculateGeo;
using static ReadSpeedShpFile.Common.Strings;

namespace ReadSpeedShpFile.Services
{
    class WriteShpFileV2Service
    {
        private static string? shpPathInput;
        private static string? directoryOutput;
        private static List<SpeedProviderUpLoadVm>? lstSpeed;
        private static List<SpeedProviderUpLoadVm>? lstSpeedFromDb;
        private static DataTable? speedTable;
        private static int precisions = 10;

        private static List<long>? lstSegmentID;
        private static List<long>? lstSegmentIDLine;
        private static List<SpeedProviderUpLoadVm>? lstCreatePolyline;

        // Danh sách dùng để tạo shape file point
        //private static List<SpeedProviderUpLoadVm> lstLineChange;// Danh sách các line có vận tốc thay đổi
        //private static List<long> lstSegmentIDLineChange;// Danh sách chứa segmentID của line có vận tốc thay đổi
        private static List<SpeedProviderUpLoadVm>? lstCreatePoint;

        public static bool CreateShpFileFromShpFile(SpeedConfig speedConfig)
        {
            // Lấy đường dẫn file input
            Console.WriteLine(lblInpShpFile);
            GetShpPathInput();
            if (!HasShpPathInput())
            {
                Console.WriteLine(lblNoPath);
                return false;
            }

            // Lấy đường dẫn file output
            Console.WriteLine(lblOutpShpFile);
            GetDirectoryOutput();

            Console.Write(lblInProcess);
            using (var progress = new ProgressBar())
            {
                int iProcess = 0;
                // Đọc dữ liệu từ shape file
                if (!ReadDataFromShpFile(speedConfig))
                {
                    Console.WriteLine(lblReadFileFl);
                    return false;
                }

                iProcess += 10;//10
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);

                // Enter để tạo dữ liệu từ shape file để cập nhật vận tốc giới hạn từ Cơ sở dữ liệu
                if (!CreateSpeedTable())
                {
                    Console.WriteLine(lblOutpCreateDataFromShpFile + lblSpace + lblFail);
                    return false;
                }

                iProcess += 10;//20
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);

                // Enter để Cập nhật vận tốc giới hạn sau khi detect
                if (!GetSpeedLimitFromSpeedTable(speedConfig))
                {
                    Console.WriteLine(lblOutpUpdSpeedAfDetect + lblSpace + lblFail);
                    return false;
                }

                iProcess += 20;//40
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);

                // Cập nhật vận tốc giới hạn từ Cơ sở dữ liệu, input là danh sách các điểm đọc từ shape file
                if (!GetSpeedLimitFromSpeedTable(speedConfig))
                {
                    Console.WriteLine(lblOutpUpdSpeedAfDetect + lblSpace + lblFail);
                    return false;
                }

                iProcess += 30;//70
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);

                // Phân tích dữ liệu để tạo shape file dạng point hay polyline
                // Enter để Phân tích dữ liệu để tạo shape file dạng point hay polyline
                
                if (!GetDataCreateShpFile())
                {
                    Console.WriteLine(lblOutpAnylisData + lblSpace + lblFail);
                    return false;
                }

                iProcess += 10;//80
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);

                // Enter để tiến hành tạo file shape file
                if (!WriteShpFile())
                {
                    Console.WriteLine(lblOutpWriteShpFile + lblSpace + lblFail);
                    return false;
                }

                iProcess += 10;//90
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);

                iProcess += 5;//95
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);

                iProcess += 5;//100
                progress.Report((double)iProcess / 100);
                Thread.Sleep(20);
                //return true;
            }
            Console.WriteLine();
            Console.WriteLine(lblSuccess);
            Console.WriteLine(@"Enter để kết thúc!");
            //Console.WriteLine("Done.");
            return true;

            //// Đọc dữ liệu từ shape file
            //Console.WriteLine(lblReadFile);
            //if (!ReadDataFromShpFile())
            //{
            //    Console.WriteLine(lblReadFileFl);
            //    return false;
            //}
            //Console.WriteLine(lblReadFileSc);

            //// Enter để tạo dữ liệu từ shape file để cập nhật vận tốc giới hạn từ Cơ sở dữ liệu
            //Console.WriteLine(lblOutpCreateDataFromShpFileFromDB);
            //Console.ReadLine();
            //Console.WriteLine(lblInProcess);

            //if (!CreateSpeedTable())
            //{
            //    Console.WriteLine(lblOutpCreateDataFromShpFile + lblSpace + lblFail);
            //    return false;
            //}
            //Console.WriteLine(lblOutpCreateDataFromShpFile + lblSpace + lblSuccess);

            //// Enter để Cập nhật vận tốc giới hạn sau khi detect
            //Console.WriteLine(lblOutpUpdSpeedAfDetectFromDB);
            //Console.ReadLine();
            //Console.WriteLine(lblInProcess);

            //// Cập nhật vận tốc giới hạn từ Cơ sở dữ liệu, input là danh sách các điểm đọc từ shape file
            //if (!GetSpeedLimitFromSpeedTable())
            //{
            //    Console.WriteLine(lblOutpUpdSpeedAfDetect + lblSpace + lblFail);
            //    return false;
            //}
            //Console.WriteLine(lblOutpUpdSpeedAfDetect + lblSpace + lblSuccess);

            //// Phân tích dữ liệu để tạo shape file dạng point hay polyline
            //// Enter để Phân tích dữ liệu để tạo shape file dạng point hay polyline
            //Console.WriteLine(lblOutpAnylisDataPointOrPolyline);
            //Console.ReadLine();
            //Console.WriteLine(lblInProcess);
            //if (!GetDataCreateShpFile())
            //{
            //    Console.WriteLine(lblOutpAnylisData + lblSpace + lblFail);
            //    return false;
            //}
            //Console.WriteLine(lblOutpAnylisData + lblSpace + lblSuccess);

            //// Enter để tiến hành tạo file shape file
            //Console.WriteLine(lblOutpWriteShpFileProcess);
            //Console.ReadLine();
            //Console.WriteLine(lblInProcess);
            //if (!WriteShpFile())
            //{
            //    Console.WriteLine(lblOutpWriteShpFile + lblSpace + lblFail);
            //    return false;
            //}
            //Console.WriteLine(lblOutpWriteShpFile + lblSpace + lblSuccess);

            //return true;
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

            if (!File.Exists(shpPathInput))
                return false;

            return true;
        }

        private static void GetDirectoryOutput()
        {
            Console.Write(lblOutpFolder);
            directoryOutput = Console.ReadLine();

            if (!Directory.Exists(directoryOutput))
                Directory.CreateDirectory(directoryOutput);
        }

        private static bool ReadDataFromShpFile(SpeedConfig speedConfig)
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

                    MultiPolyLine? multiPolyLine = fe.Geometry.BasicGeometry as MultiPolyLine;

                    if (multiPolyLine == null)
                    {
                        Console.WriteLine(lblNoPolylineInShpFile);
                        return false;
                    }

                    // Dang quy dinh cot 34 chưa SegmentID
                    //long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[34]);
                    string? colSegment = speedConfig.ColSegmendId;
                    if (string.IsNullOrEmpty(colSegment))
                        colSegment = ColSegmendId;

                    long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[Convert.ToInt32(colSegment)]);

                    foreach (var line in multiPolyLine.PolyLines)
                    {
                        for (int i = 0; i < line.Points.Count(); i++)
                        {
                            string position;
                            if (i == 0)
                            {
                                position = $"S-{segmentID}";
                            }
                            else if (i == line.Points.Count() - 1)
                            {
                                position = $"E-{segmentID}";
                            }
                            else
                            {
                                position = $"M-{segmentID}-{i - 1}";
                            }

                            lstSpeed.Add(new SpeedProviderUpLoadVm()
                            {
                                Lat = Math.Round((decimal)line.Points[i].Y, precisions),
                                Lng = Math.Round((decimal)line.Points[i].X, precisions),
                                Position = position,
                                ProviderType = 1,
                                SegmentID = segmentID
                            });

                        }

                        #region method 1
                        //// Thêm điểm đầu
                        //lstSpeed.Add(new SpeedProviderUpLoadVm()
                        //{
                        //    Lat = Math.Round((decimal)line.Points[0].Y, precisions),
                        //    Lng = Math.Round((decimal)line.Points[0].X, precisions),
                        //    Position = $"S-{segmentID}", ProviderType = 1, SegmentID = segmentID
                        //});

                        //// Thêm điểm cuối
                        //lstSpeed.Add(new SpeedProviderUpLoadVm()
                        //{
                        //    Lat = Math.Round((decimal)line.Points[line.Points.Count() - 1].Y, precisions),
                        //    Lng = Math.Round((decimal)line.Points[line.Points.Count() - 1].X, precisions),
                        //    Position = $"E-{segmentID}", ProviderType = 1, SegmentID = segmentID
                        //});

                        //// Mặc định lấy 2 điểm ở bên trong, tính ra khoảng cách rồi mình chia làm 3 đoạn
                        //float partPoint = (CalculateDistance(line.Points[0].Y, line.Points[0].X
                        //    , line.Points[line.Points.Count() - 1].Y, line.Points[line.Points.Count() - 1].X) / 2);

                        //ClsPoint first = new ClsPoint() { Latitude = line.Points[0].Y, Longitude = line.Points[0].X };
                        //ClsPoint second = new ClsPoint() { Latitude = line.Points[line.Points.Count() - 1].Y, Longitude = line.Points[line.Points.Count() - 1].X };
                        //List<ClsPoint> lstPoint = GetVituralPoint(first, second, (int)partPoint);

                        //// Thêm ở giữa 2 vị trí
                        //lstSpeed.Add(new SpeedProviderUpLoadVm()
                        //{
                        //    Lat = Math.Round((decimal)lstPoint[0].Latitude, precisions),
                        //    Lng = Math.Round((decimal)lstPoint[0].Longitude, precisions),
                        //    Position = $"M-{segmentID}-{0}",
                        //    ProviderType = 1,
                        //    MinSpeed = 1,
                        //    MaxSpeed = 1,
                        //    SegmentID = segmentID
                        //});

                        //lstSpeed.Add(new SpeedProviderUpLoadVm()
                        //{
                        //    Lat = Math.Round((decimal)lstPoint[lstPoint.Count() - 1].Latitude, precisions),
                        //    Lng = Math.Round((decimal)lstPoint[lstPoint.Count() - 1].Longitude, precisions),
                        //    Position = $"M-{segmentID}-{1}",
                        //    ProviderType = 1,
                        //    SegmentID = segmentID
                        //});

                        //segmentID = 0;

                        #endregion
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
                    return false;

                speedTable = Common.Common.CreateTableSpeedLimit3Point();
                foreach (SpeedProviderUpLoadVm item in lstSpeed)
                {
                    speedTable.Rows.Add(item.Lat, item.Lng, item.ProviderType, item.Position,
                        0, 0,
                        false, item.SegmentID, false, DateTime.Now, null, "UploadFile", null, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(lblErr + ex.ToString());
                return false;
            }

            return true;
        }

        private static bool GetSpeedLimitFromSpeedTable(SpeedConfig speedConfig)
        {
            try
            {
                if (speedTable == null || speedTable.Rows.Count < 0)
                    return false;

                lstSpeedFromDb = new List<SpeedProviderUpLoadVm>();
                lstSegmentID = new List<long>();

                SqlConnection con;
                // Kết nối Cơ sở dữ liệu
                string? connString = speedConfig.DataConnection;
                string? spGetGetSpeedLimitFromSpeedTable = speedConfig.SpGetGetSpeedLimitFromSpeedTable;
                string? spGetGetSpeedLimitFromSpeedTableParamTable = speedConfig.SpGetGetSpeedLimitFromSpeedTableParamTable;

                if (string.IsNullOrEmpty(connString))
                    connString = connStrDev;

                con = new SqlConnection(connString);
                con.Open();

                if (string.IsNullOrEmpty(spGetGetSpeedLimitFromSpeedTable))
                    spGetGetSpeedLimitFromSpeedTable = SpGetGetSpeedLimitFromSpeedTable;

                if (string.IsNullOrEmpty(spGetGetSpeedLimitFromSpeedTableParamTable))
                    spGetGetSpeedLimitFromSpeedTableParamTable = SpGetGetSpeedLimitFromSpeedTableParamTable;

                SqlCommand cmd = new SqlCommand(spGetGetSpeedLimitFromSpeedTable, con);//"[dbo].[Get_GetSpeedLimitFromSpeedTable]"
                cmd.CommandType = CommandType.StoredProcedure;

                // Pass table Valued parameter to Store Procedure
                SqlParameter sqlParam = cmd.Parameters.AddWithValue(spGetGetSpeedLimitFromSpeedTableParamTable, speedTable);// @SpeedLimit
                sqlParam.SqlDbType = SqlDbType.Structured;

                // Executing the SQL query
                SqlDataReader sdr = cmd.ExecuteReader();
                //Looping through each record
                SpeedProviderUpLoadVm itemAdd;
                while (sdr.Read())
                {
                    itemAdd = new SpeedProviderUpLoadVm();
                    itemAdd.Lat = Convert.ToDecimal(sdr[0].ToString());
                    itemAdd.Lng = Convert.ToDecimal(sdr[1].ToString());
                    itemAdd.ProviderType = Convert.ToInt32(sdr[2].ToString());
#pragma warning disable CS8601 // Possible null reference assignment.
                    itemAdd.Position = sdr[3].ToString();
#pragma warning restore CS8601 // Possible null reference assignment.
                    itemAdd.MinSpeed = Convert.ToInt32(sdr[4].ToString());
                    itemAdd.MaxSpeed = Convert.ToInt32(sdr[5].ToString());
                    itemAdd.PointError = false;
                    itemAdd.SegmentID = Convert.ToInt64(sdr[7].ToString());

                    // Lấy danh sách những SegmengId có vận tốc giới hạn trong Cơ sở dữ liệu
                    if (!lstSegmentID.Where(x => x == itemAdd.SegmentID).Any())
                        lstSegmentID.Add(itemAdd.SegmentID);

                    lstSpeedFromDb.Add(itemAdd);
                }

                //cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(lblErr + ex);
                return false;
            }

            return true;
        }

        private static bool GetDataCreateShpFile()
        {
            try
            {
                if (lstSpeedFromDb == null)
                    return false;

                lstCreatePolyline = new List<SpeedProviderUpLoadVm>();
                lstSegmentIDLine = new List<long>();
                lstCreatePoint = new List<SpeedProviderUpLoadVm>();
                List<SpeedProviderUpLoadVm> lstAdd;
                SpeedProviderUpLoadVm lineS;
                SpeedProviderUpLoadVm lineE;
                List<SpeedProviderUpLoadVm> lstLineM;

                for (int i = 0; i< lstSegmentID.Count; i++)
                {
                    lstAdd = lstSpeedFromDb.Where(x => x.SegmentID == lstSegmentID[i]).ToList();

                    lineS = lstAdd.Where(x => x.Position.Trim() == $"S-{lstSegmentID[i]}")
                       .FirstOrDefault();
                    lineE = lstAdd.Where(x => x.Position.Trim() == $"E-{lstSegmentID[i]}")
                       .FirstOrDefault();

                    if (lineS == null || lineE == null)
                        continue;

                    // So sánh xem trong 1 segment có bị thay đổi vận tốc ở đầu cuối không
                    if ((lineS.MinSpeed == lineE.MinSpeed) && (lineS.MaxSpeed == lineE.MaxSpeed))
                    {
                        lstCreatePolyline.Add(lineS);
                        lstCreatePolyline.Add(lineE);
                        lstSegmentIDLine.Add(lstSegmentID[i]);
                    }
                    else
                    {
                        lstCreatePoint.AddRange(lstAdd);
                        #region method 1
                        //lstLineM = new List<SpeedProviderUpLoadVm>();
                        //lstLineM = lstAdd.Where(x => x.SegmentID == lstSegmentID[i]
                        //&& x.Position != $"S-{lstSegmentID[i]}"
                        //&& x.Position != $"E-{lstSegmentID[i]}").ToList();
                        //lstCreatePoint.AddRange(lstLineM);
                        #endregion
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

        private static bool WriteShpFile()
        {
            if (!WritePointShpFile())
                return false;

            if (!WritePolylineShpFile())
                return false;

            return true;
        }

        private static bool WritePointShpFile()
        {
            try
            {
                // Kiểm tra xem có danh sách các điểm cần vẽ Point không
                if (lstCreatePoint == null || (!lstCreatePoint.Any()))
                   return true;

                string fileName = @"Out_Point_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString()
                + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();

                IFeatureSet fs = new FeatureSet(FeatureType.Point);

                var dataTable = new DataTable();
                dataTable.Columns.Add("SegmentID", typeof(long));
                dataTable.Columns.Add("X", typeof(double));//Lat
                dataTable.Columns.Add("Y", typeof(double));//Long
                dataTable.Columns.Add("MinSpeed", typeof(int));
                dataTable.Columns.Add("MaxSpeed", typeof(int));
                dataTable.Columns.Add("Position", typeof(string));

                for (int i = 0; i < lstCreatePoint.Count; i++)
                {
                    var point = new GeoPoint((double)Math.Round(lstCreatePoint[i].Lng, precisions), (double)Math.Round(lstCreatePoint[i].Lat, precisions));
                    var feature = new Feature(new Geometry(point));
                    fs.Features.Add(feature);

                    var row = dataTable.NewRow();
                    row[0] = lstCreatePoint[i].SegmentID;//SegmentID
                    row[1] = lstCreatePoint[i].Lat;
                    row[2] = lstCreatePoint[i].Lng;
                    row[3] = lstCreatePoint[i].MinSpeed;//MinSpeed
                    row[4] = lstCreatePoint[i].MaxSpeed;//MaxSpeed
                    row[5] = lstCreatePoint[i].Position;

                    dataTable.Rows.Add(row);
                }

                fs.Crs = Crs.Wgs84Gcs;
                fs.AttrTable = dataTable;

                var fileBytes = fs.GetShapeFileBytes();

                using var shpFile = new FileStream(directoryOutput + @"\" + fileName + ".shp", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var shxFile = new FileStream(directoryOutput + @"\" + fileName + ".shx", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var dbfFile = new FileStream(directoryOutput + @"\" + fileName + ".dbf", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var prjFile = new FileStream(directoryOutput + @"\" + fileName + ".prj", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                shpFile.Write(fileBytes.ShpBytes);
                shxFile.Write(fileBytes.ShxBytes);
                dbfFile.Write(fileBytes.DbfBytes);
                prjFile.Write(fileBytes.PrjBytes);

            }
            catch (Exception ex)
            {
                Console.WriteLine(lblErr + ex.ToString());
                return false;
            }

            return true;
        }

        private static bool WritePolylineShpFile()
        {
            try
            {
                if (lstSegmentIDLine == null || (!lstSegmentIDLine.Any()))
                    return true;

                string fileName = @"Out_Line_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString()
                + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();

                IFeatureSet fs = new FeatureSet(FeatureType.PolyLine);

                SpeedProviderUpLoadVm lineS;
                SpeedProviderUpLoadVm lineE;

                var dataTable = new DataTable();
                dataTable.Columns.Add("SegmentID", typeof(long));
                //dataTable.Columns.Add("X", typeof(double));//Lat
                //dataTable.Columns.Add("Y", typeof(double));//Long
                dataTable.Columns.Add("MinSpeed", typeof(int));
                dataTable.Columns.Add("MaxSpeed", typeof(int));
                //dataTable.Columns.Add("Position", typeof(string));


                for (int i = 0; i < lstSegmentIDLine.Count; i++)
                {
                    lineS = lstCreatePolyline.Where(x => x.SegmentID == lstSegmentIDLine[i] && x.Position == $"S-{lstSegmentIDLine[i]}")
                        .FirstOrDefault();
                    lineE = lstCreatePolyline.Where(x => x.SegmentID == lstSegmentIDLine[i] && x.Position == $"E-{lstSegmentIDLine[i]}")
                       .FirstOrDefault();
                    var pointS = new GeoPoint((double)Math.Round(lineS.Lng, precisions), (double)Math.Round(lineS.Lat, precisions));
                    var pointE = new GeoPoint((double)Math.Round(lineE.Lng, precisions), (double)Math.Round(lineE.Lat, precisions));

                    //line ring
                    var polyLine = new PolyLine(new List<GeoPoint>() { pointS, pointE });
                    var isLineRing = polyLine.IsLineRing;

                    var feature = new Feature(new Geometry(polyLine));
                    fs.Features.Add(feature);

                    var row = dataTable.NewRow();
                    row[0] = lineS.SegmentID;//SegmentID
                    //row[1] = 0;
                    //row[2] = 0;
                    row[1] = lineS.MinSpeed;//MinSpeed
                    row[2] = lineS.MaxSpeed;//MaxSpeed
                    //row[3] = string.Empty;

                    dataTable.Rows.Add(row);
                }

                fs.Crs = Crs.Wgs84Gcs;
                fs.AttrTable = dataTable;

                var fileBytes = fs.GetShapeFileBytes();

                using var shpFile = new FileStream(directoryOutput + @"\" + fileName + ".shp", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var shxFile = new FileStream(directoryOutput + @"\" + fileName + ".shx", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var dbfFile = new FileStream(directoryOutput + @"\" + fileName + ".dbf", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using var prjFile = new FileStream(directoryOutput + @"\" + fileName + ".prj", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                shpFile.Write(fileBytes.ShpBytes);
                shxFile.Write(fileBytes.ShxBytes);
                dbfFile.Write(fileBytes.DbfBytes);
                prjFile.Write(fileBytes.PrjBytes);
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
