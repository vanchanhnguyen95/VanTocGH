﻿using CoreSpatial;
using CoreSpatial.BasicGeometrys;
using CoreSpatial.CrsNs;
using ReadSpeedShpFile.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using static ReadSpeedShpFile.Common.CalculateGeo;
using static ReadSpeedShpFile.Common.Strings;

namespace ReadSpeedShpFile.Services
{
    class WriteShpFileService
    {
        private static string shpPathInput;
        private static string directoryOutput;
        private static List<SpeedProviderUpLoadVm> lstSpeed;
        private static List<SpeedProviderUpLoadVm> lstSpeedFromDb;
        private static List<long> lstSegmentID;
        private static List<long> lstSegmentIDLine;
        private static List<SpeedProviderUpLoadVm> lstCreatePoint;
        private static List<SpeedProviderUpLoadVm> lstCreatePolyline;

        private static DataTable speedTable;
        private static int precisions = 10;
        private static int colSegmendId = 34;

        public static bool CreateShpFileFromShpFile()
        {
            // Tiêu đề chương trình
            Console.WriteLine(titleString);

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
            Console.WriteLine(lblInProcess);

            // Đọc dữ liệu từ shape file
            Console.WriteLine(lblReadFile);
            if (!ReadDataFromShpFile())
            {
                Console.WriteLine(lblReadFileFl);
                return false;
            }
            Console.WriteLine(lblReadFileSc);

            // Enter để tạo dữ liệu từ shape file để cập nhật vận tốc giới hạn từ Cơ sở dữ liệu
            Console.WriteLine(lblOutpCreateDataFromShpFileFromDB);
            Console.ReadLine();
            Console.WriteLine(lblInProcess);

            if (!CreateSpeedTable())
            {
                Console.WriteLine(lblOutpCreateDataFromShpFile + lblSpace + lblFail);
                return false;
            }
            Console.WriteLine(lblOutpCreateDataFromShpFile + lblSpace + lblSuccess);

            // Ender để Cập nhật vận tốc giới hạn sau khi detect
            Console.WriteLine(lblOutpUpdSpeedAfDetectFromDB);
            Console.ReadLine();
            Console.WriteLine(lblInProcess);

            // Cập nhật vận tốc giới hạn từ Cơ sở dữ liệu, input là danh sách các điểm đọc từ shape file
            if (!GetSpeedLimitFromSpeedTable())
            {
                Console.WriteLine(lblOutpUpdSpeedAfDetect + lblSpace + lblFail);
                return false;
            }
            Console.WriteLine(lblOutpUpdSpeedAfDetect + lblSpace + lblSuccess);

            // Phân tích dữ liệu để tạo shape file dạng point hay polyline
            // Enter để Phân tích dữ liệu để tạo shape file dạng point hay polyline
            Console.WriteLine(lblOutpAnylisDataPointOrPolyline);
            Console.ReadLine();
            Console.WriteLine(lblInProcess);
            if (!GetDataCreateShpFile())
            {
                Console.WriteLine(lblOutpAnylisData + lblSpace + lblFail);
                return false;
            }
            Console.WriteLine(lblOutpAnylisData + lblSpace + lblSuccess);

            // Enter để tiến hành tạo file shape file
            Console.WriteLine(lblOutpWriteShpFileProcess);
            Console.ReadLine();
            Console.WriteLine(lblInProcess);
            if (!WriteShpFile())
            {
                Console.WriteLine(lblOutpWriteShpFile + lblSpace + lblFail);
                return false;
            }
            Console.WriteLine(lblOutpWriteShpFile + lblSpace + lblSuccess);

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

        private static void GetDirectoryOutput()
        {
            Console.Write(lblOutpFolder);
            directoryOutput = Console.ReadLine();

            if (!Directory.Exists(directoryOutput))
                Directory.CreateDirectory(directoryOutput);
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
                    //long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[34]);
                    long segmentID = Convert.ToInt64(fe.DataRow.ItemArray[colSegmendId]);

                    foreach (var line in multiPolyLine.PolyLines)
                    {
                        //float dis = CalculateDistance(line.Envelope.MinX, line.Envelope.MinY, line.Envelope.MaxX, line.Envelope.MaxY);
                        float dis = CalculateDistance(line.Points[0].X, line.Points[0].Y, line.Points[line.Points.Count() - 1].X, line.Points[line.Points.Count() - 1].Y);
                        // Thêm điểm đầu
                        lstSpeed.Add(new SpeedProviderUpLoadVm()
                        {
                            Lat = Math.Round((decimal)line.Points[0].Y, precisions),
                            Lng = Math.Round((decimal)line.Points[0].X, precisions),
                            Position = $"S-{segmentID}",
                            ProviderType = 1,
                            SegmentID = segmentID
                        });
                        // Thêm điểm cuối
                        lstSpeed.Add(new SpeedProviderUpLoadVm()
                        {
                            Lat = Math.Round((decimal)line.Points[line.Points.Count() - 1].Y, precisions),
                            Lng = Math.Round((decimal)line.Points[line.Points.Count() - 1].X, precisions),
                            Position = $"E-{segmentID}",
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
                    //Console.WriteLine("Have no data to create SpeedTable");
                    return false;

                speedTable = Common.Common.CreateTableSpeedLimit3Point();
                foreach (SpeedProviderUpLoadVm item in lstSpeed)
                {
                    speedTable.Rows.Add(item.Lat, item.Lng, item.ProviderType, item.Position,
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

        private static bool GetSpeedLimitFromSpeedTable()
        {
            try
            {
                if (speedTable == null || speedTable.Rows.Count < 0)
                    return false;

                lstSpeedFromDb = new List<SpeedProviderUpLoadVm>();
                lstSegmentID = new List<long>();

                SqlConnection con;
                //con = new SqlConnection(@"Data Source=NC-CHANHNV\MSSQLSERVER2;Initial Catalog=SpeedWebAPI;Persist Security Info=True;User ID=sa;Password=1");
                con = new SqlConnection(connStrDev);
                con.Open();

                SqlCommand cmd = new SqlCommand("[dbo].[Get_GetSpeedLimitFromSpeedTable]", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // Pass table Valued parameter to Store Procedure
                SqlParameter sqlParam = cmd.Parameters.AddWithValue("@SpeedLimit", speedTable);
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
                    itemAdd.Position = sdr[3].ToString();
                    itemAdd.MinSpeed = Convert.ToInt32(sdr[4].ToString());
                    itemAdd.MaxSpeed = Convert.ToInt32(sdr[5].ToString());
                    itemAdd.PointError = false;
                    itemAdd.SegmentID = Convert.ToInt64(sdr[7].ToString());

                    // Lấy danh sách những SegmengId có vận tốc giới hạn trong Cơ sở dữ liệu
                    if (!lstSegmentID.Where(x => x == itemAdd.SegmentID).Any())
                    {
                        lstSegmentID.Add(itemAdd.SegmentID);
                    }

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

                lstCreatePoint = new List<SpeedProviderUpLoadVm>();
                lstCreatePolyline = new List<SpeedProviderUpLoadVm>();
                List<SpeedProviderUpLoadVm> lstAdd;
                lstSegmentIDLine = new List<long>();

                for (int i = 0; i < lstSegmentID.Count; i++)
                {
                    lstAdd = lstSpeedFromDb.Where(x => x.SegmentID == lstSegmentID[i]).ToList();

                    // So sánh xem trong 1 segment có bị thay đổi vận tốc ở đầu cuối không
                    if (lstAdd.Any() && (lstAdd[0].MinSpeed == lstAdd[1].MinSpeed)
                        && (lstAdd[0].MaxSpeed == lstAdd[1].MaxSpeed))
                    {
                        lstCreatePolyline.AddRange(lstAdd);
                        lstSegmentIDLine.Add(lstSegmentID[i]);
                    }
                    else
                    {
                        lstCreatePoint.AddRange(lstAdd);
                    }

                    lstAdd = null;
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
            if(!WritePointShpFile())
                return false;

            if (!WritePolylineShpFile())
                return false;

            return true;
        }

        private static bool WritePointShpFile()
        {
            try
            {
                if (lstCreatePoint == null)
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
            catch(Exception ex)
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
                if (lstSegmentIDLine == null)
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
                    var polyLine = new PolyLine(new List<GeoPoint>(){ pointS, pointE });
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
            catch(Exception ex)
            {
                Console.WriteLine(lblErr + ex.ToString());
                return false;
            }
                
            return true;
        }

    }
}
