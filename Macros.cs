using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

class Macros {
	
	Dictionary<ulong, Dictionary<string, string[]>> macros;
	
	internal Macros() {
		macros = new Dictionary<ulong, Dictionary<string, string[]>>();
	}
	
	Dictionary<string, string[]> GetMacros(ulong id) {
		if (!macros.ContainsKey(id)) macros[id] = new Dictionary<string, string[]>();
		return macros[id];
	}
	
	internal bool ParseRegister(DiscordMessage message, List<string> args) {
		if (!args[0].Equals("register")) return false;
		Dictionary<string, string[]> userMacros = GetMacros(message.Author.Id);
		if (args.Count == 2) userMacros.Remove(args[1]);
		else if (userMacros.ContainsKey(args[1])) {
			userMacros.Remove(args[1]);
			userMacros.Add(args[1], args.Skip(2).ToArray());
		} else userMacros.Add(args[1], args.Skip(2).ToArray());
		
		return true;
		
	}
	
	internal void Replace(ulong user, List<string> args) {
		int count = args.Count;
		for (int i = args.Count - 1; i >= 0; i--) {
			if (GetMacros(user).ContainsKey(args[i])) {
				args.AddRange(macros[user][args[i]]);
				args.RemoveAt(i);
			}
		}
	}
}
