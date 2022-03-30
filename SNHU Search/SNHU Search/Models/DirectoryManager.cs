using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spire.Doc;
using System.Text;
using Spire.Doc.Documents;
using Spire.Pdf;

namespace SNHU_Search.Models
{
    public class DirectoryManager : ElasticManager
    {
        protected string m_path;
        private List<string> skippedFileList = new List<string>();
        private int countSkippedFiles = 0;
        private int countNumFiles = 0;
        private string m_username = "";
        private string elasticIndexNameLocal = "local"; 
        public void setPath(string localPath)
        {
            m_path = localPath;
        }
        public void setUsername(string username)
        {
            m_username = username;
        }
        // does not include username
        public string getElasticSearchIndexName()
        {
            return elasticIndexNameLocal;
        }          
        public void scan()
        {
            Startup.Progress = 0;
            string[] entries = Directory.GetFileSystemEntries(m_path, "*", SearchOption.AllDirectories);

            for (int i = 0; i < entries.Length; i++)
            {
                string fileExtension =System.IO.Path.GetExtension(entries[i]);
                string filename = Path.GetFileName(entries[i]);

                readFile(fileExtension, entries[i], filename);
                countNumFiles++;

                Startup.Progress = (int)((float)countNumFiles / (float)entries.Count() * 100.0);
               Task.Delay(100);               
                Console.WriteLine(entries[i]);
            }
        }

        void readFile(string fileType, string txtFilePath, string fileName)
        {
            string text = "";
            string tenWords = "";

            // directory 
            if (fileType == "")
            {
                return;
            }
            // text file
            else if(fileType == ".txt")
            {
                try
                {
                    // gets all of the text
                    text = File.ReadAllText(txtFilePath);
                    text = text.Replace("\r\n", " ");

                    tenWords = getTenWebsiteWords(text);
                    txtFilePath = txtFilePath.Replace("\\", "/");
                    addData(elasticIndexNameLocal + m_username.ToLower(), text, txtFilePath, fileName, tenWords);
                }
                catch
                {

                    skippedFileList.Add(txtFilePath);
                    countSkippedFiles++;
                    return;
                }
            }
            // Word Documents
            else if(fileType == ".docx")
            {
                try
                {
                    //Load Document
                    Document document = new Document();
                    document.LoadFromFile(txtFilePath);

                    // Extract text from Word document
                    foreach (Section section in document.Sections)
                    {
                        foreach (Paragraph paragraph in section.Paragraphs)
                        {
                            text += paragraph.Text + " ";
                        }
                    }
                    tenWords = getTenWebsiteWords(text);
                    txtFilePath = txtFilePath.Replace("\\", "/");
                    addData(elasticIndexNameLocal + m_username.ToLower(), text, txtFilePath, fileName, tenWords);
                }
                catch
                {

                    skippedFileList.Add(txtFilePath);
                    countSkippedFiles++;
                    return;
                }
            }
            // PDF Documents
            else if (fileType == ".pdf")
            {
                try
                {
                    PdfDocument document = new PdfDocument();
                    document.LoadFromFile(txtFilePath);

                    foreach (PdfPageBase page in document.Pages)
                    {
                        text += page.ExtractText();
                    }
                    text = text.Replace("\r\n", " ");

                    tenWords = getTenWebsiteWords(text);
                    txtFilePath = txtFilePath.Replace("\\", "/");
                    addData(elasticIndexNameLocal + m_username.ToLower(), text, txtFilePath, fileName, tenWords);
                }
                catch
                {

                    skippedFileList.Add(txtFilePath);
                    countSkippedFiles++;
                    return;
                }
            }
            // other types of document, skip file
            else
            {
                countSkippedFiles++;
            }
        }
        public string getTenWebsiteWords(string websiteText)
        {
            int words = 10;
            string tenWebsiteWords = websiteText;
            for (int i = 0; i < tenWebsiteWords.Length; i++)
            {
                // Increment words on a space.
                if (tenWebsiteWords[i] == ' ')
                {
                    words--;
                }
                // If we have no more words to display, return the substring.
                if (words == 0)
                {
                    return tenWebsiteWords.Substring(0, i);
                }
            }
            return tenWebsiteWords;
        }
        public int getFilesSkipped()
        {
            return countSkippedFiles;
        }
    }
}
