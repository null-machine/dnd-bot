using System.Linq;
using System.Collections.Generic;

static class Extensions {
	
	internal static List<string> ParseArgs(this string line) {
		return line.Replace('+', ' ').Split(' ').Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
	}
	
}
