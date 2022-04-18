using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Loader {

	struct Token {
		public string Value { get; set; }
	}

	public static void Main(string[] args) {
		string token = JsonSerializer.Deserialize<Token>(File.ReadAllText("Token")).Value;
		new Bot(token);
	}
}
