using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;

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
		#endregion

		#region Save/ Retrieve User's Websites
		public bool SaveWebsite(string websiteURL, string username)
		{
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand term = conn.CreateCommand();

				// make sure that the website doesn't already exist
				term.Parameters.AddWithValue("@websiteURL", websiteURL);
				term.Parameters.AddWithValue("@userID1", GetUserID(username));
				term.CommandText = "SELECT EXISTS(SELECT * FROM SNHUSearch.websites WHERE (url = @websiteURL and userID = @userID1));";
				int alreadyExists = Convert.ToInt32(term.ExecuteScalar());

				if (alreadyExists > 0)
				{
					// Already is on list
					return false;
				}
				else
				{
					// Add the website to the list with a new ID
					term.Parameters.AddWithValue("@userID", GetUserID(username));

					term.CommandText = "INSERT INTO SNHUSearch.websites (url, userID) VALUES (@websiteURL, @userID)"; // Adds to global list

					MySqlDataReader reader = term.ExecuteReader();
					if (reader.Read()) // Successful insert into column
					{
						reader.Close();
						return true;
					}
					else
					{
						reader.Close();
						return true;
					}
				}
			}
		}

		public bool RemoveWebsite(string websiteURL, string username)
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
					term.CommandText = "Delete FROM SNHUSearch.websites WHERE url = @urlWeb AND userID = @userID";
					term.Parameters.AddWithValue("@urlWeb", websiteURL);
					term.Parameters.AddWithValue("@userID", GetUserID(username));
					term.ExecuteNonQuery();
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public List<string> RetrieveUserWebsites(string username)
		{
			using (MySqlConnection conn = GetConnection())
			{
				conn.Open();
				MySqlCommand term = conn.CreateCommand();
				term.CommandText = "select url FROM SNHUSearch.websites WHERE userID=@userID";
				term.Parameters.AddWithValue("@userID", GetUserID(username));
				MySqlDataReader urlRead = term.ExecuteReader();

				List<string> formattedList = new List<string>();
				while (urlRead.Read())
				{
					formattedList.Add(Convert.ToString(urlRead[0]));
				}
				urlRead.Close();

				return formattedList;
			}
		}
		#endregion

		#region Manage User Account
		public List<string> RetrieveUserInfoFromDB(ProfileModel pm, string userNameS)
		{
			using (MySqlConnection DBconnection = GetConnection())
			{
				DBconnection.Open();
				MySqlCommand CheckData = DBconnection.CreateCommand();
				CheckData.Parameters.AddWithValue("@username", userNameS);
				//grabbing information in the database to display on the profile page for the user
				CheckData.CommandText = "SELECT email, firstName, lastName FROM SNHUSearch.Accounts_tbl where username = @username";

				MySqlDataReader DBreader = CheckData.ExecuteReader();

				List<string> currentUserList = new List<string>();

				while (DBreader.Read())
				{
					currentUserList.Add(Convert.ToString(DBreader[0])); //email
					currentUserList.Add(Convert.ToString(DBreader[1])); //first name
					currentUserList.Add(Convert.ToString(DBreader[2])); //last name
				}

				DBreader.Close();
				return currentUserList;
			}
		}
		public string UserForgetsPassword(ForgetPasswordModel fpM, string email)
		{

			using (MySqlConnection DBconnect = GetConnection())
			{
				DBconnect.Open();
				MySqlCommand CheckData = DBconnect.CreateCommand();
				//CheckData.Parameters.AddWithValue("@email", email);

				return ""; //change this to a different variable
			}
		}
		public void UserChangesPassword(ChangePasswordModel cpM, string username)
		{
			using (MySqlConnection DBconnect = GetConnection())
			{
				DBconnect.Open();

				cpM.userNewPassword = BCrypt.Net.BCrypt.HashPassword(cpM.userNewPassword);
				cpM.userConfirmNewPassword = BCrypt.Net.BCrypt.HashPassword(cpM.userConfirmNewPassword);

				MySqlCommand Query = DBconnect.CreateCommand();
				Query.Parameters.AddWithValue("@newPassword", cpM.userNewPassword);
				Query.Parameters.AddWithValue("@username", username);
				Query.CommandText = "UPDATE SNHUSearch.Accounts_tbl SET password = @newPassword WHERE username = @username";

				Query.ExecuteNonQuery();

				DBconnect.Close();
			}
		}
		#endregion

		public bool URLExist(string website)
		{
			Uri uriResult;
			bool result = Uri.TryCreate(website, UriKind.Absolute, out uriResult)
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

			return result;
		}

		public string getWebsiteTitle(string website)
		{
			string websiteTitle = "";

			try
			{
				Uri uri = new Uri(website);
				// create a request to the website's url
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				using (Stream stream = response.GetResponseStream())
				using (StreamReader reader = new StreamReader(stream))
				{
					var str = reader.ReadToEnd();
					Regex reg = new Regex("<title>(.*)</title>");
					MatchCollection m = reg.Matches(str);
					// title of website exist
					if (m.Count > 0)
					{
						websiteTitle = m[0].Value.Replace("<title>", "").Replace("</title>", "");
					}
					// no title
					else
						return "";
				}
			}
			catch
			{
				websiteTitle = "";
			}

			return websiteTitle;
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

        #region Keywords for Analytics
        public void UploadKeywordForAnalytics(string keyword)
		{
			using (MySqlConnection DBconnect = GetConnection())
			{
				DBconnect.Open();
				MySqlCommand Query = DBconnect.CreateCommand();

				//checks to see if keyword is in table
				Query.Parameters.AddWithValue("@keyword1", keyword);
				Query.CommandText = "SELECT EXISTS(SELECT * FROM SNHUSearch.Analytics_tbl WHERE keyword = @keyword1)";
				int keywordExists = Convert.ToInt32(Query.ExecuteScalar());

				if(keywordExists > 0)
                { //increments the count of how many times the keyword is searched
					Query.Parameters.AddWithValue("@keyword2", keyword);
					Query.CommandText = "UPDATE SNHUSearch.Analytics_tbl SET count = count + " + 1 + " WHERE keyword = @keyword2";
				} 
				else
                {
					int startCountForKeyword = 1;
					Query.Parameters.AddWithValue("@countForKeyword", startCountForKeyword);
					Query.Parameters.AddWithValue("@keyword", keyword);
					Query.CommandText = "INSERT INTO SNHUSearch.Analytics_tbl (keyword, count) VALUES (@keyword, @countForKeyword)"; //adds to the global list
				}

				Query.ExecuteNonQuery();
				DBconnect.Close();
			}
		}

		public List<string> AnalyticKeywordsForUser()
		{
			using (MySqlConnection DBconn = GetConnection())
            {
				DBconn.Open();
				MySqlCommand Query = DBconn.CreateCommand();
				//Query.Parameters.AddWithValue("@userID3", GetUserID(username));
				Query.CommandText = "SELECT keyword, max(count) FROM SNHUSearch.Analytics_tbl GROUP BY keyword ORDER BY count DESC LIMIT 6"; 

				MySqlDataReader DBreader = Query.ExecuteReader();
				List<string> topKeywordsPerUser = new List<string>();

				while (DBreader.Read())
				{
					topKeywordsPerUser.Add(Convert.ToString(DBreader[0]));
				}

				DBconn.Close();
				return topKeywordsPerUser;
			}
		}

		public List<string> AnalyticKeywordsGlobally()
        {
			using (MySqlConnection DBconn = GetConnection())
			{
				DBconn.Open();
				MySqlCommand Query = DBconn.CreateCommand();
				Query.CommandText = "SELECT keyword, max(count) FROM SNHUSearch.Analytics_tbl GROUP BY keyword ORDER BY count DESC LIMIT 6"; 

				MySqlDataReader DBreader = Query.ExecuteReader();
				List<string> topKeywordsGlobally = new List<string>();

				while (DBreader.Read())
				{
					topKeywordsGlobally.Add(Convert.ToString(DBreader[0]));
					topKeywordsGlobally.Add(Convert.ToString(DBreader[1]));
				}

				DBconn.Close();
				return topKeywordsGlobally;
			}
		}
        #endregion
    }
}