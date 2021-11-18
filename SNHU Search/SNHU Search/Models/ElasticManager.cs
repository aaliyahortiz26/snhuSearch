﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace SNHU_Search.Models
{
    public class ElasticManager
    {
        private string elasticConnection = "http://20.115.112.182:9200/";
        public struct WebsiteDetails
        {
            public string Keywords;
            public string URL;
            public string Title;
            public string FirstTenWords;
        };
        public void addData(string username, string keywords, string url, string title, string TenWords)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(@"{
                    ""settings"" : {
                    ""number_of_shards"" : 2,
                    ""number_of_replicas"" : 1
                },
                ""mappings"" : {
                    ""properties"" : {
                        ""tags"" : { ""keywords"" : """ + keywords + @""", ""url"" : """ + url + @""", ""titleOfWebsite"" : """ + title + @""", ""firstTenWords"" : """ + TenWords + @"""},
                        ""updated_at"" : { ""type"" : ""date"" }
                    }
                }
            }", System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(elasticConnection + username + "/_doc", content).Result;
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Got response");
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            client.Dispose();
        }

        public List<WebsiteDetails> search(string username, string sKeywords)
        {
            WebsiteDetails details = new WebsiteDetails();       
            List<WebsiteDetails> UrlKeywordsList = new List<WebsiteDetails>();

            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(String.Format(elasticConnection + username + "/_search?q={0}", sKeywords)),
                Content = new StringContent(@"{
                    ""query"": {
                        ""match_all"": { }
                    }
                }", System.Text.Encoding.UTF8, "application/json"),
            };

            HttpResponseMessage response = client.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
                using (HttpContent content = response.Content)
                {
                    var json = content.ReadAsStringAsync().Result;
                    dynamic esResults = JsonConvert.DeserializeObject(json);
                    int numHits = (int)esResults["hits"]["total"]["value"].Value;
                    var hitsList = esResults["hits"]["hits"];
                    for (int i = 0; i < numHits; i++)
                    {
                        var esTags = hitsList[i]["_source"]["mappings"]["properties"]["tags"];
                        details.URL = esTags["url"];

                        details.Keywords = esTags["keywords"];
                        details.Title = esTags["titleOfWebsite"];
                        details.FirstTenWords = esTags["firstTenWords"];

                        UrlKeywordsList.Add(details);
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed - {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            client.Dispose();
            return UrlKeywordsList;
        }


        public void removeData(string username, string website)
        {
            List<string> UrlKeywordsList = new List<string>();
            string urlString;

            if (website[website.Count()-1] == '/')
            {
                website = website.TrimEnd(new[] { '/' });
            }

            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(String.Format(elasticConnection + username + "/_search?q=" + website)),
                Content = new StringContent(@"{
                    ""query"": {
                        ""match_all"": { }
                    }
                }", System.Text.Encoding.UTF8, "application/json"),
            };

            HttpResponseMessage response = client.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
                using (HttpContent content = response.Content)
                {
                    var json = content.ReadAsStringAsync().Result;
                    dynamic esResults = JsonConvert.DeserializeObject(json);
                    int numHits = (int)esResults["hits"]["total"]["value"].Value;
                    var hitsList = esResults["hits"]["hits"];
                    for (int i = 0; i < numHits; i++)
                    {
                        // returns the id of the hit
                        var id = hitsList[i]["_id"];
                        var esTags = hitsList[i]["_source"]["mappings"]["properties"]["tags"];                   
                        var Url = esTags["url"];
                        urlString = Url;                
                        if (urlString[urlString.Count() - 1] == '/')
                        {
                            urlString = urlString.TrimEnd(new[] { '/' });
                        }
                        if (urlString == website)
                        {
                            client.DeleteAsync(elasticConnection + username + "/_doc/" + id);
                        }
                    }
                }
            }
        }
    }
}
