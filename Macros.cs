using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

[Serializable()]
public class Macros {
	
	public Dictionary<string, Dictionary<string, string[]>> macros;
	
	internal Macros(Dictionary<string, Dictionary<string, string[]>> macros) {
		this.macros = macros;
	}
	
	Dictionary<string, string[]> GetMacros(ulong id) {
		if (!macros.ContainsKey(id.ToString())) {
			macros[id.ToString()] = new Dictionary<string, string[]>();
		}
		return macros[id.ToString()];
	}
	
	internal bool ParseSave(DiscordMessage message, List<string> args) {
		if (!args[0].Equals("save") || args.Count < 2) return false;
		Dictionary<string, string[]> macros = GetMacros(message.Author.Id);
		DiscordMessageBuilder reply;
		string content;
		if (args.Count == 2) {
			bool removed = macros.ContainsKey(args[1]);
			content = removed ? $":firecracker: `{args[1]}` → `{string.Join(' ', macros[args[1]])}` has been deleted." : $":grey_question: `{args[1]}` was not found, so no changes have been made.";
			macros.Remove(args[1]);
		} else {
			string[] oldMacro = null;
			if (macros.ContainsKey(args[1])) {
				oldMacro = macros[args[1]];
				macros.Remove(args[1]);
			}
			macros.Add(args[1], args.Skip(2).ToArray());
			if (oldMacro != null) {
				content = $":writing_hand: `{args[1]}` → `{string.Join(' ', macros[args[1]])}` has overwritten `{args[1]}` → `{string.Join(' ', oldMacro)}`.";
			} else content = $":writing_hand: `{args[1]}` → `{string.Join(' ', macros[args[1]])}` has been saved.";
		}
		reply = new DiscordMessageBuilder();
		reply.Content = content;
		reply.WithReply(message.Id);
		message.RespondAsync(reply);
		Loader.SerializeMacros(this.macros);
		return true;
	}
	
	internal bool ParseView(DiscordMessage message, List<string> args) {
		if (!(args[0].Equals("view") || args[0].Equals("v"))) return false;
		Dictionary<string, string[]> macros = GetMacros(message.Author.Id);
		StringBuilder content = new StringBuilder();
		if (args.Count == 2) {
			if (macros.ContainsKey(args[1])) content.Append($":mag: `{args[1]}` → `{string.Join(' ', macros[args[1]])}`");
			else content.Append($":mag: `{args[1]}` was not found.");
		} else if (args.Count == 1) {
			if (macros.Count == 0) content.Append($":mag: {message.Author.Username} has saved no rolls.");
			else {
				content.Append($":mag: {message.Author.Username} has saved {macros.Count} rolls:\n");
				foreach (KeyValuePair<string, string[]> macro in macros) {
					content.Append($"`{macro.Key}` → `{string.Join(' ', macro.Value)}`\n");
				}
			}
		} else return false;
		DiscordMessageBuilder reply = new DiscordMessageBuilder();
		reply.Content = content.ToString();
		reply.WithReply(message.Id);
		message.RespondAsync(reply);
		return true;
	}
	
	internal void Replace(ulong user, List<string> args) {
		int count = args.Count;
		Dictionary<string, string[]> macros = GetMacros(user);
		for (int i = args.Count - 1; i >= 0; i--) {
			if (macros.ContainsKey(args[i])) {
				args.AddRange(this.macros[user.ToString()][args[i]]);
				args.RemoveAt(i);
			}
		}
	}
}
