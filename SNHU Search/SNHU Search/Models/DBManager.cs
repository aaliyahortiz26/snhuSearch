using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Linq;
using System.IO;
using System.Threading;

namespace SNHU_Search.Models
{
    public class DBManager
    {
        public string ConnectionString { get; set; }

        public DBManager(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        #region Login/Create Account


        #endregion
    }
}
