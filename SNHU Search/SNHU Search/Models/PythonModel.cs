using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;


namespace SNHU_Search.Models
{
    public class PythonModel
    {

        public void Scrape(string website/*string cmd, string args*/)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C python WebScrape.py";
            process.StartInfo = startInfo;
            process.Start();
        }

    }
}
