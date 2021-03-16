using System;

namespace DiscordBot {

	class Roll {

		internal int result;
		internal string input;
		internal int repeats;
		internal int size;
		internal bool Critical => size == 20 && repeats == 1 && result == 20;
		internal bool CritFail => size == 20 && repeats == 1 && result == 1;
		Random random;

		internal Roll(int repeats, int size) {
			this.repeats = repeats;
			this.size = size;
			random = new Random();
			Reroll();
		}


		internal void Reroll() {
			input = $"{repeats}d{size} (";
			result = 0;
			for (int i = 0; i < repeats; i++) {
				int roll = random.Next(1, size + 1);
				result += roll;
				if (roll == 1 || roll == size) input += $"**{roll}**";
				else input += roll;
				if (i != repeats - 1) input += ", ";
			}
			input += ")";
		}
	}
}
