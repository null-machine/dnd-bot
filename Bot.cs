using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;

namespace DiscordBot {

	class Bot {

		static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

		static Roll TestRoll(string split) {
			string[] splits = split.Split('d');
			int repeats = 0, size = 0;
			if (splits.Length != 2) return null;
			if (split.StartsWith("d") && int.TryParse(splits[1], out size)) return new Roll(1, size);
			if (!(int.TryParse(splits[0], out repeats) && int.TryParse(splits[1], out size))) return null;
			return new Roll(repeats, size);
		}

		static async Task MainAsync() {
			var discord = new DiscordClient(new DiscordConfiguration() {
				Token = "NTUxMzc4Nzg4Mjg2NDY0MDAw.XHp1gw.ZDzYuw5-XHdPJy-uK8_iN3mybXw",
				TokenType = TokenType.Bot
			});

			discord.MessageCreated += async e => {

				if (e.Message.Content.StartsWith("roll")) {

					HashSet<string> parameters = new HashSet<string>(e.Message.Content.Split(' '));
					int mod = 0, repeats = 0, i;
					bool advantage = parameters.Contains("adv") || parameters.Contains("advantage");
					bool disadvantage = parameters.Contains("dav") || parameters.Contains("disadvantage");
					List<Roll> rolls = new List<Roll>();
					Roll roll;
					foreach (string param in parameters) {
						if (!int.TryParse(param, out i)) {
							roll = TestRoll(param);
							if (roll != null) rolls.Add(roll);
						} else if (param.StartsWith("r") && int.TryParse(param.Substring(1), out i)) {
							repeats += i;
						} else mod += i;

					}
					if (rolls.Count == 0) rolls.Add(new Roll(1, 20));

					string rollStr = "**Roll:** ";
					int result = 0, secondResult;
					for (int i = 0; i < rolls.Count; i++) {
						rollStr += $"{rolls[i].input} ";
						if (i != rolls.Count - 1) rollStr += "+ ";
						result += rolls[i].result;
					}
					//if (advantage ^ disadvantage) {
					//	for (int i = 0; i < rolls.Count; i++) {
					//		rollStr += $"{rolls[i].input} ";
					//		if (i != rolls.Count - 1) rollStr += "+ ";
					//		result += rolls[i].result;
					//	}
					//}
					if (mod < 0) rollStr += $"- {-mod}";
					else if (mod > 0) rollStr += $"+ {mod}";
					result += mod;



					if (rolls.Count == 1 && rolls[0].repeats == 1 && mod == 0) await e.Message.RespondAsync(rollStr);
					else await e.Message.RespondAsync(rollStr + $"\n**Result:** {result}");
				}
			};

			await discord.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}
