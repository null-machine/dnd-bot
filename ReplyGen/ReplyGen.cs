using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class ReplyGen {
	
	static ReplyGen self;
	
	Random random = new Random();
	List<(HashSet<string>, List<string>)> prompts = new List<(HashSet<string>, List<string>)>();
	
	ReplyGen(string fileName) {
		StreamReader reader = File.OpenText(fileName);
		string line = null;
		HashSet<string> keywords = new HashSet<string>();
		List<string> replies = new List<string>();
		bool parsingKeywords = true;
		while ((line = reader.ReadLine()) != null) {
			// Console.WriteLine($"currently parsing: {line}");
			if (!line.StartsWith('\t')) {
				if (!parsingKeywords) { // rising edge
					// Console.WriteLine($"rising edge");
					prompts.Add((keywords, replies));
					keywords = new HashSet<string>(); // can't clear, pointers
					replies = new List<string>();
					parsingKeywords = true;
				}
				// Console.WriteLine($"added keyword");
				keywords.Add(line);
			} else {
				// if (parsingKeywords) { // falling edge
				parsingKeywords = false;
				// Console.WriteLine($"added response");
				replies.Add(line);
			}
		}
		// Console.WriteLine($"{prompts.Count} {prompts[1].Item2.Count}");
		reader.Close();
	}
	
	internal static void Init(string fileName) {
		self = new ReplyGen(fileName);
	}
	
	internal static string Gen(string input) {
		// string[] tokens = Regex.Replace(input, @"[^\w\s]", "").ToLower().Split(' ');
		// this fails on keywords with spaces aaaaaargh
		
		for (int i = 0; i < self.prompts.Count - 1; ++i) {
			foreach (string keyword in self.prompts[i].Item1) {
				int index = (Regex.Replace(input, @"[^\w\s]", "").ToLower()).IndexOf(keyword);
				// Console.WriteLine($"{Regex.Replace(input, @"[^\w\s]", "").ToLower()} | {keyword} | {index}");
				// int index = input.ToLower().IndexOf(keyword);
				// Console.WriteLine($"{input.ToLower()} | {keyword} | {index}");
				if (index != -1) {
					// maybe check to see if there are spaces or eol to either end of the keyword?
					// if both are true, then commit match
					bool leftCleared = false;
					bool rightCleared = false;
					if (index - 1 >= 0) {
						if (input[index - 1] == ' ' || input[index - 1] == '?' || input[index - 1] == '!' || input[index - 1] == '.' || input[index - 1] == '\"' || input[index - 1] == '\'') {
							leftCleared = true;
						}
					} else {
						leftCleared = true;
					}
					if (index + keyword.Length < input.Length) {
						if (input[index + keyword.Length] == ' ' || input[index + keyword.Length] == '?' || input[index + keyword.Length] == '!' || input[index + keyword.Length] == '.' || input[index + keyword.Length] == '\"' || input[index + keyword.Length] == '\'') {
							rightCleared = true;
						}
					} else {
						rightCleared = true;
					}
					if (!leftCleared || !rightCleared) {
						continue;
					}
					string remainder = Regex.Replace(input.Substring(index + keyword.Length), @"[!|?|.]", "").Trim();
					remainder = Regex.Replace(remainder, @"\byourself\b", "###self");
					remainder = Regex.Replace(remainder, @"\byour\b", "###y");
					remainder = Regex.Replace(remainder, @"\byou\b", "###e");
					remainder = Regex.Replace(remainder, @"\bmyself\b", "yourself");
					remainder = Regex.Replace(remainder, @"\bmy\b", "your");
					remainder = Regex.Replace(remainder, @"\bme\b", "you");
					remainder = remainder.Replace("###self", "myself");
					remainder = remainder.Replace("###y", "my");
					remainder = remainder.Replace("###e", "I");
					string reply = self.prompts[i].Item2[self.random.Next(self.prompts[i].Item2.Count)];
					reply = reply.Replace("*", remainder);
					Console.WriteLine($"match found '{keyword}' '{input}' '{remainder}'");
					return reply.Trim();
				}
			}
		}
		return self.prompts[self.prompts.Count - 1].Item2[self.random.Next(self.prompts[self.prompts.Count - 1].Item2.Count)];
	}

}