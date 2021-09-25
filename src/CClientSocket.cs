using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace code.socket
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ClientSocket
    {
        public ClientSocket(OnReceivedBuffer ReceivedBufferHandler, OnConnectionStatus ConnectionStatusHandler)
        {
            OnReceivedBufferDelegate += ReceivedBufferHandler;
            OnConnectionStatusDelegate += ConnectionStatusHandler;
        }
        // create a TCP/IP socket
        Socket ocClientSocket = null; // new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Thread ocClientThread = null;
        bool bRunReaderThread = true;
        byte[] naDataRead = new byte[5 * 1024 * 1024];

        #region Connection External Event
        public delegate void OnConnectionStatus(bool bConnetionDone, string strMsg);
        private event OnConnectionStatus OnConnectionStatusDelegate = null;
        #endregion

        #region Connection
        public void Connect(string strIP = "127.0.0.1", int nPort = 8080)
        {
            // Create a TCP/IP socket
            ocClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Start(strIP, nPort); // #### make it blocking mode

            /*
            Thread TR_client = new Thread(delegate(object unused) { Start(strIP, nPort); });
            // problem with Linux OS:
            // #### TR_client.SetApartmentState(ApartmentState.STA); // Additional information: The calling thread must be STA, because many UI components require this.
            TR_client.IsBackground = true;
            bRunReaderThread = true;
            TR_client.Start();
            */
        }

        public void Disconnect()
        {
            try
            {
                if (ocClientThread != null)
                {
                    bRunReaderThread = false;
                    ocClientThread.Abort();
                }

                if (ocClientSocket != null)
                {
                    // Disables sends and receives on a Socket
                    ocClientSocket.Shutdown(SocketShutdown.Both); // event to client that connection is terminated >>> int nLenDataRead = ocClientSocket.Receive(naDataRead) if (nLenDataRead == 0) connection_is_closed; <<<

                    // Closes the Socket connection and releases all associated resources
                    //   ___\\   For connection-oriented protocols, it is recommended that you call Shutdown before calling the Close method
                    //      //
                    ocClientSocket.Close();
                }
            }
            catch
            {

            }
        }

        public bool IsConnected()
        {
            try
            {
                return (ocClientSocket != null && ocClientSocket.IsBound && ocClientSocket.Connected); // !(ocClientSocket.Poll(1, SelectMode.SelectRead) && ocClientSocket.Available == 0) && ocClientSocket.IsBound);
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private void Start(string strIpAddress, int nPort)
        {
            try
            {
                Console.WriteLine($"Try to Connect endpoint {strIpAddress}:{nPort}");
                IPAddress ipAddress = IPAddress.Parse(strIpAddress);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, nPort);

                // connect to the remote endpoint (Sync)
                ocClientSocket.Connect(remoteEP);

                // begin connect to the remote endpoint (Async)
                // ocClientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), ocClientSocket);
                Console.WriteLine($"...");

                Console.WriteLine("Try to start [Receive thread]");
                ocClientThread = new Thread(delegate(object unused) { Receive(); });
                // problem with Linux OS:
                // #### ocClientThread.SetApartmentState(ApartmentState.STA); // Additional information: The calling thread must be STA, because many UI components require this.
                ocClientThread.IsBackground = true;
                ocClientThread.Start();
                Console.WriteLine($"...");

                // connection is done
                if (OnConnectionStatusDelegate != null)
                    OnConnectionStatusDelegate(true, "Connection established");
            }
            catch (Exception e)
            {
                Console.WriteLine("exception on [Start] -> " + e.Message);

                // No connection could be made because the target machine actively refused it 127.0.0.1:400
                // Cannot access a disposed object

                // connection is failed
                if (OnConnectionStatusDelegate != null)
                    OnConnectionStatusDelegate(false, e.Message);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("...");

                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                #region Complete the connection
                // EndConnect is a blocking method that completes the asynchronous remote host connection request started in the BeginConnect method
                client.EndConnect(ar); // Ends a pending asynchronous connection request
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("exception on [ConnectCallback] -> " + e.Message);
            }
        }
        #endregion

        #region received
        public delegate void OnReceivedBuffer(byte[] buffer, int nLen);
        public event OnReceivedBuffer OnReceivedBufferDelegate = null;

        private void Receive()
        {
            FileStream FileStreamLocal = null;
            try
            {
                // Create a new file stream where we will be saving the data (local drive)
                string strCurrentDirectory = Environment.CurrentDirectory;
                string strLogDirectory = "Log";
                string strLogFileReceived = strCurrentDirectory + "\\" + strLogDirectory + "\\" + "ReceivedLog.txt";

                //FileStreamLocal = new FileStream(strLogFileReceived, FileMode.Create, FileAccess.Write, FileShare.None);

                // get data thread
                while (bRunReaderThread)
                {
                    try
                    {
                        Buffer.SetByte(naDataRead, 0, 0);

                        // Socket.Receive -> Receives data from a bound Socket into a receive buffer.

                        // If no data is available for reading, the Receive method will block until data is available,
                        // unless a time-out value was set by using Socket.ReceiveTimeout.
                        int nLenDataRead = ocClientSocket.Receive(naDataRead);
                        if (nLenDataRead > 0)
                        {
                            // ...
                            //string strDataRead = Encoding.UTF8.GetString(naDataRead, 0, nLenDataRead);
                            //Console.WriteLine("GetData ... len: {0}", nLenDataRead);
                            if (nLenDataRead > 1024 * 1024 * 5)
                            {
                                nLenDataRead = 0;
                            }

                            // Console.WriteLine("   ->" + strDataRead);
                            //FileStreamLocal.Write(naDataRead, 0, nLenDataRead);

                            // send to parent
                            if (OnReceivedBufferDelegate != null)
                                OnReceivedBufferDelegate(naDataRead, nLenDataRead);
                        }
                        else
                        {
                            ocClientSocket.Shutdown(SocketShutdown.Both);
                            ocClientSocket.Close();

                            // connection is failed
                            if (OnConnectionStatusDelegate != null)
                                OnConnectionStatusDelegate(false, "Connection closed by server");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("exception on [GetData] -> " + e.Message);

                        // connection is failed
                        if (OnConnectionStatusDelegate != null)
                            OnConnectionStatusDelegate(false, e.Message);
                        return;
                    }
                    finally
                    {
                        // Console.WriteLine("exit thread [GetData]");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("exit thread [GetData] -> " + e.Message);

                // close socket connection
                if (IsConnected())
                {
                    ocClientSocket.Shutdown(SocketShutdown.Both);
                    ocClientSocket.Close();
                }

                // connection is failed
                if (OnConnectionStatusDelegate != null)
                    OnConnectionStatusDelegate(false, e.Message);
                return;
            }
            finally
            {
                if (FileStreamLocal != null)
                    FileStreamLocal.Close();
            }
        }
        #endregion

        #region Send
        public Int32 SendData(Byte[] buffer, Int32 nLen)
        {
            try
            {
                if (IsConnected())
                {
                    // blocks until send returns
                    return ocClientSocket.Send(buffer, 0, nLen, SocketFlags.None);
                }
                else
                {
                    Console.WriteLine("Can't send packet because socket is closed !!!");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
                return (e.ErrorCode);
            }
            return 0;
        }
        #endregion
    }
}
