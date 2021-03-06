﻿using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
//using ConsoleAppHelper;
using shelltwitlib;
using shelltwitlib.Helpers;
using System.Collections.Generic;

namespace shelltwit
{
	class Program
	{
		const string CLEAR = "/c";
		const string TIME_LINE = "/tl";
		const string HELP = "/?";
		const string MENTIONS = "/m";

		public const string CONSUMER_KEY = "<<ENTER CONSUMER KEY>>";
		public const string CONSUMER_SECRET = "<<ENTER CONSuMER SECRET>>";

		static void Main(string[] args)
		{
			try
			{
				//Debug.Assert(false, "Attach VS here!");

				//http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
				//AppDomain.CurrentDomain.AssemblyResolve += (sender, arg) => { 
				//	string resourceName = "AssemblyLoadingAndReflection." + 
				//	new AssemblyName(arg.Name).Name + ".dll"; 
					
				//	using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) { 
				//		Byte[] assemblyData = new Byte[stream.Length]; 
				//		stream.Read(assemblyData, 0, assemblyData.Length); 
				//		return Assembly.Load(assemblyData); 
				//	}
				//};

				shelltwitlib.API.OAuth.OAuthHelper.Initilize(CONSUMER_KEY, CONSUMER_SECRET);

				if (args.Length == 0)
				{
					PrintTwits(shelltwitlib.API.Tweets.Timeline.GetTimeline());
					return;
				}


				if (args[0].StartsWith("/"))
				{
					string flag = args[0].ToLower().Trim();
					switch (flag)
					{
						case CLEAR:
							TwUser.ClearCredentials();
							Console.WriteLine("User credentials cleared!");
							return;
						case HELP:
							ShowUsage();
							return;
						case TIME_LINE:
							PrintTwits(shelltwitlib.API.Tweets.Timeline.GetTimeline());
							return;
						case MENTIONS:
							PrintTwits(shelltwitlib.API.Tweets.Mentions.GetMentions());
							return;
						default:
							Console.WriteLine("Invalid flag: " + flag);
							ShowUsage();
							return;
					}
				}

				if (args[0].StartsWith("\\"))
				{
					Console.WriteLine("Really? do you really wanna twit that?. [T]wit, or [N]o sorry, I messed up...");
					ConsoleKeyInfo input = Console.ReadKey();
					while (input.Key != ConsoleKey.T && input.Key != ConsoleKey.N)
					{
						Console.WriteLine();
						Console.WriteLine("[T]wit, or [N]o sorry, I messed up...");
						input = Console.ReadKey();
					}
					Console.WriteLine();
		
					if (input.Key == ConsoleKey.N)
					{
						Console.WriteLine("That's what I thought!");
						return;
					}
				}

				shelltwitlib.API.OAuth.OAuthHelper.Initilize(CONSUMER_KEY, CONSUMER_SECRET);
				string status = BitLyHelper.Util.GetShortenString(args);
				string response = shelltwitlib.API.Tweets.Update.UpdateStatus(status);

				if (response != "OK")
					Console.WriteLine("Response was not OK: " + response);
				//ConsoleWriter.WriteWarning("Response was not OK: " + response);
			}
			catch (WebException wex)
			{
				//ConsoleWriter.WriteError(wex.Message);
				Console.WriteLine(wex.Message);

				HttpWebResponse res = (HttpWebResponse)wex.Response;
				if (res != null)
				{
					UpdateError errors = UpdateError.GetFromStream(res.GetResponseStream());
					errors.Errors.ForEach(e => Console.WriteLine(e.ToString())/* ConsoleWriter.WriteWarning(e.ToString())*/);
				}
			}
			catch (Exception ex)
			{
				//ConsoleWriter.WriteError(ex.Message);
				Console.WriteLine(ex.Message);
			}
			finally
			{
#if DEBUG
				if (Debugger.IsAttached)
				{
					//ConsoleWriter.WriteWarning("Press <enter> to exit...");
					Console.WriteLine("Press <enter> to exit...");
					Console.ReadLine();

				}
#endif
			}

			Environment.Exit(0);
		}

		static void PrintTwits(List<Status> twits)
		{
			twits.ForEach(twit => Console.WriteLine("{0} (@{1}): {2}", twit.User.Name, twit.User.ScreenName,  twit.Text));
		}

		static void ShowUsage()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			object[] assemblyAtt = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
			object[] assemblyCop = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			string title = ((AssemblyTitleAttribute)assemblyAtt[0]).Title;
			string copyRight = ((AssemblyCopyrightAttribute)assemblyCop[0]).Copyright;
			string version = assembly.GetName().Version.ToString();

			Console.WriteLine(title);
			Console.WriteLine(string.Format("{0} v{1}", copyRight, version));
			Console.WriteLine("");
			Console.WriteLine("Usage: twit [/c|/tl|/m|/?|status] [<mediaPath>]");
			Console.WriteLine("");
			Console.WriteLine("/c 		: clears user stored credentials");
			Console.WriteLine("/tl 		: show user's timeline");
			Console.WriteLine("/m 		: show user's mentions");
			Console.WriteLine("/? 		: show this help");
			Console.WriteLine("status	 	: status to update at twitter.com");
			Console.WriteLine("mediaPath	: full path, between brackets, to the media file to upload.");
			Console.WriteLine("");

		}
	}
}
