﻿using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace TelnetLib
{


    /// <summary>
    /// Summary description for clsScriptingTelnet.
    /// </summary>
    public class TelnetClient
    {
        private string _ServerName = "";
        private IPEndPoint _iep;

        private string _Address;
        private readonly int _port;
        private int _Timeout;
        private Socket _s;

        private byte[] _mByBuff = new byte[32767];
        private StringBuilder _strFullLog = new StringBuilder();

        // Holds everything received from the server since our last processing
        private StringBuilder _strWorkingData = new StringBuilder();

        private readonly object _messagesLockWorkingData = new object();

        public TelnetClient(string address, int port, int commandTimeout, string serverName = "")
        {
            this._Address = address;
            this._ServerName = serverName;
            _port = port;
            _Timeout = commandTimeout;
        }

        private void ParseTelnetData(ref StringBuilder sb, int nBytesRec)
        {
            //_mByBuff, 0, nBytesRec
            long k = 0;
            while (k < nBytesRec)
            {
                int input = _mByBuff[k];
                k++;
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = _mByBuff[k];
                        k++;
                        if (inputverb == -1)
                            break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = _mByBuff[k];
                                k++;
                                if (inputoption == -1)
                                    break;

                                _s.Send(new byte[] { ((byte)Verbs.IAC) }, 1, SocketFlags.None);
                                if (inputoption == (int)Options.SGA)
                                    _s.Send(new byte[] { (inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO) }, 1, SocketFlags.None);
                                else
                                    _s.Send(new byte[] { (inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT) }, 1, SocketFlags.None);

                                _s.Send(new byte[] { ((byte)inputoption) }, 1, SocketFlags.None);
                                break;

                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
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
                    var sb = new StringBuilder();
                    ParseTelnetData(ref sb, nBytesRec);
                    string sRecieved = CleanDisplay(sb.ToString());

                    lock (_messagesLockWorkingData)
                    {
                        _strWorkingData.Append(sRecieved.ToLower());
                        _strFullLog.Append(sRecieved);
                    }

#if DEBUG
                    //Console.Write(sRecieved.Trim());
                    Debug.Write(sRecieved);
#endif
                    //Thread.Sleep(10)
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

        private void DoSend(string strText)
        {
            try
            {
                byte[] smk = new byte[strText.Length];
                for (int i = 0; i <= strText.Length - 1; i++)
                {
                    byte ss = Convert.ToByte(strText.ToCharArray()[i]);
                    smk[i] = ss;
                }

                //here we are setting working data to ""
                lock (_messagesLockWorkingData)
                {
                    _strWorkingData.Clear();
                }
                _s.Send(smk, 0, smk.Length, SocketFlags.None);
            }
            catch (Exception)
            {
                //MessageBox.Show("ERROR IN RESPOND OPTIONS");
            }
        }

        private string CleanDisplay(string input)
        {
            input = input.Replace(((char)0).ToString(), "");
            return input;


            //input = input.Replace(@"(0x (B", @"|");
            //input = input.Replace(@"(0 x(B", @"|");
            //input = input.Replace(@")0=>", @"");
            //input = input.Replace(@"[0m>", @"");
            //input = input.Replace(@"7[7m", @"[");
            //input = input.Replace(@"[0m*8[7m", @"]");
            //input = input.Replace(@"[0m", @"");

            //        input = input.Replace("\u001b(0x \u001b(B", "|");
            //        input = input.Replace("\u001b(0 x\u001b(B", "|");
            //        input = input.Replace("\u001b)0\u001b=\u001b>", "");
            //        input = input.Replace("\u001b[0m\u001b>", "");
            //        input = input.Replace("\u001b7\u001b[7m", "[");
            //        input = input.Replace("\u001b[0m*\u001b8\u001b[7m", "]");
            //        input = input.Replace("\u001b[0m", "");

        }


        public bool Connect()
        {
            try
            {
                _iep = new IPEndPoint(IPAddress.Parse(_Address), _port);
                _s = new Socket((_iep.AddressFamily == AddressFamily.InterNetworkV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork), SocketType.Stream, ProtocolType.Tcp);
                _s.Connect(_iep);

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

        //this implementation is working for almost all linux rpm/debian based distributions
        //platforms like solaris , HP-UX , Cisco/Juniper Routers  not tested
        public bool Login(string Username, string Password)
        {
            //here in telnet we actually dont know , where we are in terminal session
            //it will be goos to initiate connection again
            if (this.IsConnected)
            {
                this.Disconnect();
                this._s.Dispose();
            }

            //here now initiate connection
            try { 
                if (!this.Connect()) { throw new Exception("Failed to connect."); } 

                //here wait for login prompt
                this.WaitFor("login:|Username:" ,"|");
                this.SendAndWait(Username, "Password:");
                this.SendAndWait(Password , "#|$", "|");
                return true;
            }
            catch { throw; }
        }

        public bool IsConnected
        {
            get
            {
                try
                { return _s.Connected; }
                catch (Exception) { return false; }
            }
        }

        public void Disconnect()
        {
            try { if (_s.Connected) { _s.Disconnect(false); } }
            catch { }
        }

        public int WaitFor(string dataToWaitFor)
        {

            // Get the starting time
            long lngStart = System.DateTime.Now.AddSeconds(_Timeout).Ticks;
            long lngCurTime = 0;
            //  Dim start_index As UInt64 = 0
            //  Dim End_index As UInt64 = 0
            string ln = "";
            // Dim L As Integer = 0
            while (ln.IndexOf(dataToWaitFor, StringComparison.OrdinalIgnoreCase) == -1)
            {
                // Timeout logic
                lngCurTime = System.DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    throw new Exception("Timeout waiting for : " + dataToWaitFor);
                }
                Thread.Sleep(5);

                if ((ln.IndexOf("Idle too long; timed out", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    //intReturn = -2
                    if (ln.IndexOf(dataToWaitFor, StringComparison.OrdinalIgnoreCase) != -1)
                        return 0;
                    lock (_messagesLockWorkingData)
                    {
                        _strWorkingData.Clear();
                    }
                    throw new Exception("Connection Terminated forcefully");
                }

                //  L = strWorkingData.Length
                lock (_messagesLockWorkingData)
                {
                    ln = _strWorkingData.ToString(0, _strWorkingData.Length);
                    _strWorkingData.Remove(0, ln.Length < 50 ? 0 : ln.Length - 50);
                    //CLIPPING OF LN FROM WORKING DATA
                }
            }
            lock (_messagesLockWorkingData)
            {
                _strWorkingData.Length = 0;
            }

            return 0;
        }

        public int WaitFor(string dataToWaitFor, string breakCharacter)
        {
            // Get the starting time
            long lngStart = System.DateTime.Now.AddSeconds(_Timeout).Ticks;
            long lngCurTime = 0;
            //  Dim start_index As UInt64 = 0
            //  Dim End_index As UInt64 = 0
            string ln = "";

            string[] breaks = dataToWaitFor.Split(breakCharacter.ToCharArray());
            int intReturn = -1;

            while (intReturn == -1)
            {
                // Timeout logic
                lngCurTime = System.DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    throw new Exception("Timeout waiting for : " + dataToWaitFor);
                }
                Thread.Sleep(5);
                for (int i = 0; i <= breaks.Length - 1; i++)
                {
                    if (ln.IndexOf(breaks[i], StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        intReturn = i;
                    }
                }
                if ((ln.IndexOf("Idle too long; timed out", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    for (int i = 0; i <= breaks.Length - 1; i++)
                    {
                        lock (_messagesLockWorkingData)
                        {
                            if (_strWorkingData.ToString().IndexOf(breaks[i].ToLower(), StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                return i;
                            }
                        }
                    }
                    throw new Exception("Connection Terminated forcefully");
                }
                lock (_messagesLockWorkingData)
                {
                    ln = _strWorkingData.ToString(0, _strWorkingData.Length);
                    _strWorkingData.Remove(0, ln.Length < 50 ? 0 : ln.Length - 50);
                    //CLIPPING OF LN FROM WORKING DATA
                }

            }
            lock (_messagesLockWorkingData)
            {
                _strWorkingData.Length = 0;
            }
            return intReturn;

        }


        public void SendMessage(string message, bool suppressCarriageReturn = false)
        {
            DoSend(message + (suppressCarriageReturn ? "" : "\r"));
        }

        public bool WaitAndSend(string waitFor, string message, bool suppressCarriegeReturn = false)
        {
            this.WaitFor(waitFor);
            SendMessage(message, suppressCarriegeReturn);
            return true;
        }


        public int SendAndWait(string message, string waitFor, bool suppressCarriegeReturn = false)
        {
            lock (_messagesLockWorkingData)
            {
                _strWorkingData.Length = 0;
            }

            SendMessage(message, suppressCarriegeReturn);
            this.WaitFor(waitFor);
            return 0;
        }

        public int SendAndWait(string message, string waitFor, string breakCharacter, bool suppressCarriegeReturn = false)
        {
            lock (_messagesLockWorkingData)
            {
                _strWorkingData.Length = 0;
            }
            SendMessage(message, suppressCarriegeReturn);
            int t = this.WaitFor(waitFor, breakCharacter);
            return t;
        }


        /// <summary>
        /// Clears all data in the session log
        /// </summary>
        public void ClearSessionLog()
        {
            lock (_messagesLockWorkingData)
            {
                _strFullLog.Clear();
            }
        }



        #region "Property"

        public string ServerName
        {
            get { return _ServerName; }
            set { _ServerName = value; }
        }
        public string Address
        {
            get { return _Address; }
        }
        public Int32 Timeout
        {
            get { return _Timeout; }
            set { _Timeout = Math.Max(value, 0); }
        }

        /// <summary>
        /// A full log of session activity
        /// </summary>
        public string SessionLog
        {
            get
            {
                lock (_messagesLockWorkingData)
                {
                    return _strFullLog.ToString();
                }
            }
        }

        #endregion

    }
}

