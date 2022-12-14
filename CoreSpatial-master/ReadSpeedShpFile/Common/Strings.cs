using System;

namespace ReadSpeedShpFile.Common
{
    class Strings
    {
        // SpeedWebAPI
        public static readonly string connStr = @"Data Source=NC-CHANHNV\MSSQLSERVER2;Initial Catalog=SpeedWebAPI;Persist Security Info=True;User ID=sa;Password=1";
        public static readonly string connStrDev = @"Data Source=NC-CHANHNV\MSSQLSERVER2;Initial Catalog=SpeedWebAPI;Persist Security Info=True;User ID=sa;Password=1";

        public static readonly string ColSegmendId = @"34";
        public static readonly string SpInsSpeedLimit = @"[dbo].[Ins_SpeedLimit]";
        public static readonly string SpInsSpeedLimitParamTable = @"@SpeedLimit";

        public static readonly string SpGetGetSpeedLimitFromSpeedTable = @"[dbo].[Get_GetSpeedLimitFromSpeedTable]";
        public static readonly string SpGetGetSpeedLimitFromSpeedTableParamTable = @"@SpeedLimit";

        public static string fileNameOut = "Out" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString()
                + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();

        //public static string titleString = "\t\t\t\t..........Speed Limit Management System..........\n";
        public static string titleString = "\t\t\t\t..........TOOL CẬP NHẬT VẬN TỐC GIỚI HẠN..........\n";
        public static string titleString_EN = "\t\t\t\t..........Speed Limit Management System..........\n";

        public static string lblChoose1 = @">>Chọn 1: Cập nhật dữ liệu từ shape file vào Cơ sở dữ liệu";
        public static string lblChoose1_EN = @">>Choose 1: Create data Speed Limit to Database";

        public static string lblChoose2 = @">>Chọn 2: Cập nhật vận tốc giới hạn từ shape file, output là shape file";
        public static string lblChoose2_EN = @">>Choose 2: Export data Speed Limit to shpfile, output là shape file";

        public static string lblChoose1Or2 = "\n>>Vui lòng chọn 1 hoặc 2: ";
        public static string lblChoose1Or2_EN = "\n>>Choose 1 or 2, please: ";

        public static string lblInpShpFile = @">>Nhập đường dẫn đến shape file  (ví dụ: E:\File\speedlimit.shp)";
        public static string lblInpShpFile_EN = @">>Please Input Path shape file  (ex: E:\File\speedlimit.shp)";

        public static string lblOutpShpFile = @">>Nhập đường dẫn đấn thư mục chứa file out (ví dụ: E:\File)";
        public static string lblOutpShpFile_EN = @">>Input Directory Out put file (ex: E:\File)";

        public static string lblNoPath = @">>Đường dẫn không đúng!";
        public static string lblNoPath_EN = @">>path does not exist!";

        public static string lblReadFile = @">>Đang đọc file..........................................";
        public static string lblReadFile_EN = @">>Reading file..........................................";

        public static string lblReadFileSc = @">>Đọc file thành công";
        public static string lblReadFileSc_EN = @">>Đọc file thành công";

        public static string lblReadFileFl = @">>Đọc file thất bại";
        public static string lblReadFileFl_EN = @">>Read file fail";

        public static string lblInProcess = @">>đang tiến hành....";
        public static string lblInProcess_EN = ">>in progress....";

        public static string lblSuccess = "thành công";
        public static string lblSuccess_EN = "success";

        public static string lblFail = "thất bại";
        public static string lblFail_EN = "fail";

        public static string lblSpace = " ";

        public static string lblErr = @">>Lỗi: ";
        public static string lblErr_EN = ">>Error: ";

        public static string lblPathFileIs = "\n>>Đường dẫn đến shape file là: ";
        public static string lblPathFileIs_EN = "\n>>Shape file Path: ";

        public static string lblDotShp = ".shp";

        public static string lblDatanoPolyline = @">>Dữ liệu không phải là Polyline";
        public static string lblDatanoPolyline_EN = ">>Data is not PolyLine";

        public static string lblNoPolylineInShpFile = @">>Không có dữ liệu dạng Polyline trong shape file";
        public static string lblNoPolylineInShpFile_EN = ">>No have PolyLine in shape file";

        public static string lblUpdFile = ".shp";

        #region Read file
        public static string lblInpCreateDataFromShpFile = @">>Nhập Enter để tạo dữ liệu từ shape file để cập nhật vào Cơ sở dữ liệu";
        public static string lblInpCreateDataFromShpFile_EN = @">>Enter to Create Data from shape file to Create data to Database";

        public static string lblInpCreateDataFromShpFileToDb = @">>Tạo dữ liệu từ shape file vào Cơ sở dữ liệu";
        public static string lblInpCreateDataFromShpFileToDb_EN = @">>Tạo dữ liệu từ shape file vào Cơ sở dữ liệu";

        public static string lblInpUpdDataToDBProcess = @">>Nhập Enter để tiến hành cập nhật dữ liệu vào Cơ sở dữ liệu";
        public static string lblInpUpdDataToDBProcess_EN = @">>Enter to Update data to Database";

        public static string lblInpUpdDataToDB = @">>Cập nhật dữ liệu vào Cơ sở dữ liệu";
        public static string lblInpUpdDataToDB_EN = @">>Update data to Database";
        #endregion

        #region Write file
        public static string lblOutpCreateDataFromShpFileFromDB = @">>Nhập Enter để tạo dữ liệu từ shape file để cập nhật vận tốc giới hạn từ Cơ sở dữ liệu";
        public static string lblOutpCreateDataFromShpFileFromDB_EN = ">>Enter to Create data to Create data to Database to Update Speed";

        public static string lblOutpCreateDataFromShpFile = @">>Tạo dữ liệu từ shape file để cập nhật vận tốc giới hạn từ Cơ sở dữ liệu";
        public static string lblOutpCreateDataFromShpFile_EN = @">>Tạo dữ liệu từ shape file để cập nhật vận tốc giới hạn từ Cơ sở dữ liệu";

        public static string lblOutpUpdSpeedAfDetectFromDB = @">>Nhập Enter để Cập nhật vận tốc giới hạn sau khi detect";
        public static string lblOutpUpdSpeedAfDetectFromDB_EN = @">>Enter to Update Speed Limit after detect";

        public static string lblOutpUpdSpeedAfDetect = @">>Cập nhật vận tốc giới hạn sau khi detect";
        public static string lblOutpUpdSpeedAfDetect_EN = @">>Cập nhật vận tốc giới hạn sau khi detect";

        public static string lblOutpAnylisDataPointOrPolyline = @">>Nhập Enter để Phân tích dữ liệu để tạo shape file dạng point hay polyline";
        public static string lblOutpAnylisDataPointOrPolyline_EN = @">>Enter to Analysis Data to create Point or Polyline";

        public static string lblOutpAnylisData = @">>Phân tích dữ liệu";
        public static string lblOutpAnylisData_EN = @">>Analysis Data";

        public static string lblOutpWriteShpFileProcess = @">>Nhập Enter để tiến hành tạo file shape file";
        public static string lblOutpWriteShpFileProcess_EN = @">>Enter to Write Shaple File";

        public static string lblOutpWriteShpFile = @">>Tạo file shape file";
        public static string lblOutpWriteShpFile_EN = @">>Write Shaple File";

        public static string lblOutpFolder = "\n>>Thư mục chứa file output là: ";
        public static string lblOutpFolder_EN = "\n>>Directory: ";
        #endregion
    }
}
