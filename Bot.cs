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
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;

class Bot {

	DiscordClient client;
	string[] hearts = new string[] { ":blue_heart:", ":yellow_heart:", ":heart:" };
	Random funRandom = new Random();
	Dictionary<string, KarmicRandom> userRandoms = new Dictionary<string, KarmicRandom>();
	DiscordChannel relay;
	DiscordChannel musicChannel;
	VoiceNextConnection connection;
	Queue<string> songs = new Queue<string>();
	Queue<string> downloads = new Queue<string>();

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
		DiscordMessage message = e.Message;
		string line = message.Content.Replace('+', ' ').ToLower();
		line = new string(line.Where(i => !char.IsPunctuation(i) || i == '-').ToArray());
		List<string> args = line.Split(' ').Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
		if (args.Count == 0) return null;
		if (ParsePing(message, args)) return null;
		if (ParseChain(message, args)) return null;
		if (ParsePlay(message, e)) return null;
		return null;
	}

	async Task Main() {
		await client.ConnectAsync();
		relay = await client.GetChannelAsync(1025223875304374274);
		// relay = await client.GetChannelAsync(881599328639135754);

		client.MessageCreated += MessageCreated;
		// client.MessageCreated += ParsePlay;
		client.Ready += Ready;
		client.UseVoiceNext();
		// client.onuserleft += check if any left, if not, leave

		new Thread(delegate() { Maestro(); }).Start();
		// new Thread(delegate() { DumpRelay(); }).Start();

		await Task.Delay(-1);
	}

	void DumpRelay() {
		while (true) {
			try {
				using (FileStream fileStream = File.Open("Relay.txt", FileMode.Open)) {
					StreamReader reader = new StreamReader(fileStream);
					string line;
					while ((line = reader.ReadLine()) != null) {
						relay.SendMessageAsync(line);
						Thread.Sleep(1000);
					}
					fileStream.SetLength(0);
				}
			} catch {
				Thread.Sleep(10000);
			}
			Thread.Sleep(2000);
		}
	}

	async Task Ready(DiscordClient client, ReadyEventArgs e) {
		await client.UpdateStatusAsync(new DiscordActivity("God", ActivityType.Playing), UserStatus.Online);
	}

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
					foundSpecial = true;
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
				userRandoms.Add(user, new KarmicRandom());
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
		if (input.Equals("x")) return 2;
		string[] splits = input.Split('x');
		if (splits.Length != 2 || !(splits[0].Equals("") ^ splits[1].Equals(""))) return -1;
		int parse;
		if (int.TryParse(splits[0], out parse)) return parse;
		if (int.TryParse(splits[1], out parse)) return parse;
		return -1;
	}

	bool ParsePing(DiscordMessage message, List<string> args) {
		if (!args[0].Equals("boop") && !args[0].Equals("ping")) return false;
		if (funRandom.Next(100) == 0) message.CreateReactionAsync(DiscordEmoji.FromName(client, ":black_heart:"));
		else message.CreateReactionAsync(DiscordEmoji.FromName(client, hearts[funRandom.Next(hearts.Length)]));
		return true;
	}

	bool ParsePlay(DiscordMessage message, MessageCreateEventArgs e) {
		if (message.Content.Length < 5 || message.Content.Substring(0, 5) != "play ") return false;
		musicChannel = message.Channel;
		VoiceNextExtension voiceNext = client.GetVoiceNext();
		DiscordVoiceState voiceState = ((DiscordMember)message.Author).VoiceState;
		VoiceNextConnection voiceConnection = voiceNext.GetConnection(e.Guild);
		if (voiceState == null) {
			DiscordMessageBuilder reply = new DiscordMessageBuilder() {
				Content = "Please join a voice channel."
			};
			reply.WithReply(message.Id);
			message.RespondAsync(reply);
			return true;
		} else if (voiceConnection == null) {
			// voiceConnection = await voiceNext.ConnectAsync(voiceState.Channel);
			_ = Connect(voiceNext, voiceState, voiceConnection);
		}

		// string filePath = message.Content.Substring(5);
		// if (!File.Exists($@"C:\Users\LOWERCASE\Desktop\Home\Music\{filePath}")) {
		// 	DiscordMessageBuilder reply = new DiscordMessageBuilder() {
		// 		Content = "File does not exist."
		// 	};
		// 	reply.WithReply(message.Id);
		// 	message.RespondAsync(reply);
		// 	return true;
		// } else {
		// 	_ = Play(voiceConnection, filePath);
		// }

		try {
			string url = message.Content.Substring(5);
			string videoId = HttpUtility.ParseQueryString(new Uri(url).Query)["v"];
			Console.WriteLine(videoId);
			string[] fileNames = Directory.GetFiles("./Downloads");
			// Console.WriteLine(fileNames.Length);
			string targetString = null;
			for (int i = 0; i < fileNames.Length; ++i) {
				if (fileNames[i].Contains(videoId)) {
					targetString = fileNames[i];
					break;
				}
			}
			if (string.IsNullOrEmpty(targetString))	{
				DiscordMessageBuilder reply = new DiscordMessageBuilder() {
					// Content = "The song will be queued when ready. View progress with `view queue` (WIP)."
					Content = "The song will be queued when ready."
				};
				reply.WithReply(message.Id);
				message.RespondAsync(reply);
				ProcessStartInfo command = new ProcessStartInfo {
					FileName = "./Downloads/yt-dlp.exe",
					Arguments = $@"-x --audio-format mp3 -P ./Downloads {url}"
				};
				Process download = Process.Start(command);
			} else {
				songs.Enqueue(targetString);
			}
		} catch (Exception exc) {
			Console.WriteLine(exc);
			DiscordMessageBuilder reply = new DiscordMessageBuilder() {
				Content = "LOWERCASE#0357 There was an error in music playback, please check the console."
			};
			reply.WithReply(message.Id);
			message.RespondAsync(reply);
			return false;
		}
		return true;
	}

	async Task Connect(VoiceNextExtension voice, DiscordVoiceState state, VoiceNextConnection connection) {
		connection = await voice.ConnectAsync(state.Channel);
	}

	async Task Play(VoiceNextConnection connection, string filePath) {
		while (connection.IsPlaying) await connection.WaitForPlaybackFinishAsync();
		try {
			await connection.SendSpeakingAsync(true);

			ProcessStartInfo psi = new ProcessStartInfo {
				FileName = "ffmpeg.exe",
				Arguments = $@"-i ""C:\Users\LOWERCASE\Desktop\Home\Music\{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1 -filter:a loudnorm -loglevel quiet",
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			Process ffmpeg = Process.Start(psi);
			var ffout = ffmpeg.StandardOutput.BaseStream;

			var txStream = connection.GetTransmitSink();
			await ffout.CopyToAsync(txStream);
			await txStream.FlushAsync();
			await connection.WaitForPlaybackFinishAsync();
		} catch (Exception exc) {
			Console.WriteLine(exc);
		}
	}

	void Maestro() {
		while (true) {
			string[] fileNames = Directory.GetFiles("/Downloads");
			Console.WriteLine(fileNames);
			Thread.Sleep(2000);
		}
	}
}
