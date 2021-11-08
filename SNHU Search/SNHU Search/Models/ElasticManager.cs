using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace SNHU_Search.Models
{
    public class ElasticManager
    {
        private string elasticConnectionPost = "http://20.115.112.182:9200/test_index1/_doc";
        private string elasticConnectionRequest = "http://20.115.112.182:9200/test_index1/_search?q={0}";
        public void addData(string keywords, string url)
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
                        ""tags"" : { ""keywords"" : """ + keywords + @""", ""url"" : """ + url + @"""},
                        ""updated_at"" : { ""type"" : ""date"" }
                    }
                }
            }", System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(elasticConnectionPost, content).Result;
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

        public List<string> search(string sKeywords)
        {
            List<string> UrlKeywordsList = new List<string>();

            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(String.Format(elasticConnectionRequest, sKeywords)),
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
                        var Url = esTags["url"];
                        var Keywords = esTags["keywords"];
                        UrlKeywordsList.Add(Url.ToString());
                        UrlKeywordsList.Add(Keywords.ToString());
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
    }
}
