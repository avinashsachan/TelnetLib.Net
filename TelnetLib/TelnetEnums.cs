using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelnetLib
{
    public class TerminalType
    {
        public static string[] GetTerminalType
        {
            get
            {
                return new string[] { "XTERM", "ANSI", "VT100", "UNKNOWN" };
            }
        }
    }

    public enum TelnetCommand
    {
        IAC = 255,          /* interpret as command: */
        DONT = 254,         /* you are not to use option */
        DO = 253,           /* please, you use option */
        WONT = 252,         /* I won't use option */
        WILL = 251,         /* I will use option */
        SB = 250,           /* interpret as subnegotiation */
        GA = 249,           /* you may reverse the line */
        EL = 248,           /* erase the current line */
        EC = 247,           /* erase the current character */
        AYT = 246,          /* are you there */
        AO = 245,           /* abort output--but let prog finish */
        IP = 244,           /* interrupt process--permanently */
        BREAK = 243,        /* break */
        DM = 242,           /* data mark--for connect. cleaning */
        NOP = 241,          /* nop */
        SE = 240,           /* end sub negotiation */
        EOR = 239,          /* end of record (transparent mode) */
        ABORT = 238,        /* Abort process */
        SUSP = 237,         /* Suspend process */
        EOF = 236,          /* End of file: EOF is already used... */
        SYNCH = 242,        /* for telfunc calls */
    }

    public enum TelnetSubOptionsQualifiers
    {

        TELQUAL_IS = 0,         /* option is... */
        TELQUAL_SEND = 1,       /* send option */
        TELQUAL_INFO = 2,       /* ENVIRON: informational version of IS */
        TELQUAL_REPLY = 2,      /* AUTHENTICATION: client version of IS */
        TELQUAL_NAME = 3,       /* AUTHENTICATION: client version of IS */

        LFLOW_OFF = 0,          /* Disable remote flow control */
        LFLOW_ON = 1,           /* Enable remote flow control */
        LFLOW_RESTART_ANY = 2,  /* Restart output on any char */
        LFLOW_RESTART_XON = 3,  /* Restart output only on XON */

    }

    public enum TelnetOptions
    {
        BINARY = 0,         /* 8-bit data path */
        ECHO = 1,           /* echo */
        RCP = 2,            /* prepare to reconnect */
        SGA = 3,            /* suppress go ahead */
        NAMS = 4,           /* approximate message size */
        STATUS = 5,         /* give status */
        TM = 6,             /* timing mark */
        RCTE = 7,           /* remote controlled transmission and echo */
        NAOL = 8,           /* negotiate about output line width */
        NAOP = 9,           /* negotiate about output page size */
        NAOCRD = 10,        /* negotiate about CR disposition */
        NAOHTS = 11,        /* negotiate about horizontal tabstops */
        NAOHTD = 12,        /* negotiate about horizontal tab disposition */
        NAOFFD = 13,        /* negotiate about formfeed disposition */
        NAOVTS = 14,        /* negotiate about vertical tab stops */
        NAOVTD = 15,        /* negotiate about vertical tab disposition */
        NAOLFD = 16,        /* negotiate about output LF disposition */
        XASCII = 17,        /* extended ascii character set */
        LOGOUT = 18,        /* force logout */
        BM = 19,            /* byte macro */
        DET = 20,           /* data entry terminal */
        SUPDUP = 21,        /* supdup protocol */
        SUPDUPOUTPUT = 22,  /* supdup output */
        SNDLOC = 23,        /* send location */
        TTYPE = 24,         /* terminal type */
        EOR = 25,           /* end or record */
        TUID = 26,          /* TACACS user identification */
        OUTMRK = 27,        /* output marking */
        TTYLOC = 28,        /* terminal location number */
        REGIME3270 = 29,    /* 3270 regime */
        X3PAD = 30,         /* X.3 PAD */
        NAWS = 31,          /* negotiate about window size */
        TSPEED = 32,        /* terminal speed */
        LFLOW = 33,         /* remote flow control */
        LINEMODE = 34,      /* Linemode option */
        XDISPLOC = 35,      /* X Display Location */
        OLD_ENVIRON = 36,   /* Old - Environment variables */
        AUTHENTICATION = 37,/* Authenticate */
        ENCRYPT = 38,       /* Encryption option */
        NEW_ENVIRON = 39,   /* New - Environment variables */
        EXOPL = 255,        /* extended-options-list */

    }

    class TelnetEnums
    {

    }
}
