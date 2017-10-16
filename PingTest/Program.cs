using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingTest
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static void Main(string[] args)
        {
            // logging
            log4net.Config.XmlConfigurator.Configure();

            SqlConnection conn = CreateConnection();

            var bgWork = new BackgroundWorker();
            bgWork.WorkerSupportsCancellation = true;
            bgWork.DoWork += (s, e) =>
            {
                while (!bgWork.CancellationPending)
                {

                    using (var cmd = new SqlCommand("SELECT * FROM MOCK_DATA", conn))
                    {

                        var a = new SqlDataAdapter(cmd);

                        var ds = new DataSet();

                        var sw = Stopwatch.StartNew();

                        a.Fill(ds);

                        sw.Stop();

                        var logMessage = "query executed in " + sw.Elapsed.ToString();
                        Log(logMessage);

                    }

                    Thread.Sleep(5000);

                }

            };
            bgWork.RunWorkerCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    Log("ERROR: " + e.Error.Message, true);
                }
                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                }
                if (e.Error != null)
                {
                    conn = CreateConnection();
                    bgWork.RunWorkerAsync();
                };

            };

            bgWork.RunWorkerAsync();

            Console.ReadKey();
            bgWork.CancelAsync();

            Thread.Sleep(5000);

        }

        private static void Log(string logMessage, bool isError = false)
        {
            Console.WriteLine(logMessage);
            if (isError) {
                Logger.Error(logMessage);
            }
            else
            {
                Logger.Info(logMessage);
            }
        }

        private static SqlConnection CreateConnection()
        {
            do
            {
                try
                {
                    Console.WriteLine("connecting");
                    //var connectionString = @"server = ACCESS-1303SF2\SQL2014 ; database = ping ; user id = sa ; pwd = Patrick@1";
                    var connectionString = ConfigurationManager.ConnectionStrings["pingDB"].ConnectionString;
                    var conn = new SqlConnection(connectionString);
                    conn.Open();
                    Console.WriteLine("connection successful");
                    return conn;
                }
                catch (Exception)
                {
                    Console.WriteLine("connection failed");
                    Thread.Sleep(1000);
                }
            } while (true);
        }

    }
}
