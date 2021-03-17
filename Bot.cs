using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

class Bot {
	
	DiscordClient client;
	DiscordChannel relay;
	string[] hearts = new string[] { ":sparkling_heart:", ":revolving_hearts:", ":blue_heart:" };
	string[] rollPrefixes = new string[] { "roll ", "please roll,", "r ", "<@551378788286464000>" };
	Random random = new Random();
	Random funRandom = new Random();
	
	internal Bot(string token) {
		DiscordConfiguration config = new DiscordConfiguration() {
			Token = token,
			TokenType = TokenType.Bot
		};
		client = new DiscordClient(config);
		Main().GetAwaiter().GetResult();
	}
	
	Task MessageCreated(DiscordClient client, MessageCreateEventArgs e) {
		if (e.Author.Equals(client.CurrentUser)) return null;
		Log(e.Message);
		if (ParsePing(e.Message)) return null;
		if (ParseRoll(e.Message)) return null;
		return null;
	}
	
	async Task Main() {
		await client.ConnectAsync();
		relay = await client.GetChannelAsync(782245881625182269);
		await client.UpdateStatusAsync(activity: new DiscordActivity("DMs cry", ActivityType.Watching), userStatus: UserStatus.DoNotDisturb);
		client.MessageCreated += MessageCreated;
		await Task.Delay(-1);
	}
	
	void Log(DiscordMessage message) {
		if (message.Channel.Name != null || message.Channel.Equals(relay)) return;
		relay.SendMessageAsync($"{message.Author.Username}#{message.Author.Discriminator}: {message.Content}");
	}
	
	bool ParseRoll(DiscordMessage message) {
		bool forced = false;
		string content = new string(message.Content.Where(i => !char.IsPunctuation(i) || i == '-').ToArray()).ToLower();
		foreach (string prefix in rollPrefixes) {
			if (content.StartsWith(prefix)) {
				forced = true;
				break;
			}
		}
		if (content.Equals("r") || content.Equals("roll")) forced = true;
		content.Replace('+', ' ');
		string[] args = content.Split(' ');
		List<Dice> dices = new List<Dice>();
		bool[] parsables = new bool[args.Length];
		int mod = 0, repeats = 0, parse;
		for (int i = 0; i < args.Length; i++) {
			Dice dice = ParseDice(args[i]);
			if (dice != null) {
				dices.Add(dice);
				parsables[i] = true;
			} else if (int.TryParse(args[i], out parse)) {
				mod += parse;
				parsables[i] = true;
			} else if (args[i].Contains('x')) {
				parse = ParseRepeats(args[i]);
				if (parse != -1) {
					repeats += parse;
					parsables[i] = true;
				}
			}
		}
		repeats = repeats < 1 ? 1 : repeats;
		if (dices.Count == 0) dices.Add(new Dice(1, 20, random));
		Console.WriteLine($"{forced} {parsables.All(i => i)} {mod} {repeats} {dices.Count}");
		if (forced || (parsables.All(i => i) && parsables.Length > 1)) {
			PrintRoll(message, new Roll(dices, mod), repeats);
			return true;
		} else return false;
	}
	
	void PrintRoll(DiscordMessage message, Roll roll, int repeats) {
		Console.WriteLine("printing");
		StringBuilder text = new StringBuilder();
		for (int i = 0; i < repeats; i++) {
			text.Append($"{roll} \n");
		}
		message.RespondAsync(text.ToString());
	}
	
	Dice ParseDice(string input) {
		string[] splits = input.Split('d');
		if (splits.Length != 2) return null;
		int repeats = 0, size = 0;
		if (input.StartsWith("d") && int.TryParse(splits[1], out size)) return new Dice(1, size, random);
		if (!(int.TryParse(splits[0], out repeats) && int.TryParse(splits[1], out size))) return null;
		return new Dice(repeats, size, random);
	}
	
	int ParseRepeats(string input) {
		if (input.Equals("x")) return 2;
		string[] splits = input.Split('x');
		Console.WriteLine($"splits {splits.Length} {splits[1]}");
		if (splits.Length != 2 || !(splits[0].Equals("") ^ splits[1].Equals(""))) return -1;
		int parse;
		if (int.TryParse(splits[0], out parse)) return parse;
		if (int.TryParse(splits[1], out parse)) return parse;
		return -1;
	}
	
	bool ParsePing(DiscordMessage message) {
		string content = message.Content;
		if (!content.StartsWith("boop ") && !content.Equals("boop")) return false;
		message.RespondAsync(hearts[funRandom.Next(hearts.Length)]);
		return true;
	}
	
	// 	string[] rollStr = new string[repeats]; // should be a regular string
	// 	int[] results = new int[repeats];
	// 	string resultStr = results.Length > 1 ? "**Results:** " : "**Result:** ";
	// 	bool critical = false, critFail = false, max = false, min = false;
	// 	for (int i = 0; i < repeats; i++) {
	// 		max = true; min = true;
	// 		for (int j = 0; j < rolls.Count; j++) {
	// 			if (rolls[j].Critical) critical = true;
	// 			if (rolls[j].CritFail) critFail = true;
	// 			if (rolls[j].result != rolls[j].repeats) min = false;
	// 			if (rolls[j].result != rolls[j].repeats * rolls[j].size) max = false;
	// 			results[i] += rolls[j].result;
	// 			rolls[j].Reroll();
	// 			Console.WriteLine(j);
	// 			// if (j < 4) {
	// 			rollStr[i] += $"{rolls[j].input} ";
	// 			if (j != rolls.Count - 1) rollStr[i] += "+ ";
	// 			// }
	// 			// if (j == 4) rollStr[i] += "... ";
	// 		}
	// 		if (mod < 0) rollStr[i] += $"- {-mod}";
	// 		else if (mod > 0) rollStr[i] += $"+ {mod}";
	// 		results[i] += mod;
	// 		if (max ^ min) resultStr += $"**{results[i]}**";
	// 		else resultStr += $"{results[i]}";
	// 		if (i != repeats - 1) resultStr += ", ";
	// 	}
	//
	// 	string message = "";
	// 	for (int i = 0; i < repeats; i++) {
	// 		if (i < 4) message += $"**Roll:** {rollStr[i]}\n";
	// 		if (i == 4) message += "...\n";
	// 	}
	// 	int total = 0;
	// 	if (repeats > 1) {
	// 		foreach (int result in results) {
	// 			total += result;
	// 		}
	// 	}
	// 	if (repeats == 1 && rolls.Count == 1 && rolls[0].repeats == 1 && mod == 0) await e.Message.RespondAsync(message);
	// 	else {
	// 		if (total == 0) await e.Message.RespondAsync(message + resultStr);
	// 		else await e.Message.RespondAsync(message + resultStr + $"\n**Total:** {total}\n");
	// 		// if (total == 0) await e.Message.RespondAsync(message + resultStr);
	// 		// else await e.Message.RespondAsync(message + resultStr + $"\n**Total:** {total}\n");
	// 	}
	// 	if (!(critical ^ critFail)) return;
	// 	// if (critical) await e.Message.RespondAsync("https://cdn.discordapp.com/emojis/781352087123787797.png");
	// 	if (critical) await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/820807418807582801/821439706432274452/image0.png");
	// 	// if (critFail) await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/781613898662019154/783040445113696326/washawash.png");
	// 	if (critFail) await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/820807418807582801/821439651457269800/image0.png");
	// };
	//
	// 	await client.ConnectAsync();
	// 	await Task.Delay(-1);
	// }
}
