﻿using System;
using System.IO;
using System.Xml.Serialization;

namespace shelltwitlib
{
	public class TwUser
	{
		const string USER_FILE = "twit.usr";
		const string OAUTH_TOKEN = "oauth_token";
		const string OAUTH_TOKEN_SECRET = "oauth_token_secret";

		static string s_configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), USER_FILE);

		[XmlAttribute]
		public string Username { get; set; }

		[XmlAttribute]
		public string Password { get; set; }

		[XmlAttribute]
		public string OAuthToken { get; set; }

		[XmlAttribute]
		public string OAuthTokenSecret { get; set; }

		public TwUser()
		{
		}

		public TwUser(string user, string password, string token, string tokensecret)
		{
			Username = user;
			Password = password;
			OAuthToken = token;
			OAuthTokenSecret = tokensecret;
		}

		public string GetUrlData()
		{
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", Username, Password)));
		}

		public string GetKey(string consumerKey)
		{
			return "";
		}


		public static TwUser GetUserCrdentials(string username)
		{
			username = username.Replace(Path.DirectorySeparatorChar, '.');

			string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), username);

			if (!File.Exists(userPath))
				return null;

			TwUser twiUser = new TwUser();

			XmlSerializer deserializer = new XmlSerializer(typeof(TwUser));
			using (StreamReader reader = new StreamReader(userPath))
				twiUser = (TwUser)deserializer.Deserialize(reader);

			return twiUser;
		}

		public void SaveUserCredentials(string username)
		{
			username = username.Replace(Path.DirectorySeparatorChar, '.');

			string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), username);

			Serialize(userPath);
		}

		public static TwUser LoadCredentials()
		{
			TwUser twiUser = new TwUser();
			if (!File.Exists(s_configFile))
			{
				Console.WriteLine("Enter Twitter username...");
				twiUser.Username = Console.ReadLine();

				Console.WriteLine("Enter Twitter password...");
				twiUser.Password = Console.ReadLine();
			}
			else
			{
				XmlSerializer deserializer = new XmlSerializer(typeof(TwUser));
				using (StreamReader reader = new StreamReader(s_configFile))
					twiUser = (TwUser)deserializer.Deserialize(reader);
			}

			if (string.IsNullOrEmpty(twiUser.OAuthToken) || string.IsNullOrEmpty(twiUser.OAuthTokenSecret))
				twiUser.SetOAuthCredentials();

			return twiUser;
		}

		public void SetOAuthCredentials()
		{
			string accessToken = shelltwitlib.API.OAuth.OAuthHelper.GetAccessToken(Username, Password);
			string[] tokens = accessToken.Split(new char[] {'&'},StringSplitOptions.RemoveEmptyEntries);

			foreach (string tok in tokens)
			{
				string[] props = tok.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (props[0] != OAUTH_TOKEN && props[0] != OAUTH_TOKEN_SECRET)
					continue;
				
				if (props[0] == OAUTH_TOKEN)
					OAuthToken = props[1];
				else if (props[0] == OAUTH_TOKEN_SECRET)
					OAuthTokenSecret = props[1];

				if (!string.IsNullOrEmpty(OAuthToken) && !string.IsNullOrEmpty(OAuthTokenSecret))
					break;
			}

			Serialize();
		}

		private void Serialize()
		{
			Serialize(s_configFile);
		}

		private void Serialize(string fileName)
		{
			//Do not store user credentials
			Username = null;
			Password = null;

			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add("", "");
			XmlSerializer serializer = new XmlSerializer(typeof(TwUser));
			using (StreamWriter writer = new StreamWriter(fileName))
				serializer.Serialize(writer, this, namespaces);
		}


		public static void ClearCredentials()
		{
			if (File.Exists(s_configFile))
				File.Delete(s_configFile);
		}
	}
}
