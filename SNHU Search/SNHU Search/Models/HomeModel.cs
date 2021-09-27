using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.ComponentModel.DataAnnotations;

namespace SNHU_Search.Models
{
    public class HomeModel
    {
        public string keywordSearchBar { get; set; }
    }
}
