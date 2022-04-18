using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SNHU_Search.Models
{
    public class AnalyticsModel
    {
        public string Keyword { get; set; }
        public int CountOfKeyword { get; set; }

        public AnalyticsModel(string Keyword, int CountKeyword)
        {
            this.Keyword = Keyword;
            this.CountOfKeyword = CountKeyword;
        }
    }
}
