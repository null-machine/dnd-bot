using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordBot {
	class Bot {

		static DiscordClient client;
		static DiscordChannel relay;
		static string[] names = new string[] { "bot", "bhot" };
		static string[] hearts = new string[] { ":sparkling_heart:", ":revolving_hearts:" };
		static Random random = new Random();

		// add thank you responses
		// truncate roll history to first three
		// highlight maxes and mins in results
		// add CheckContext()
		// make main static instead of the whole ass bot static

		static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

		static Roll TestRoll(string split) {
			string[] splits = split.Split('d');
			int repeats = 0, size = 0;
			if (splits.Length != 2) return null;
			if (split.StartsWith("d") && int.TryParse(splits[1], out size)) return new Roll(1, size);
			if (!(int.TryParse(splits[0], out repeats) && int.TryParse(splits[1], out size))) return null;
			return new Roll(repeats, size);
		}

		static bool CheckCommand(string content) {
			string[] segments = content.Split(' ');
			string[] prefixes = new string[] { "roll", "please roll" };
			string[] triggers = new string[] { "to", "for", "a", "an", "please" };
			foreach (string prefix in prefixes) if (content.StartsWith(prefix)) return true;
			foreach (string trigger in triggers) if (content.Contains($"roll {trigger}")) return true;
			if (content.Contains("roll")) {
				foreach (string name in names) {
					if (content.Contains($" {name}") || content.StartsWith(name)) {
						return true;
					}
				}
			}
			if (TestRoll(segments[0]) != null) return true;
			return false;
		}

		static bool CheckBoop(string content) {
			if (content.StartsWith("boop")) return true;
			foreach (string name in names) if (content.Contains("boop") && (content.EndsWith("!") || content.Contains(name))) return true;
			if (content.Contains("a boop") || content.Contains("boop for")) return true;
			return false;
		}

		static async void LogMessage(DiscordMessage message) {
			string log= $"`{message.Author.Username}#{message.Author.Discriminator} ";
			log += $"{message.Channel?.Name} " ?? "";
			log += $"{message.Channel.Guild?.Name} " ?? "";
			log = $"{log.Trim()}`\n{message.Content}";
			await relay.SendMessageAsync(log);
		}

		static async Task MainAsync() {
			Console.WriteLine("bot online!!");
			DiscordConfiguration config = new DiscordConfiguration() {
				Token = "NTUxMzc4Nzg4Mjg2NDY0MDAw.XHp1gw.ewM9OUP4IhPvceDj8JBn-sYNk3s"
			};
			client = new DiscordClient(config);
			relay = await client.GetChannelAsync(782245881625182269);

			client.MessageCreated += async e => {
				if (e.Channel == relay) return;
				LogMessage(e.Message);

				string content = e.Message.Content.ToLower();
				string[] segments = e.Message.Content.Split(' ');
				for (int i = 0; i < segments.Length; i++) segments[i] = new string(segments[i].Where(j => !char.IsPunctuation(j)).ToArray());
				if (CheckBoop(content)) await e.Message.RespondAsync(hearts[random.Next(hearts.Length)]);
				if (content.StartsWith("nya") || content.Contains(" nya")) await e.Message.RespondAsync("https://cdn.discordapp.com/emojis/781352087123787797.png");
				if (CheckCommand(content)) {
					int mod = 0, repeats = 0, parse;
					List<Roll> rolls = new List<Roll>();
					Roll roll;
					foreach (string param in segments) {
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
					bool critical = false;
					for (int i = 0; i < repeats; i++) {
						for (int j = 0; j < rolls.Count; j++) {
							if (rolls[j].Critical) critical = true;
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
					if (critical) await e.Message.RespondAsync("https://cdn.discordapp.com/emojis/781352087123787797.png");
				}

			};

			await client.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}
