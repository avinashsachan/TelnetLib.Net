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

            if (con.Connect())
            {
                Console.WriteLine("Conencted");
                //System.Threading.Thread.Sleep(2000);
                con.WaitFor("login:");
                con.SendAndWait(FromConfig.Username + "\n", "Password:",true);
                con.SendAndWait(FromConfig.Password + "\n", "$", true);
                Console.WriteLine(con.SessionLog);
                con.SendAndWait("ls -lrt" + "\n", "$", true);
            }
            else
            {
                Console.WriteLine("No connection.");
            }




        }
    }
}
