using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Loader {
	
	static JsonSerializerOptions options = new JsonSerializerOptions() {
		WriteIndented = true
	};
	
	public static void Main(string[] args) {
		string token = JsonSerializer.Deserialize<Data>(File.ReadAllText("Token")).Token;
		// Macros macros = new Macros(JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string[]>>>(File.ReadAllText("Macros.json"), options));
		new Bot(token);
	}
	
	internal static void SerializeMacros(Dictionary<string, Dictionary<string, string[]>> macros) {
		
		File.WriteAllText("Macros.json", JsonSerializer.Serialize(macros, options));
	}
}
