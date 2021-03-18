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
		new Bot(JsonSerializer.Deserialize<Data>(File.ReadAllText("Token")).Token);
	}
}
