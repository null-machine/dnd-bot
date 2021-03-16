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
		static string[] hearts = new string[] { ":sparkling_heart:", ":revolving_hearts:", ":blue_heart:" };
		static Random random = new Random();
		
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
			string[] args = content.Split(' ');
			string[] prefixes = new string[] { "roll", "please roll " };
			string[] triggers = new string[] { "to", "for", "a", "an", "please" };
			foreach (string prefix in prefixes) if (content.StartsWith(prefix)) return true;
			foreach (string trigger in triggers) if (content.Contains($" roll {trigger}")) return true;
			if (content.Contains("roll")) {
				foreach (string name in names) {
					if ((content.Contains($" {name}") && content.IndexOf(name) < content.IndexOf("roll")) || content.StartsWith(name)) {
						return true;
					}
				}
			}
			if (TestRoll(new string(args[0].Where(j => !char.IsPunctuation(j) != null).ToArray())) != null) return true;
			return false;
		}
		
		static bool CheckBoop(string content) {
			if (content.StartsWith("boop")) return true;
			foreach (string name in names) {
				if (content.Contains(name) && content.Contains("best")) return true;
				if (content.Contains("boop") && (content.EndsWith("!") || content.Contains(name))) return true;
			}
			if (content.Contains("a boop") || content.Contains("boop for")) return true;
			return false;
		}
		
		static async void LogMessage(DiscordMessage message) {
			if (message.Channel.Equals(relay)) {
				Console.WriteLine("bruh");
				return;
			}
			string log = $"`{message.Author.Username}#{message.Author.Discriminator} ";
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
				string content = e.Message.Content.ToLower();
				// TODO only log dms
				LogMessage(e.Message);
				string[] args = e.Message.Content.ToLower().Split(' ');
				for (int i = 0; i < args.Length; i++) args[i] = new string(args[i].Where(j => !char.IsPunctuation(j) || j == '-').ToArray());
				if (CheckBoop(content)) await e.Message.RespondAsync(hearts[random.Next(hearts.Length)]);
				if (!CheckCommand(content)) return;
				
				int mod = 0, repeats = 0, parse;
				List<Roll> rolls = new List<Roll>();
				Roll roll;
				foreach (string arg in args) {
					if (int.TryParse(arg, out parse)) mod += parse;
					if (arg.Equals("adv") || arg.Equals("dis") || arg.Equals("advantage") || arg.Equals("disadvantage")) repeats += 2;
					if (arg.StartsWith("r")) {
						if (int.TryParse(arg[1..], out parse)) repeats += parse;
						else if (arg[1..].Equals("")) repeats += 2;
					}
					roll = TestRoll(arg);
					if (roll != null) rolls.Add(roll);
				}
				if (repeats < 1) repeats = 1;
				if (rolls.Count == 0) rolls.Add(new Roll(1, 20));
				
				string[] rollStr = new string[repeats]; // should be a regular string
				int[] results = new int[repeats];
				string resultStr = results.Length > 1 ? "**Results:** " : "**Result:** ";
				bool critical = false, critFail = false, max = false, min = false;
				for (int i = 0; i < repeats; i++) {
					max = true; min = true;
					for (int j = 0; j < rolls.Count; j++) {
						if (rolls[j].Critical) critical = true;
						if (rolls[j].CritFail) critFail = true;
						if (rolls[j].result != rolls[j].repeats) min = false;
						if (rolls[j].result != rolls[j].repeats * rolls[j].size) max = false;
						results[i] += rolls[j].result;
						rolls[j].Reroll();
						if (j < 4) {
							rollStr[i] += $"{rolls[j].input} ";
							if (j != rolls.Count - 1) rollStr[i] += "+ ";
						}
						if (j == 4) rollStr[i] += "... ";
					}
					if (mod < 0) rollStr[i] += $"- {-mod}";
					else if (mod > 0) rollStr[i] += $"+ {mod}";
					results[i] += mod;
					if (max ^ min) resultStr += $"**{results[i]}**";
					else resultStr += $"{results[i]}";
					if (i != repeats - 1) resultStr += ", ";
				}
				
				string message = "";
				for (int i = 0; i < repeats; i++) {
					if (i < 4) message += $"**Roll:** {rollStr[i]}\n";
					if (i == 4) message += "...\n";
				}
				int total = 0;
				if (repeats > 1) {
					foreach (int result in results) {
						total += result;
					}
				}
				if (repeats == 1 && rolls.Count == 1 && rolls[0].repeats == 1 && mod == 0) await e.Message.RespondAsync(message);
				else {
					if (total == 0) await e.Message.RespondAsync(message + resultStr);
					else await e.Message.RespondAsync(message + resultStr + $"\n**Total:** {total}\n");
				}
				if (!(critical ^ critFail)) return;
				// if (critical) await e.Message.RespondAsync("https://cdn.discordapp.com/emojis/781352087123787797.png");
				if (critical) await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/820807418807582801/821439706432274452/image0.png");
				// if (critFail) await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/781613898662019154/783040445113696326/washawash.png");
				if (critFail) await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/820807418807582801/821439651457269800/image0.png");
			};
			
			await client.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}
