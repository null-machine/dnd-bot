using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

class Macros {
	
	Dictionary<ulong, Dictionary<string, string[]>> macros;
	
	internal bool ParseRegister(DiscordMessage message) {
		string content = message.Content.ToLower();
		if (!content.StartsWith("register ")) return false;
		List<string> args = content.ParseArgs();
		return true;
		
	}
	
	internal void Replace(ulong user, List<string> args) {
		int count = args.Count;
		for (int i = args.Count - 1; i >= 0; i--) {
			if (macros[user].ContainsKey(args[i])) {
				args.AddRange(macros[user][args[i]]);
				args.RemoveAt(i);
			}
		}
	}
}
