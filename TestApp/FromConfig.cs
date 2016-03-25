using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace TestApp
{
    class FromConfig
    {
        public static string Server
        { get { return ConfigurationManager.AppSettings["Server"].ToString(); } }

        public static string Username
        { get { return ConfigurationManager.AppSettings["Username"].ToString(); } }

        public static string Password
        { get { return ConfigurationManager.AppSettings["Password"].ToString(); } }
    }
}
