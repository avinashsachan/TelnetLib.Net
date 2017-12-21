using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelnetLib
{
    public static class Helper
    {
        /// <summary>
        ///  
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static byte[] ConvertToByteArray(string strText)
        {
            return strText.ToCharArray().Select(x => Convert.ToByte(x)).ToArray();
            //byte[] smk = new byte[strText.Length];
            //var charArray = strText.ToCharArray();
            //for (int i = 0; i <= strText.Length - 1; i++)
            //{
            //    smk[i] = Convert.ToByte(charArray[i]);
            //}
            //return smk;
        }
    }
}
