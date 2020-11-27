using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;

namespace DiscordBot {

	class Bot {

	// add dynamic commands, e.g. "hey bhot, roll please"
	// add thank you responses

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
				Token = "NTUxMzc4Nzg4Mjg2NDY0MDAw.XHp1gw.ewM9OUP4IhPvceDj8JBn-sYNk3s",
				TokenType = TokenType.Bot
			});

			discord.MessageCreated += async e => {
				Console.WriteLine(e.Message.Content.ToLower());
				if (e.Message.Content.ToLower().StartsWith("🗡️")) await e.Message.RespondAsync(":dagger:");
				if (e.Message.Content.ToLower().StartsWith("boop")) await e.Message.RespondAsync(":sparkling_heart:");
			if (e.Message.Content.ToLower().StartsWith("nya")) await e.Message.RespondAsync("https://cdn.discordapp.com/emojis/781352087123787797.png");
					if (e.Message.Content.ToLower().StartsWith("roll")) {

						List<string> parameters = new List<string>(e.Message.Content.Split(' '));
						int mod = 0, repeats = 0, parse;
						List<Roll> rolls = new List<Roll>();
						Roll roll;
						foreach (string param in parameters) {
							if (int.TryParse(param, out parse)) {
								mod += parse;
							} else if (param.StartsWith("r")) {
								if (int.TryParse(param[1..], out parse)) repeats += parse;
								else if (param[1..].Equals("")) repeats += 2;
							} else if (param.Equals("adv") || param.Equals("dis")) repeats += 2;
							else {
								roll = TestRoll(param);
								if (roll != null) rolls.Add(roll);
							}
						}
						if (repeats < 1) repeats = 1;
						if (repeats > 20) repeats = 10;
						if (rolls.Count == 0) rolls.Add(new Roll(1, 20));

						string[] rollStr = new string[repeats];
						int[] results = new int[repeats];
						for (int i = 0; i < repeats; i++) {
							for (int j = 0; j < rolls.Count; j++) {
								rollStr[i] += $"{rolls[j].input} ";
								if (j != rolls.Count - 1) rollStr[i] += "+ ";
								results[i] += rolls[j].result;
								rolls[j].Reroll();
							}
						}
						string message = "";
						string resultStr = results.Length > 1 ? "**Results:** " : "**Result:** ";
						for (int i = 0; i < repeats; i++) {
							if (mod < 0) rollStr[i] += $"- {-mod}";
							else if (mod > 0) rollStr[i] += $"+ {mod}";
							results[i] += mod;
							message += $"**Roll:** {rollStr[i]}\n";
							resultStr += results[i];
							if (i != repeats - 1) {
								resultStr += ", ";
							}
						}
						//Console.WriteLine("finished " + message + resultStr);
						if (repeats == 1 && rolls.Count == 1 && rolls[0].repeats == 1 && mod == 0) await e.Message.RespondAsync(message);
						else await e.Message.RespondAsync(message + resultStr);
					}
					
			};

			await discord.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}
