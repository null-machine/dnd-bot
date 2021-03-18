using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

[Serializable()]
public class Macros {
	
	public Dictionary<ulong, Dictionary<string, string[]>> macros;
	public Dictionary<string, Dictionary<string, string[]>> testMacros;
	
	internal Macros() {
		macros = new Dictionary<ulong, Dictionary<string, string[]>>();
		testMacros = new Dictionary<string, Dictionary<string, string[]>>();
		
	}
	
	Dictionary<string, string[]> GetMacros(ulong id) {
		if (!macros.ContainsKey(id)) {
			macros[id] = new Dictionary<string, string[]>();
		}
		return macros[id];
	}
	
	Dictionary<string, string[]> GetTestMacros(ulong id) {
		if (!testMacros.ContainsKey(id.ToString())) {
			testMacros[id.ToString()] = new Dictionary<string, string[]>();
		}
		return testMacros[id.ToString()];
	}
	
	internal bool ParseRegister(DiscordMessage message, List<string> args) {
		if (!args[0].Equals("register") || args.Count < 2) return false;
		DiscordMessageBuilder reply;
		string content;
		Dictionary<string, string[]> userMacros = GetMacros(message.Author.Id);
		Dictionary<string, string[]> testUserMacros = GetTestMacros(message.Author.Id);
		if (args.Count == 2) {
			bool removed = userMacros.ContainsKey(args[1]);
			content = removed ? $":firecracker: `{args[1]}` → `{string.Join(' ', userMacros[args[1]])}` has been unregistered." : $":grey_question: `{args[1]}` was not found, so no changes have been made.";
			userMacros.Remove(args[1]);
		} else {
			if (userMacros.ContainsKey(args[1])) userMacros.Remove(args[1]);
			userMacros.Add(args[1], args.Skip(2).ToArray());
			testUserMacros.Add(args[1].ToString(), args.Skip(2).ToArray());
			content = $":writing_hand: `{args[1]}` → `{string.Join(' ', userMacros[args[1]])}` has been registered.";
		}
		reply = new DiscordMessageBuilder();
		reply.Content = content;
		reply.WithReply(message.Id);
		message.RespondAsync(reply);
		Loader.SerializeMacros(testMacros);
		return true;
	}
	
	internal void Replace(ulong user, List<string> args) {
		int count = args.Count;
		Dictionary<string, string[]> userMacros = GetMacros(user);
		for (int i = args.Count - 1; i >= 0; i--) {
			if (userMacros.ContainsKey(args[i])) {
				args.AddRange(macros[user][args[i]]);
				args.RemoveAt(i);
			}
		}
	}
}
