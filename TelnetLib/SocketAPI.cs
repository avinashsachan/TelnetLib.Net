using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TelnetLib
{
    public delegate void PacketRecieved(object sender, EventArgs e);
    class SocketAPI
    {
        public event PacketRecieved DataRecieved;

        public Socket _s;
        public IPEndPoint _iep;
        public byte[] _mByBuff = new byte[32767];
        public Queue<byte[]> OutputQueue = new Queue<byte[]>();

        public SocketAPI()
        {

        }

        public bool Connect(string _Address, Int32 _Port)
        {
            try
            {
                _iep = new IPEndPoint(IPAddress.Parse(_Address), _Port);
                _s = new Socket((_iep.AddressFamily == AddressFamily.InterNetworkV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork), SocketType.Stream, ProtocolType.Tcp);
                _s.Connect(_iep);
#if DEBUG
                //Console.WriteLine("Connected");
#endif
                // If the connect worked, setup a callback to start listening for incoming data
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                _s.BeginReceive(_mByBuff, 0, _mByBuff.Length, SocketFlags.None, recieveData, _s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnRecievedData(IAsyncResult ar)
        {
            try
            {
                // Get The connection socket from the callback
                Socket sock = (Socket)ar.AsyncState;
                // Get The data , if any
                int nBytesRec = sock.EndReceive(ar);

                if (nBytesRec > 0)
                {
#if DEBUG
                    //Console.WriteLine("New byte found");
#endif
                    //here now copy this buffer to this 
                    byte[] buff = new byte[nBytesRec];
                    Array.Copy(_mByBuff, buff, nBytesRec);

                    OutputQueue.Enqueue(buff);
                    OnChanged(EventArgs.Empty);

                    Thread.Sleep(5);
                    // Launch another callback to listen for data
                    AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                    sock.BeginReceive(_mByBuff, 0, _mByBuff.Length, SocketFlags.None, recieveData, sock);
                }
                else
                {
                    // If no data was recieved then the connection is probably dead
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }

            }
            catch
            {
                //Console.Write(ex.Message);
            }
        }

        public int Write(byte[] byteToSend)
        {
            return _s.Send(byteToSend, byteToSend.Length, SocketFlags.None);
        }

        public bool IsConnected
        {
            get
            {
                try { return _s.Connected; }
                catch { return false; }
            }
        }

        public void Disconnect()
        {
            try { if (this.IsConnected) { _s.Disconnect(false); } }
            catch { }
        }


        // Invoke the Changed event; called whenever list changes
        protected virtual void OnChanged(EventArgs e)
        {
            DataRecieved?.Invoke(this, e);
        }
    }
}
