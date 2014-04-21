using Ashod;
using System;
using System.Collections.Generic;

namespace CSGORanks
{
	public enum Rank
	{
		Silver1,
		Silver2,
		Silver3,
		Silver4,
		SilverElite,
		SilverEliteMaster,
		GoldNova1,
		GoldNova2,
		GoldNova3,
		GoldNovaMaster,
		MasterGuardian1,
		MasterGuardian2,
		MasterGuardianElite,
		DistinguishedMasterGuardian,
		LegendaryEagle,
		LegendaryEagleMaster,
		SupremeMasterFirstClass,
		TheGlobalElite,
	}

	public class Statistics
	{
		public string LastPage;
		public SerializableDictionary<Rank, int> RankDistribution = new SerializableDictionary<Rank, int>();
		public HashSet<string> CommentsAnalyzed = new HashSet<string>();
		public SerializableDictionary<string, Rank> UserRanks = new SerializableDictionary<string, Rank>();

		public static Rank? GetRankFromString(string rankString)
		{
			switch (rankString)
			{
				case "Silver I":
					return Rank.Silver1;

				case "Silver II":
					return Rank.Silver2;

				case "Silver III":
					return Rank.Silver3;

				case "Silver IV":
					return Rank.Silver4;

				case "Silver Elite":
					return Rank.SilverElite;

				case "Silver Elite Master":
					return Rank.SilverEliteMaster;

				case "Gold Nova I":
					return Rank.GoldNova1;

				case "Gold Nova II":
					return Rank.GoldNova2;

				case "Gold Nova III":
					return Rank.GoldNova3;

				case "Gold Nova Master":
					return Rank.GoldNovaMaster;

				case "Master Guardian I":
					return Rank.MasterGuardian1;

				case "Master Guardian II":
					return Rank.MasterGuardian2;

				case "Master Guardian Elite":
					return Rank.MasterGuardianElite;

				case "Distinguished Master Guardian":
					return Rank.DistinguishedMasterGuardian;

				case "Legendary Eagle":
					return Rank.LegendaryEagle;

				case "Legendary Eagle Master":
					return Rank.LegendaryEagleMaster;

				case "Supreme Master First Class":
					return Rank.SupremeMasterFirstClass;

				case "The Global Elite":
					return Rank.TheGlobalElite;

				default:
					return null;
			}
		}
	}
}
