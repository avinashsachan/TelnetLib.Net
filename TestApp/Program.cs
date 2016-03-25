using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelnetLib;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //new connection 
            var con = new TelnetClient(FromConfig.Server, 23, 30);


            try
            {
                if (!con.Login(FromConfig.Username, FromConfig.Password))
                {
                    throw new Exception("Failed to connect.");
                }

                Console.WriteLine(con.SessionLog);
                con.SendAndWait("cd /", "$");
                con.SendAndWait("ls -ltr ", "$");                
                Console.WriteLine(con.SessionLog);
                con.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(con.SessionLog);
                Console.WriteLine(ex.Message);
            }
           
        }
    }
}
