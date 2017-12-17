using Microsoft.VisualBasic;
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
        #region "Property"

        // Holds everything received from the server since our last processing
        private readonly object _messagesLockWorkingData = new object();
        private StringBuilder _strWorkingData = new StringBuilder();
        private StringBuilder _strFullLog = new StringBuilder();


        //private Socket _s;
        //private IPEndPoint _iep;
        private byte[] _mByBuff = new byte[32767];
        private object _lockPacketRead = new object();
        private bool packetReadInProcess = false;
        private bool _packetReadInProcess
        {
            get
            {
                lock (_lockPacketRead)
                {
                    return this.packetReadInProcess;
                }
            }

            set
            {
                lock (_lockPacketRead)
                {
                    this.packetReadInProcess = value;
                }
            }
        }


        private SocketAPI Channel = new SocketAPI();

        private string _ServerName = "";
        public string ServerName
        {
            get { return _ServerName; }
            set { _ServerName = value; }
        }

        private string _Address;
        public string Address
        {
            get { return _Address; }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
        }


        private int _Timeout;
        public Int32 Timeout
        {
            get { return _Timeout; }
            set { _Timeout = Math.Max(value, 0); }
        }



        private string _CurrentTerminalType = "XTERM";
        public string CurrentTerminalType
        {
            get { return _CurrentTerminalType; }
        }

        private string _RequestedTerminalType = "XTERM";


        #endregion

        public void SetTerminalType(string terminalType)
        {
            this._RequestedTerminalType = terminalType;
            RequestedTerminalType();
        }

        private void RequestedTerminalType()
        {
            //throw new NotImplementedException();
            //if (!this.IsConnected) return;

        }

        public TelnetClient(string address, int port, int commandTimeout, string serverName = "")
        {
            Channel = new SocketAPI();
            Channel.DataRecieved += new PacketRecieved(OnRecievedData);
            this._Address = address;
            this._ServerName = serverName;
            this._Port = port;
            this._Timeout = commandTimeout;

        }

        private void ParseTelnetData(ref StringBuilder sb, int nBytesRec)
        {
            //_mByBuff, 0, nBytesRec
            var byteToSend = new List<byte>();
            //int inputOption = 0;


            long k = 0;
            while (k < nBytesRec)
            {
                int input = _mByBuff[k];
                k++;
                switch (input)
                {
                    case -1:
                    case 0:
                        break;
                    case (int)TelnetCommand.IAC:
                        // interpret as command
                        int inputCommand = _mByBuff[k];
                        k++;

                        switch (inputCommand)
                        {
                            case -1:
                                break;
                            case (int)TelnetCommand.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputCommand);
                                break;
                            case (int)TelnetCommand.DO:
                                int inputOptionDO = _mByBuff[k];
                                k++;
                                if (inputOptionDO == -1) break;
                                switch ((TelnetOptions)inputOptionDO)
                                {
                                    case TelnetOptions.SGA:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.WILL, (byte)inputOptionDO });
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.DO, (byte)inputOptionDO });
                                        break;
                                    case TelnetOptions.ECHO:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.WONT, (byte)inputOptionDO });
                                        break;

                                    case TelnetOptions.TM:
                                    case TelnetOptions.TTYPE:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.WILL, (byte)inputOptionDO });
                                        break;

                                    case TelnetOptions.STATUS:
                                    case TelnetOptions.EXOPL:
                                    default:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.WONT, (byte)inputOptionDO });
                                        break;
                                }

                                break;
                            case (int)TelnetCommand.WILL:
                                int inputOptionWill = Channel._mByBuff[k];
                                k++;

                                if (inputOptionWill == -1) break;

                                switch ((TelnetOptions)inputOptionWill)
                                {
                                    case TelnetOptions.SGA:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.DO, (byte)inputOptionWill });
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.WILL, (byte)inputOptionWill });

                                        break;
                                    case TelnetOptions.ECHO:
                                        //here we need to check  echo setting 
                                        //byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.DO, (byte)inputOptionWill });
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.DONT, (byte)inputOptionWill });
                                        break;

                                    case TelnetOptions.EXOPL:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.DONT, (byte)inputOptionWill });
                                        break;
                                    case TelnetOptions.STATUS:
                                    case TelnetOptions.TM:
                                    default:
                                        break;
                                }

                                break;

                            case (int)TelnetCommand.WONT:
                                int inputOptionWont = _mByBuff[k];
                                k++;
                                if (inputOptionWont == -1) break;
                                switch ((TelnetOptions)inputOptionWont)
                                {

                                    case TelnetOptions.SGA:
                                    case TelnetOptions.BINARY:
                                    case TelnetOptions.ECHO:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.DONT, (byte)inputOptionWont });
                                        break;
                                    case TelnetOptions.STATUS:
                                    case TelnetOptions.TM:
                                    case TelnetOptions.EXOPL:
                                    default:
                                        break;
                                }

                                break;


                            case (int)TelnetCommand.DONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputOptionDont = _mByBuff[k];
                                k++;
                                if (inputOptionDont == -1) break;

                                switch ((TelnetOptions)inputOptionDont)
                                {
                                    case TelnetOptions.SGA:
                                    case TelnetOptions.BINARY:
                                    case TelnetOptions.ECHO:
                                        byteToSend.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.WONT, (byte)inputOptionDont });
                                        break;
                                    case TelnetOptions.STATUS:
                                    case TelnetOptions.TM:
                                    case TelnetOptions.EXOPL:
                                    default:
                                        break;
                                }

                                break;

                            case (int)TelnetCommand.SB:
                                int inputOptionSB = _mByBuff[k];
                                k++;
                                if (inputOptionSB == -1) break;

                                switch ((TelnetOptions)inputOptionSB)
                                {
                                    case TelnetOptions.TTYPE:
                                        int d = _mByBuff[k];
                                        k++;
                                        if (d == 1)
                                        {

                                            var ANS = new List<byte>();
                                            ANS.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.SB, (byte)TelnetOptions.TTYPE });
                                            ANS.Add((byte)00);
                                            ANS.AddRange(Helper.ConvertToByteArray(CurrentTerminalType));
                                            ANS.AddRange(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.SE });
                                            byteToSend.AddRange(ANS.ToArray());

                                        }
                                        break;
                                }

                                //here ignoring othere Sub Options 
                                while ((int)_mByBuff[k] != 255)
                                {
                                    k++;
                                }

                                break;
                            case (int)TelnetCommand.SE:
                            default:
                                break;
                        }
                        break;
                    case 07:
                        break;
                    case 27:
                        //    \033    \x1b
                        //here this is color code
                        //                        k++;
                        if (_mByBuff[k] >= 64 && _mByBuff[k] <= 95)
                        {
                            var e = new List<Int32>();
                            e.AddRange(new Int32[] { 61, 109, 104, 59 });
                            while (!e.Contains(_mByBuff[k]))
                            {
                                Console.WriteLine("{0}\t{1}\t{2}", _mByBuff[k], _mByBuff[k].ToString("X"), (char)_mByBuff[k]);
                                k++;
                            }

                            //here check tailing 
                            if (_mByBuff[k] == 59)
                            {
                                if (_mByBuff[k + 3] == 109)
                                {
                                    k += 3;
                                }
                                else {
                                    //Console.WriteLine("");
                                }
                            }
                            k++;
                        }
                        else {
                            sb.Append((char)input);
                        }

                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }

            //here if list contain some ans then send 
            if (byteToSend.Count > 0)
            {
                Channel.Write(byteToSend.ToArray());
            }

        }

        private void OnRecievedData(object sender, EventArgs e)
        {
            //any previous event is already in process 
            if (_packetReadInProcess) return;

            _packetReadInProcess = true;
            try
            {
                while (Channel.outputQueue.Count > 0)
                {
                    _mByBuff = Channel.outputQueue.Dequeue();
                    int nBytesRec = _mByBuff.Length;

                    if (nBytesRec > 0)
                    {
                        var sb = new StringBuilder();
                        ParseTelnetData(ref sb, nBytesRec);
                        string sRecieved = sb.ToString();

                        lock (_messagesLockWorkingData)
                        {
                            _strWorkingData.Append(sRecieved.ToLower());
                            _strFullLog.Append(sRecieved);
                        }

#if DEBUG
                        //Console.Write(sRecieved.Trim());
                        Debug.WriteLine(sRecieved);
#endif
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                _packetReadInProcess = false;
            }
        }

        private void DoSend(string strText)
        {
            try
            {
                byte[] smk = Helper.ConvertToByteArray(strText);

                //here we are setting working data to ""
                lock (_messagesLockWorkingData)
                {
                    _strWorkingData.Clear();
                }
                //_s.Send(smk, 0, smk.Length, SocketFlags.None);
                Channel.Write(smk);
            }
            catch (Exception)
            {
                //MessageBox.Show("ERROR IN RESPOND OPTIONS");
            }
        }

        public bool Connect()
        {
            try
            {
                var success = Channel.Connect(this._Address, this._Port);
                if (!success) throw new Exception("Failed to connect");
                return true;
            }
            catch (Exception ex)
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
            }

            //here now initiate connection
            try
            {
                if (!this.Connect()) { throw new Exception("Failed to connect."); }

                //here wait for login prompt
                this.WaitFor("login:|Username:|login :|Username :", "|");
                this.SendAndWait(Username, "Password:|Password :", "|");
                this.SendAndWait(Password, "#|$|>", "|");
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                throw;

            }
        }

        //created 2 function to avoid extra overhead
        public int WaitFor(string dataToWaitFor)
        {

            // Get the starting time
            long lngStart = System.DateTime.Now.AddSeconds(_Timeout).Ticks;

            string ln = "";

            while (ln.IndexOf(dataToWaitFor, StringComparison.OrdinalIgnoreCase) == -1)
            {
                try
                {
                    if (ln.IndexOf(dataToWaitFor, StringComparison.OrdinalIgnoreCase) != -1) return 0;

                    if (System.DateTime.Now.Ticks > lngStart) throw new Exception("Timeout waiting for : " + dataToWaitFor);

                    if ((ln.IndexOf("Idle too long; timed out", StringComparison.OrdinalIgnoreCase) != -1))
                        throw new Exception("Connection Terminated forcefully");

                    lock (_messagesLockWorkingData)
                    {
                        ln = _strWorkingData.ToString(0, _strWorkingData.Length);
                        if (ln.Length > 50)
                            _strWorkingData.Remove(0, ln.Length - 50);
                    }
                }
                catch (Exception)
                {
                    lock (_messagesLockWorkingData) { _strWorkingData.Clear(); }
                    throw;
                }
            }

            lock (_messagesLockWorkingData) { _strWorkingData.Clear(); }
            return 0;
        }

        public int WaitFor(string dataToWaitFor, string breakCharacter)
        {
            // Get the starting time
            long lngStart = System.DateTime.Now.AddSeconds(_Timeout).Ticks;
            string ln = "";

            string[] breaks = dataToWaitFor.Split(breakCharacter.ToCharArray());
            int intReturn = -1;

            while (intReturn == -1)
            {
                if (System.DateTime.Now.Ticks > lngStart) { throw new Exception("Timeout waiting for : " + dataToWaitFor); }

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

        //created 2 function to avoid extra overhead
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

        public bool IsConnected
        {
            get
            {
                try
                { return this.Channel.IsConnected; }
                catch (Exception) { return false; }
            }
        }

        public void Disconnect()
        {
            try { if (this.IsConnected) { this.Channel.Disconnect(); } }
            catch { }
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

    }
}

