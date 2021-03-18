using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Loader {
	
	public static void Main(string[] args) {
		// Data data = new Data() {
		// 	Token = ""
		// };
		// File.WriteAllText("Token", JsonSerializer.Serialize(data));
		new Bot(JsonSerializer.Deserialize<Data>(File.ReadAllText("Token")).Token, new Macros());
	}
	
	internal static void SerializeMacros(Dictionary<string, Dictionary<string, string[]>> macros) {
		File.WriteAllText("Macros.json", JsonSerializer.Serialize(macros));
	}
}
