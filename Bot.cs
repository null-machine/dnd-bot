using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus;
// using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;

class Bot {

	DiscordClient client;
	string[] hearts = new string[] { ":blue_heart:", ":yellow_heart:", ":heart:", ":purple_heart:" };
	Random funRandom = new Random();
	Dictionary<string, Random> userRandoms = new Dictionary<string, Random>();
	
	DiscordChannel logChannel;
	// DiscordChannel relay;
	// DiscordChannel musicChannel;
	// VoiceNextConnection connection;
	// Queue<string> songs = new Queue<string>();
	// List<string> downloads = new List<string>();
	

	internal Bot(string token) {
		DiscordConfiguration config = new DiscordConfiguration() {
			Token = token,
			TokenType = TokenType.Bot,
			Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
		};
		client = new DiscordClient(config);
		// ReplyGen.Gen("test");
		ReplyGen.Init("ReplyGen/Keywords.data");
		// ReplyGen.Gen("among us i love something");
		// Console.WriteLine(ReplyGen.Gen("hello"));
		// Console.WriteLine(ReplyGen.Gen("i love something"));
		Main().GetAwaiter().GetResult();
	}

	async Task<Task> MessageCreated(DiscordClient client, MessageCreateEventArgs context) {
		if (context.Author.Equals(client.CurrentUser)) return Task.CompletedTask;
		if (context.Guild == null) {
			string log = $"{context.Author.Username} {context.Author.Id}: {context.Message.Content} ";
			foreach (DiscordAttachment attachment in context.Message.Attachments) {
				log += $"\n{attachment.Url}";
			}
			foreach (DiscordEmbed embed in context.Message.Embeds) {
				log += $"\n{embed.Url.AbsoluteUri}";
			}
			return logChannel.SendMessageAsync(log);
		}
		DiscordMessage message = context.Message;
		string line = message.Content.Replace('+', ' ').ToLower();
		line = new string(line.Where(i => !char.IsPunctuation(i) || i == '-').ToArray());
		List<string> args = line.Split(' ').Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
		// TODO maybe make +5 instead of 5
		if (args.Count == 0) return Task.CompletedTask;
		if (ParsePing(message, args)) return Task.CompletedTask;
		if (ParseChain(message, args)) return Task.CompletedTask;
		if (ParseChat(message, args, context) == Task.CompletedTask) return Task.CompletedTask;
		if (await ParseEcho(message, args) == Task.CompletedTask) return Task.CompletedTask; // echo has to be last
		return Task.CompletedTask;
	}

	async Task Main() {
		await client.ConnectAsync();
		client.MessageCreated += MessageCreated;
		client.Ready += Ready;

		// relay = await client.GetChannelAsync(1025223875304374274);
		// new Thread(delegate() { DumpRelay(); }).Start();

		logChannel = await client.GetChannelAsync(1126250005653635154);
		await Task.Delay(-1);
	}

	async Task Ready(DiscordClient client, ReadyEventArgs e) {
		Console.WriteLine("Picaro is online.");
		await client.UpdateStatusAsync(new DiscordActivity("God", ActivityType.Playing), UserStatus.Online);
		// await client.UpdateStatusAsync(new DiscordActivity("maintenance", ActivityType.Competing), UserStatus.DoNotDisturb);
	}

	// void DumpRelay() {
	// 	while (true) {
	// 		try {
	// 			using (FileStream fileStream = File.Open("Relay.txt", FileMode.Open)) {
	// 				StreamReader reader = new StreamReader(fileStream);
	// 				string line;
	// 				while ((line = reader.ReadLine()) != null) {
	// 					relay.SendMessageAsync(line);
	// 					Thread.Sleep(1000);
	// 				}
	// 				fileStream.SetLength(0);
	// 			}
	// 		} catch {
	// 			Thread.Sleep(10000);
	// 		}
	// 		Thread.Sleep(2000);
	// 	}
	// }


	bool ParseChain(DiscordMessage message, List<string> args) {
		bool forced = false;
		if (args[0].Equals("roll")) {
			forced = true;
			args.RemoveAt(0);
		}
		if (args.Count == 1) {
			Dice singleDice = ParseDice(args[0]);
			if (singleDice != null) forced = true;
		}

		List<Dice> dices = new List<Dice>();
		bool[] parsables = new bool[args.Count];
		int mod = 0, repeats = 0, x;
		bool foundSpecial = false;
		for (int i = 0; i < args.Count; i++) {
			Dice dice = ParseDice(args[i]);
			if (string.IsNullOrEmpty(args[i])) {
				parsables[i] = true;
			} else if (dice != null) {
				dices.Add(dice);
				parsables[i] = true;
				foundSpecial = true;
			} else if (int.TryParse(args[i], out x)) {
				mod += x;
				parsables[i] = true;
			} else if (args[i].Contains('x')) {
				x = ParseRepeats(args[i]);
				if (x != -1) {
					repeats += x;
					parsables[i] = true;
					// foundSpecial = true;
				}
			}
		}
		repeats = repeats < 1 ? 1 : repeats;
		if (dices.Count == 0) dices.Add(new Dice(1, 20));
		if (forced || (parsables.All(i => i) && parsables.Length > 1 && foundSpecial)) {
			PrintChain(message, new DiceChain(dices, mod), repeats);
			return true;
		} else return false;
	}

	void PrintChain(DiscordMessage message, DiceChain chain, int repeats) {
		string user = $"{message.Author.Username}#{message.Author.Discriminator}";
		if (!userRandoms.ContainsKey(user)) {
			userRandoms.Add(user, new Random());
			// userRandoms.Add(user, new KarmicRandom());
		}
		Random random = userRandoms[user];
		StringBuilder text = new StringBuilder();
		BoldInt[] results = new BoldInt[repeats];
		bool truncated = false, multiple = repeats > 1;
		for (int i = 0; i < repeats; i++) {
			results[i] = chain.Roll(random);
			if (i < 6) text.Append($"{chain}\n");
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
		BoldInt displayTotal = new BoldInt(total, total == chain.min * repeats || total == chain.max * repeats);
		if (multiple) text.Append($"| **Total:** {displayTotal}");
		DiscordMessageBuilder reply = new DiscordMessageBuilder() {
			Content = text.ToString()
			// Content = "meow"
		};
		reply.WithReply(message.Id);
		message.RespondAsync(reply);
	}

	Dice ParseDice(string input) {
		string[] splits = input.Split('d');
		if (splits.Length != 2) return null;
		int repeats = 0, size = 0;
		if (input.StartsWith("d") && int.TryParse(splits[1], out size)) return new Dice(1, size);
		if (!(int.TryParse(splits[0], out repeats) && int.TryParse(splits[1], out size))) return null;
		return new Dice(repeats, size);
	}

	int ParseRepeats(string input) {
		if (input.Equals("x")) return -1;
		string[] splits = input.Split('x');
		if (splits.Length != 2 || !(splits[0].Equals("") ^ splits[1].Equals(""))) return -1;
		int parse;
		if (int.TryParse(splits[0], out parse)) return parse;
		if (int.TryParse(splits[1], out parse)) return parse;
		return -1;
	}

	bool ParsePing(DiscordMessage message, List<string> args) {
		if (message.Content == "BAD BOT") {
			message.CreateReactionAsync(DiscordEmoji.FromName(client, ":broken_heart:"));
			return true;
		}
		string text = message.Content.ToLower();
		if (text != "ping" && text != "boop" && text != "good bot") return false;
		else if (funRandom.Next(100) == 0) message.CreateReactionAsync(DiscordEmoji.FromName(client, ":black_heart:"));
		else message.CreateReactionAsync(DiscordEmoji.FromName(client, hearts[funRandom.Next(hearts.Length)]));
		return true;
	}

	async Task<Task> ParseEcho(DiscordMessage message, List<string> args) {
		// Console.WriteLine("parse echo called");
		HashSet<DiscordUser> authors = new HashSet<DiscordUser>();
		string content = message.Content;
		foreach (DiscordMessage text in await message.Channel.GetMessagesAsync(3)) {
			// Console.WriteLine(text.Content);
			if (content != text.Content) break;
			authors.Add(text.Author);
		}
		// foreach (DiscordUser author in authors) {
		// 	Console.WriteLine(author);
		// }
		if (authors.Count >= 3) {
			// Console.WriteLine("send message reached");
			// Console.WriteLine(content);
			// await SendMessage(content, message.Channel);
			await message.Channel.SendMessageAsync(content);
			return Task.CompletedTask;
		}
		return null;
	}
	
	async Task ParseChat(DiscordMessage message, List<string> args, MessageCreateEventArgs context) {
		if (args[0] != "<551378788286464000>") await new Task(() => { return; } );
		
		string response;
		string authorName;
		
		if (context.Guild != null) {
			authorName = (await context.Guild.GetMemberAsync(message.Author.Id)).Nickname;
			// Console.WriteLine($"nickname {authorName}");
			if (authorName == "") authorName = (await context.Guild.GetMemberAsync(message.Author.Id)).DisplayName;
			// Console.WriteLine($"display name {authorName}");
			if (authorName == "") authorName = "[YOUR NAME HERE]";
		} else {
			authorName = context.Author.Username;
		}
		
		if (funRandom.Next(3) == 0) {
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "java";
			p.StartInfo.Arguments = "-cp \"C:/Users/LOWERCASE/Desktop/Server/dnd-bot/Eliza\" Main \"" + message.Content.Substring(22) + "\"";
			p.Start();
			response = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
		} else {
			response = ReplyGen.Gen(message.Content.Substring(22)).Replace("~", authorName);
		}
		
		DiscordMessageBuilder reply = new DiscordMessageBuilder() {
			// Content = output
			Content = response
			// Content = ReplyGen.Gen(message.Content.Substring(22)).Replace("~", $"{context.Guild.Members.ToString()}")
		}.WithReply(message.Id);
		authorName = message.Author.Username;
		Console.WriteLine($"{authorName} {response}");
		await message.RespondAsync(reply);
		await Task.CompletedTask;
	}
	
	
	// async Task SendMessage(string content, DiscordChannel channel) {
	// 	await new DiscordMessageBuilder().WithContent(echo).SendAsync(channel);
	// }
}
