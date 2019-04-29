using Npgsql;
using Postgre_Backup_Restore.ParamaterManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Postgre_Backup_Restore.DBEntitiy
{
    public class DBUtils
    {
        /***************
        * C:\Program Files\PostgreSQL\9.5\data\pg_hba.conf(PostgreSQL Client Authentication Configuration File)
        * 
        * Alttaki Satırlari Dosyaya Ekle:
        * 
        * # IPv4 allowed client connection:
        * host    all				all  			192.168.1.0/24       	md5
        * 
        * 
        ****************/
        private static string userID = Parameters.Params.User;
        private static string pass = Parameters.Params.Password;
        private static string dbase = Parameters.Params.Base;
        private static string host = Parameters.Params.Host;
        private static string port = Parameters.Params.Port;
        private static NpgsqlConnection conn;

        public static EntityConnectionStringBuilder BuildConnectionString()
        {
            //if (conn == null)
            //{
            //    DBCreateDummyConnection();
            //}
            //DBConCheck();
            string providerName = "System.Data.EntityClient";
            string metadata = @"res://*/TesaEntityModel.csdl|res://*/TesaEntityModel.ssdl|res://*/TesaEntityModel.msl";
            string provider = "Npgsql";
            //host = "localhost";
            //host = "192.168.1.153";
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            entityBuilder.Provider = providerName;
            entityBuilder.Metadata = metadata;
            entityBuilder.Provider = provider;
            entityBuilder.ProviderConnectionString =
                "PORT=" + port +
                ";TIMEOUT=15;POOLING=True;MINPOOLSIZE=1;MAXPOOLSIZE=20;COMMANDTIMEOUT=20;COMPATIBLE=2.2.5.0" +
                ";USER ID=" + userID +
                ";PASSWORD=" + pass +
                ";DATABASE=" + dbase +
                ";HOST=" + host +
                ";";
            return entityBuilder;
        }

        private static void DBCreateDummyConnection()
        {
            try
            {
                // PostgeSQL-style connection string
                string connstring = String.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};",
                    host, "5432", userID,
                    pass, dbase);
                // Making connection with Npgsql provider
                conn = new NpgsqlConnection(connstring);
                conn.StateChange += conn_StateChange;
                conn.Notice += conn_Notice;
                conn.Notification += conn_Notification;
                conn.Open();
            }
            catch (Exception e)
            {
                Trace.WriteLine("DBConCheck err : " + e.Message);
            }
        }

        private static void DBConCheck()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();

                // quite complex sql statement
                string sql = "SELECT 'test'";
                // data adapter making request from our connection
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                // i always reset DataSet before i do
                // something with it.... i don't know why :-)
                ds.Reset();
                // filling DataSet with result from NpgsqlDataAdapter
                da.Fill(ds);
                // since it C# DataSet can handle multiple tables, we will select first
                dt = ds.Tables[0];
                // connect grid to DataTable
                // since we only showing the result we don't need connection anymore
                //conn.Close();

            }
            catch (Exception e)
            {
                Trace.WriteLine("DBConCheck err : " + e.Message);
            }
        }


        static void conn_Notification(object sender, NpgsqlNotificationEventArgs e)
        {
            Trace.WriteLine("conn_Notification");
        }

        private static void conn_Notice(object sender, NpgsqlNoticeEventArgs e)
        {
            Trace.WriteLine("conn_Notice");
        }

        private static void conn_StateChange(object sender, StateChangeEventArgs e)
        {
            Trace.WriteLine("conn_StateChange");
        }
    }
}
