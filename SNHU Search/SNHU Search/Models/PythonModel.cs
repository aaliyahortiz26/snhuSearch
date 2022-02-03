using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net.Http;

namespace SNHU_Search.Models
{
    public class PythonModel
    {

        public string Scrape(string website)
        {                      
            try
            {
                Console.WriteLine(Directory.GetCurrentDirectory());
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                startInfo.ErrorDialog = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C python WebScrape.py " + website;
                process.StartInfo = startInfo;
                process.Start();

                string stdoutx = process.StandardOutput.ReadToEnd();
                string stderrx = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Debug.WriteLine("Exit code: {0}", process.ExitCode);
                Debug.WriteLine("Stdout: {0}", stdoutx);
                Debug.WriteLine("Stderr: {0}", stderrx);

                string textFromWebsite = "";            

                var fileStream = new FileStream(@"webscrape.txt", FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        textFromWebsite += " " + line;
                    }
                }

                // remove characters from website's text
                List<char> charsToRemove = new List<char>() { ',', '/', '\\', '"' };

                foreach (char c in charsToRemove)
                {
                    textFromWebsite = textFromWebsite.Replace(c.ToString(), String.Empty);
                }
                return textFromWebsite;
            } 
            
            catch (Exception e)
            {
                Debug.WriteLine(e.Message.ToString());
                return "";
            }
        }

    }
}
