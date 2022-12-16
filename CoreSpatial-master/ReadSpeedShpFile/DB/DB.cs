using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using static ReadSpeedShpFile.Common.Strings;

namespace ReadSpeedShpFile.DB
{
    public class DB
    {
        static SqlConnection conn;

        public static void OpenConnection()
        {
            conn = new SqlConnection(connStr);
            conn.Open();
        }

        public static void CloseConnection()
        {
            conn.Close();
        }

        public static int ExecuteQueries(string Query_)
        {
            SqlCommand cmd = new SqlCommand(Query_, conn);
            int ctr = cmd.ExecuteNonQuery();
            return ctr;
        }
    }
}
