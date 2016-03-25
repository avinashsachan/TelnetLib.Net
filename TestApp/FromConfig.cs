using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class FromConfig
    {
        public static string Server { get {
            return System.Configuration.ConfigurationManager.AppSettings["Server"].ToString();

        
        } }

        public static string Username
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Username"].ToString();


            }
        }

        public static string Password
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();


            }
        }
    }
}
