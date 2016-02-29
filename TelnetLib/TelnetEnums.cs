using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelnetLib
{
    class TelnetEnums
    {
         public enum Verbs
        {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }
        public enum Options
        {
            SGA = 3
        }
    }
}
