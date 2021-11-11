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
		public bool SaveUser(SignupModel SignUpM)
		{
			bool bRet = false;
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand CheckUser = conn.CreateCommand();

				// Checks to see if there are duplicate usernames
				CheckUser.Parameters.AddWithValue("@username", SignUpM.UserName);
				CheckUser.CommandText = "select count(*) from SNHUSearch.Accounts_tbl where username = @username";

				// if 1 then already exists
				int UserExist = Convert.ToInt32(CheckUser.ExecuteScalar());

				if (UserExist >= 1)
				{
					bRet = true;
				}
				else
				{
					// Hash password
					SignUpM.Password = BCrypt.Net.BCrypt.HashPassword(SignUpM.Password);
					SignUpM.ConfirmPassword = BCrypt.Net.BCrypt.HashPassword(SignUpM.ConfirmPassword);

					MySqlCommand Query = conn.CreateCommand();
					Query.CommandText = "insert into SNHUSearch.Accounts_tbl (firstname, lastname, username, email, password) VALUES (@firstname,@lastname, @username, @email, @password)";
					Query.Parameters.AddWithValue("@firstname", SignUpM.FirstName);
					Query.Parameters.AddWithValue("@lastname", SignUpM.LastName);
					Query.Parameters.AddWithValue("@username", SignUpM.UserName);
					Query.Parameters.AddWithValue("@email", SignUpM.Email);
					Query.Parameters.AddWithValue("@password", SignUpM.Password);

					Query.ExecuteNonQuery();
				}
			}
			return bRet;
		}

		// Get userid from database
		public int GetUserID(string sUsername)
		{
			int userID = -1;
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand FindUser = conn.CreateCommand();

				// Checks to see if there are duplicate usernames
				FindUser.Parameters.AddWithValue("@username", sUsername);
				FindUser.CommandText = "SELECT userId FROM SNHUSearch.Accounts_tbl where username = @username";

				// Execute the SQL command against the DB:
				MySqlDataReader reader = FindUser.ExecuteReader();
				if (reader.Read()) // Read returns false if the user does not exist!
				{
					// Read the DB values:
					Object[] values = new object[1];
					int fieldCount = reader.GetValues(values);
					if (1 == fieldCount)
					{
						// Successfully retrieved the user from the DB:
						userID = userID = Convert.ToInt32(values[0]);
					}
				}
				reader.Close();
			}
			return userID;
		}
		// Load user after pressing login button on login page
		public bool LoadUser(LoginModel dbUser, ref int userid, string sUsername)
		{
			bool bRet = false;

			// Checks the username and password for Login Screen
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand CheckData = conn.CreateCommand();

				// Checks to see if there are duplicate usernames
				CheckData.Parameters.AddWithValue("@username", sUsername);
				CheckData.CommandText = "SELECT userId, password FROM SNHUSearch.Accounts_tbl where username = @username";

				// Execute the SQL command against the DB:
				MySqlDataReader reader = CheckData.ExecuteReader();
				if (reader.Read()) // Read returns false if the user does not exist!
				{
					// Read the DB values:
					Object[] values = new object[2];
					int fieldCount = reader.GetValues(values);
					if (2 == fieldCount) 
					{
						// Successfully retrieved the user from the DB:
						userid = Convert.ToInt32(values[0]);
						dbUser.Password = values[1].ToString();

						bRet = true;
					}
					else
                    {
						// User does not exist
						bRet = false;
                    }
				}
				reader.Close();
			}

			return bRet;
		}

		// Checks if password matches with username
		public bool CheckPassword(LoginModel loginUser, string sUsername, int nUserID)
		{
			bool bRet = false;
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				// Checks the username and password for Login Screen
				MySqlCommand CheckData = conn.CreateCommand();
				CheckData.Parameters.AddWithValue("@username", sUsername);
				CheckData.CommandText = "SELECT userId, password FROM SNHUSearch.Accounts_tbl where username = @username";

				// Execute the SQL command against the DB:
				MySqlDataReader reader = CheckData.ExecuteReader();
				if (reader.Read()) // Read returns false if the user does not exist!
				{
					// Read the DB values:
					Object[] values = new object[2];
					int fieldCount = reader.GetValues(values);
					if (2 == fieldCount)
					{
						// Successfully retrieved the user from the DB:
						nUserID = Convert.ToInt32(values[0]);
						string password = Convert.ToString(values[1]);

						bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginUser.Password, password);
						if (isValidPassword)
						{
							bRet = true;
						}
					}
					else
                    {
						bRet = false;
					}
				}
				reader.Close();
			}
			return bRet;
		}

		public bool SaveWebsite(string websiteURL, string username)
        {
			using (MySqlConnection conn = GetConnection())
            {
				conn.Open();
				MySqlCommand term = conn.CreateCommand();

				// Let's make sure the website doesn't already exist first
				term.Parameters.AddWithValue("@websiteURL", websiteURL);
				term.CommandText = "SELECT EXISTS(SELECT * FROM SNHUSearch.websites WHERE url = @websiteURL)";
				int alreadyExists = Convert.ToInt32(term.ExecuteScalar());

				if (alreadyExists > 0)
                {
					// Already is on list
					return true;
                } 
				else
                {
					// Get current highest ID of website
					term.CommandText = "SELECT MAX(urlID) FROM websites";
					int newCount = Convert.ToInt32(term.ExecuteScalar());
					newCount++;

					// Add the website to the list with a new ID
					term.Parameters.AddWithValue("@newCount", newCount);
					term.CommandText = "INSERT INTO SNHUSearch.websites (url, urlID) VALUES (@websiteURL, @newCount)"; // Adds to global list

					MySqlDataReader reader = term.ExecuteReader();
					if (reader.Read()) // Successful insert into column
					{
						reader.Close();
						return true;
					}
					else
					{
						reader.Close();
						SaveToUser(newCount, username);
						return true;
					}
				}
			}
        }

		private bool SaveToUser(int newWebId, string username)
        {
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand term = conn.CreateCommand();

				// Pull current websites, and then add
				term.Parameters.AddWithValue("@username", username);
				term.CommandText = "select SavedWebsites FROM SNHUSearch.Accounts_tbl WHERE username=@username";
				MySqlDataReader reader = term.ExecuteReader();
				
				Object[] values = new object[2];
				if (reader.Read()) // Will not run if there is a null user
                {
					reader.GetValues(values);
				}


				string savedWebsites = values[0].ToString();
				savedWebsites += (Convert.ToString(newWebId) + "/");
				reader.Close();

				term.Parameters.AddWithValue("@SavedWebsites", savedWebsites);
				term.CommandText = "UPDATE SNHUSearch.Accounts_tbl SET SavedWebsites = @SavedWebsites WHERE username=@username";
				term.ExecuteReader();

				return true;
			}
        }

		public List<string> RetrieveUserWebsites(string username)
        {
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand term = conn.CreateCommand();
				term.Parameters.AddWithValue("@username", username);
				term.CommandText = "select SavedWebsites FROM Accounts_tbl WHERE username=@username";
				string savedWebsites = term.ExecuteScalar().ToString();

				List<string> formattedList = new List<string>();
				string currentWebId = "";
				for (int i = 0; i < savedWebsites.Length - 1; i++)
                {
					if (savedWebsites[i + 1] == '/')
                    {
						// Add number to be converted
						currentWebId += savedWebsites[i];
						formattedList.Add(ConvertIdToName(currentWebId));
						currentWebId = "";
                    }
					else
                    {
						// Keep going, not done
						if (savedWebsites[i] == '/')
							continue;
						else
							currentWebId += savedWebsites[i];
                    }
                }
				return formattedList;
			}
		}

		// Internal class to convert ID value to matching value on website table list and extract weblist name
		private string ConvertIdToName(string IdValue)
		{
			// ID value will be something like '100' leading to the 100th index in the websites table, and we will return that value
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand term = conn.CreateCommand();
				// find matching ID in global web list:
				term.Parameters.AddWithValue("@urlID", IdValue);
				term.CommandText = "SELECT url FROM SNHUSearch.websites WHERE urlID = @urlID";
				string fetchedWebsite = term.ExecuteScalar().ToString();

				return fetchedWebsite;
			}
		}
		#endregion
	}
}