using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelnetLib
{
    public enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        //InterpretAsCommand
        IAC = 255
    }
    public enum Options
    {
        //SuppressGoAhead
        SGA = 3
    }

    class TelnetEnums
    {

    }
}
