using Ashod;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace CSGORanks
{
	class Scraper
	{
		private const string StatisticsFile = "Statistics.xml";

		private Statistics Statistics;
		private bool Terminating = false;

		public void Run()
		{
			var thread = new Thread(() => RunThread());
			thread.Start();
			while (!Terminating)
			{
				var command = Console.ReadLine();
				switch (command)
				{
					case "stop":
						Logger.Log("Terminating");
						Terminating = true;
						break;
				}
			}
			thread.Join();
			PrintStatistics();
			Console.ReadLine();
		}

		private void RunThread()
		{
			if (File.Exists(StatisticsFile))
			{
				Statistics = XmlFile.Read<Statistics>(StatisticsFile);
				ProcessPages(Statistics.LastPage);
			}
			else
			{
				Statistics = new Statistics();
				ProcessPages("http://www.reddit.com/r/GlobalOffensive/top/?sort=top&t=all");
			}
		}

		private void ProcessPages(string initialUri)
		{
			var currentUri = initialUri;
			while (!Terminating)
			{
				string nextUri = null;
				ProcessPage(currentUri, ref nextUri);
				currentUri = nextUri;
			}
		}

		private void ProcessPage(string uri, ref string nextUri)
		{
			try
			{
				Logger.Log("Processing page: {0}", uri);
				Statistics.LastPage = uri;
				string content = Download(uri);
				var document = new HtmlDocument();
				document.LoadHtml(content);
				var nextNode = document.DocumentNode.SelectSingleNode("//a[@rel = 'nofollow next']");
				if (nextNode == null)
					throw new ApplicationException("Unable to detect next page");
				nextUri = nextNode.Attributes["href"].Value;
				var nodes = document.DocumentNode.SelectNodes("//a[@class = 'comments may-blank']");
				foreach (var node in nodes)
				{
					if (Terminating)
						return;
					string link = node.Attributes["href"].Value;
					if (Statistics.CommentsAnalyzed.Contains(link))
					{
						Logger.Log("Skipping comments: {0}", link);
						continue;
					}
					ProcessComments(link);
				}
			}
			catch (Exception exception)
			{
				Logger.Exception("Failed to process page {0}", exception, uri);
				Terminating = true;
			}
		}

		private void ProcessComments(string uri)
		{
			Logger.Log("Processing comments: {0}", uri);
			string content = Download(uri);
			var document = new HtmlDocument();
			document.LoadHtml(content);
			var userNodes = document.DocumentNode.SelectNodes("//p[@class = 'tagline']");
			int newUsers = 0;
			foreach (var userNode in userNodes)
			{
				try
				{
					var nameNode = userNode.SelectSingleNode(".//a[starts-with(@class, 'author ')]");
					if (nameNode == null)
					{
						// The post has likely been deleted
						continue;
					}
					string username = nameNode.InnerText;
					var rankNode = userNode.SelectSingleNode(".//span[starts-with(@class, 'flair ')]");
					if (rankNode == null)
					{
						// This likely means that the user has not specified a flair
						continue;
					}
					string rankString = rankNode.InnerText;
					var maybeRank = Statistics.GetRankFromString(rankString);
					if (maybeRank == null)
					{
						// This flair likely does not correspond to a CS:GO rank
						continue;
					}
					var rank = maybeRank.Value;
					var userRanks = Statistics.UserRanks;
					if (!userRanks.ContainsKey(username))
					{
						userRanks[username] = rank;
						var rankDistribution = Statistics.RankDistribution;
						if (!rankDistribution.ContainsKey(rank))
							rankDistribution[rank] = 0;
						rankDistribution[rank]++;
						newUsers++;
					}
				}
				catch (Exception exception)
				{
					Logger.Exception("Failed to process user", exception);
				}
			}
			Statistics.CommentsAnalyzed.Add(uri);
			Logger.Log("Detected {0} new user(s)", newUsers);
			XmlFile.Write(Statistics, StatisticsFile);
		}

		private string Download(string uri)
		{
			var client = new WebClient();
			string output = client.DownloadString(new Uri(uri));
			return output;
		}

		private void PrintStatistics()
		{
			int totalCount = 0;
			foreach (var count in Statistics.RankDistribution.Values)
				totalCount += count;
			int previousCount = 0;
			foreach (var rankObject in Enum.GetValues(typeof(Rank)))
			{
				var rank = (Rank)rankObject;
				int count = Statistics.RankDistribution[rank];
				previousCount += count;
				double percentage = GetPercentage(count, totalCount);
				double topPercentile = GetPercentage(previousCount, totalCount);
				Logger.Log("{0}: {1:F1}% of population, top percentile {2:F1}%", rank, percentage, topPercentile);
			}
			Logger.Log("Samples: {0}", totalCount);
		}

		private double GetPercentage(int numerator, int denominator)
		{
			return (double)numerator / denominator * 100.0;
		}
	}
}
