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
	Random random = new Random();
	Random funRandom = new Random();
	Macros macros;
	
	internal Bot(string token, Macros macros) {
		DiscordConfiguration config = new DiscordConfiguration() {
			Token = token,
			TokenType = TokenType.Bot
		};
		client = new DiscordClient(config);
		this.macros = macros;
		Main().GetAwaiter().GetResult();
	}
	
	Task MessageCreated(DiscordClient client, MessageCreateEventArgs e) {
		if (e.Author.Equals(client.CurrentUser)) return null;
		Log(e.Message);
		
		string line = e.Message.Content.Replace('+', ' ').ToLower();
		line = new string(line.Where(i => !char.IsPunctuation(i) || i == '-').ToArray());
		List<string> args = line.Split(' ').Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
		if (args.Count == 0) return null;
		if (ParsePing(e.Message, args)) return null;
		if (macros.ParseView(e.Message, args)) return null;
		if (macros.ParseSave(e.Message, args)) return null;
		if (ParseRoll(e.Message, args)) return null;
		return null;
	}
	
	async Task Main() {
		await client.ConnectAsync();
		relay = await client.GetChannelAsync(782245881625182269);
		client.MessageCreated += MessageCreated;
		new StatusCycler(client, funRandom);
		await Task.Delay(-1);
	}
	
	void Log(DiscordMessage message) {
		if (message.Channel.Name != null || message.Channel.Equals(relay)) return;
		relay.SendMessageAsync($"{message.Author.Username}#{message.Author.Discriminator}: {message.Content}");
	}
	
	bool ParseRoll(DiscordMessage message, List<string> args) {
		bool forced = false;
		if (args[0].Equals("r") || args[0].Equals("roll")) {
			forced = true;
			args.RemoveAt(0);
		}
		if (args.Count == 1) {
			Dice singleDice = ParseDice(args[0]);
			if (singleDice != null) forced = true;
		}
		
		if (forced) macros.Replace(message.Author.Id, args);
		
		List<Dice> dices = new List<Dice>();
		bool[] parsables = new bool[args.Count]; // TODO can be swapped for a count
		int mod = 0, repeats = 0, parse;
		for (int i = 0; i < args.Count; i++) {
			Dice dice = ParseDice(args[i]);
			if (string.IsNullOrEmpty(args[i])) {
				parsables[i] = true;
			} else if (dice != null) {
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
		if (forced || (parsables.All(i => i) && parsables.Length > 1)) {
			PrintRoll(message, new Roll(dices, mod), repeats);
			return true;
		} else return false;
	}
	
	void PrintRoll(DiscordMessage message, Roll roll, int repeats) {
		StringBuilder text = new StringBuilder();
		BoldInt[] results = new BoldInt[repeats];
		bool truncated = false, multiple = repeats > 1;
		for (int i = 0; i < repeats; i++) {
			results[i] = roll.Reroll();
			if (i < 4) text.Append($"{roll}\n");
			else truncated = true;
		}
		if (truncated) text.Append("...\n");
		if (multiple) text.Append("**Results:** ");
		else text.Append("**Result:** ");
		if (repeats <= 20) {
			text.Append($"{string.Join(", ", results.Select(i => i.ToString()).ToArray())} ");
		} else {
			text.Append($"{string.Join(", ", results.Take(20).Select(i => i.ToString()).ToArray())} ... ");
		}
		int total = results.Sum(i => i.value);
		BoldInt displayTotal = new BoldInt(total, total == roll.min * repeats || total == roll.max * repeats);
		if (multiple) text.Append($"| **Total:** {displayTotal}");
		DiscordMessageBuilder reply = new DiscordMessageBuilder() {
			Content = text.ToString()
		};
		reply.WithReply(message.Id);
		message.RespondAsync(reply);
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
		if (splits.Length != 2 || !(splits[0].Equals("") ^ splits[1].Equals(""))) return -1;
		int parse;
		if (int.TryParse(splits[0], out parse)) return parse;
		if (int.TryParse(splits[1], out parse)) return parse;
		return -1;
	}
	
	bool ParsePing(DiscordMessage message, List<string> args) {
		if (!args[0].Equals("boop")) return false;
		if (funRandom.Next(100) == 0) message.CreateReactionAsync(DiscordEmoji.FromName(client, ":black_heart:"));
		else message.CreateReactionAsync(DiscordEmoji.FromName(client, hearts[funRandom.Next(hearts.Length)]));
		return true;
	}
}
