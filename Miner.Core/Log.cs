using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Miner
{
    public class Log
    {
        private static string file = Application.StartupPath + @"\err.log";
        public static void LogError(Exception e,string msg)
        {
            string res = DateTime.Now.ToString("yyyy-mm-dd HH:MM:ss");
            res += "------------------------------------------------------------------------------------\r\n";
            res += "异常信息：";
            res += e.Message;
            res += "\r\n";
            res += msg;
            res += "\r\n";
            File.AppendAllText(file, res);
        }
    }
}
