﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft;
using Microsoft.Data;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.Data.SqlClient.Server;
using Microsoft.Data.SqlTypes;
using Microsoft.Net;
using Microsoft.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.IIS.Core;

namespace Personal_Website.Back_End.DBFunctions
{
    public class SQLCrud
    {
        public static int InsertNewMessage(string name, string type, string message, DateTime date, string ip, string token, string mail = null)
        {
            SqlConnection conn = new SqlConnection(@"Server=tcp:jansafronov.database.windows.net,1433;Initial Catalog=Content;Persist Security Info=False;User ID=jansafr;Password=A42a24r22battle;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            // Open connection
            conn.Open();

            // Initialization
            SqlDataAdapter adapter;
            SqlCommand sqlcmd;

            DateTime dt = GetMaxDate(ip);

            if (dt != SqlDateTime.MinValue.Value)
            {
                bool trip = dt.Year != date.Year || dt.Month != date.Month || dt.Day != date.Day;
                bool limit = (date.Hour - dt.Hour) * 59 + date.Minute - dt.Minute - (date.Second - dt.Second >= 0 ? 0 : 1) >= 59;
                if (trip || limit)
                {
                    // Data adapter
                    adapter = new SqlDataAdapter();

                    // Build Sql command and insert into the adapter
                    sqlcmd = new SqlCommand("[dbo].[MessageInsert]", conn);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddRange(new SqlParameter[] { new SqlParameter("@name", name), new SqlParameter("@message", message + "postfixstore"), new SqlParameter("@mail", mail), new SqlParameter("@date", new SqlDateTime(date)), new SqlParameter("@ip", ip), new SqlParameter("@token", token) });
                    return sqlcmd.ExecuteNonQuery();
                    adapter.InsertCommand = sqlcmd;
                    return adapter.InsertCommand.ExecuteNonQuery();
                    /*adapter.InsertCommand.ExecuteReader();
                    adapter.SelectCommand.ExecuteReader();*/

                    conn.Close();

                    return 1;
                }

                conn.Close();

                return 0;
            }

            // Data adapter
            adapter = new SqlDataAdapter();

            // Build Sql command and insert into the adapter
            sqlcmd = new SqlCommand("[dbo].[MessageInsert]", conn);
            sqlcmd.CommandType = CommandType.StoredProcedure;
            sqlcmd.Parameters.AddRange(new SqlParameter[] { new SqlParameter("@name", name), new SqlParameter("@message", message + "postfixstore"), new SqlParameter("@mail", mail), new SqlParameter("@date", new SqlDateTime(date)), new SqlParameter("@ip", ip), new SqlParameter("@token", token) });
            return sqlcmd.ExecuteNonQuery();
            adapter.InsertCommand = sqlcmd;
            return adapter.InsertCommand.ExecuteNonQuery();
            /*adapter.InsertCommand = sqlcmd;
            adapter.InsertCommand.ExecuteNonQuery();*/

            // Close connection
            conn.Close();

            return 1;
        }

        public static int RemoveMessage(DateTime date, string ip)
        {
            // Connection to the data source
            SqlConnection conn = new SqlConnection(@"Server=tcp:jansafronov.database.windows.net,1433;Initial Catalog=Content;Persist Security Info=False;User ID=jansafr;Password=A42a24r22battle;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            // Open connection
            conn.Open();

            // Initialization
            SqlDataAdapter adapter;
            SqlCommand sqlcmd;

            DateTime dt = GetMaxDate(ip);

            if (dt != SqlDateTime.MinValue.Value)
            {
                bool trip = dt.Year != date.Year || dt.Month != date.Month || dt.Day != date.Day;
                bool limit = (date.Hour - dt.Hour) * 59 + date.Minute - dt.Minute - (date.Second - dt.Second >= 0 ? 0 : 1) >= 59;

                if (!trip && !limit)
                {
                    // Data adapter
                    adapter = new SqlDataAdapter();

                    // Build Sql command and insert into the adapter
                    sqlcmd = new SqlCommand("[dbo].[Delete]", conn);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddRange(new SqlParameter[] { new SqlParameter("@ip", ip), new SqlParameter("@date", dt) });
                    adapter.InsertCommand = sqlcmd;
                    adapter.InsertCommand.ExecuteNonQuery();

                    conn.Close();

                    return 1;
                }

                conn.Close();
            }

            return 0;
        }

        public static DateTime GetMaxDate(string ip)
        {
            // Connection to the data source
            SqlConnection conn = new SqlConnection(@"Server=tcp:jansafronov.database.windows.net,1433;Initial Catalog=Content;Persist Security Info=False;User ID=jansafr;Password=A42a24r22battle;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            // Open connection
            conn.Open();

            // Retrieve Messages data tables
            SqlCommand sqlcmd = new SqlCommand("[dbo].[UptoDate]", conn);
            sqlcmd.CommandType = CommandType.StoredProcedure;
            SqlDataReader sdr = sqlcmd.ExecuteReader();
            
            SqlDateTime sdtmax = SqlDateTime.MinValue.Value;
            
            if (!sdr.HasRows)
            {
                sdr.Close();
                return sdtmax.Value;
            }

            while (sdr.Read())
            {
                SqlDateTime sdt = sdr.GetSqlDateTime(1).Value;

                if (sdr.GetTextReader(0).ReadLine().Contains(ip) && SqlDateTime.GreaterThan(sdt, sdtmax).Value)
                {
                    sdtmax = sdt;
                }
            }

            sdr.Close();
            return sdtmax.Value;
        }

        public static string[] GetResources(string type) {

            // Connection to the data source
            SqlConnection conn = new SqlConnection(@"Server=tcp:jansafronov.database.windows.net,1433;Initial Catalog=Content;Persist Security Info=False;User ID=jansafr;Password=A42a24r22battle;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            
            // Open connection
            conn.Open();

            // Retrieve Messages data tables
            SqlCommand sqlcmd = new SqlCommand("[dbo].[ResourceSelect]", conn);
            sqlcmd.CommandType = CommandType.StoredProcedure;
            sqlcmd.Parameters.AddRange(new SqlParameter[] { new SqlParameter("@type", type) });
            SqlDataReader sdr = sqlcmd.ExecuteReader();

            List<string> resources = new List<string>();

            int i = 0;
            while (sdr.Read())
            {
                resources.Add(sdr.GetTextReader(1).ReadLine());
                i++;
            }

            sdr.Close();

            return resources.ToArray();
        }
    }
}
