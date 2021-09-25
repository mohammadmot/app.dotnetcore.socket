using System;
using System.IO;
using System.Threading;

using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace code.socket
{
    class Program
    {
        #region helper
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        #endregion

        static void Main(string[] args)
        {







            var configBuilder = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json"), optional: true, reloadOnChange: true);

            var config = configBuilder.Build();
            var LoggerType = config["LoggerConfig:LoggerType"];
            var ApplicationId = config["LoggerConfig:ApplicationId"];
            //var v = config.GetSection("LoggerConfig").GetChildren();
            /*var vChild = v.GetChildren();*/ //  ("LoggerType");
            Console.WriteLine($"Path> { Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json") }");
            LoggerConfig loggerConfig = new LoggerConfig();
            // Configure<LoggerConfig>(Configuration.GetSection("LoggerConfig"));

            // Check Type Of Logger
            //switch (loggerConfig.LoggerType)
            switch (LoggerType)
            {
                case "Serilog":
                    {
                        System.Diagnostics.Debug.WriteLine("> LoggerType=Serilog");
                        break;
                    }
                case "Log4Net":
                    {
                        System.Diagnostics.Debug.WriteLine("> LoggerType=Log4Net");
                        break;
                    }
                default:
                    {
                        System.Diagnostics.Debug.WriteLine("> LoggerType=default");
                        throw new Exception("#### LoggerType in appsettings.json is not valid.");
                    }
            }


            Console.WriteLine($"Path> { Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json") }");


            string host = "94.182.180.208"; // "127.0.0.1";
            int port = 1234;

            if (args.Length > 0)
            {
                if (args[0] != null)
                    host = args[0];
                if (args[1] != null && int.Parse(args[1]) > 0)
                    port = int.Parse(args[1]);
            }

            Console.WriteLine("Hello World!");

            ClientSocket Socket = new ClientSocket(OnReceivedBuffer, OnConnectionStatus);

            int nDataSendLen = 0;
            byte[] naDataSend = new byte[5 * 1024];

            int nTry = 0;
            while (true)
            {
                if (!Socket.IsConnected())
                {
                    Console.WriteLine($"Try {++nTry} to connect {host}:{port}");
                    Socket.Connect(host, port);
                }
                else
                {
                    if (nDataSendLen > 9)
                        nDataSendLen = 0;
                    
                    naDataSend[nDataSendLen] = (byte)(nDataSendLen+48);
                    Socket.SendData(naDataSend, nDataSendLen);

                    #region print
                    byte[] aMsg = new byte[nDataSendLen];
                    Array.Copy(naDataSend, aMsg, nDataSendLen);
                    Console.WriteLine($"SendData> {nDataSendLen} bytes: " + System.Text.Encoding.UTF8.GetString(aMsg));
                    // Array.Copy(naDataSend, 0, nDataSendLen);
                    #endregion

                    // Console.WriteLine($"SendData> {nDataSendLen} bytes: " + System.Text.Encoding.UTF8.GetString(naDataSend,nDataSendLen));

                    nDataSendLen++;
                }
                Thread.Sleep(1000);
            }
        }

        public static void OnReceivedBuffer(byte[] buffer, int nLen)
        {
            char[] ach = new char[nLen];
            for (int n = 0; n < nLen; n++)
                ach[n] = (char)buffer[n];
            string strMsg = new string(ach);
            Console.WriteLine("OnReceivedBuffer>Received: " + strMsg);
        }

        public static void OnConnectionStatus(bool bConnetionDone, string strMsg)
        {
            if (bConnetionDone == true)
            {
                Console.WriteLine("OnConnectionStatus>Success: " + strMsg);
            }
            else
            {
                Console.WriteLine("OnConnectionStatus>Fail: " + strMsg);
            }
        }
    }
}
