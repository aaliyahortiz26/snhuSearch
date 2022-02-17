﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SNHU_Search.Models
{
    public class DirectoryManager : ElasticManager
    {
        protected string m_path;
        private List<string> skippedFileList = new List<string>();
        private int countSkippedFiles = 0;

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
            string[] entries = Directory.GetFileSystemEntries(m_path, "*", SearchOption.AllDirectories);

            for (int i = 0; i < entries.Length; i++)
            {
                string fileExtension =System.IO.Path.GetExtension(entries[i]);
                string filename = Path.GetFileName(entries[i]);

                readFile(fileExtension, entries[i], filename);


                Console.WriteLine(entries[i]);
            }
        }

        void readFile(string fileType, string txtFilePath, string fileName)
        {
            string text = "";
            string tenWords = "";

            // text file
            // if(fileType == ".txt")
            // {
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
            //}
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
