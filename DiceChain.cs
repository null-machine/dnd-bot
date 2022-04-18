using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class DiceChain {

	internal List<Dice> dices;
	internal int mod = 0, min = 0, max = 0;
	internal float Average => min + (max - min) * 0.5f;

	internal DiceChain(List<Dice> dices, int mod = 0) {
		this.dices = dices;
		this.mod = mod;
		min = mod;
		max = mod;
		foreach (Dice dice in dices) {
			min += dice.count;
			max += dice.count * dice.size;
		}
	}

	internal BoldInt Roll(Random random) {
		int result = mod;
		foreach (Dice dice in dices) {
			result += dice.Roll(random).value;
		}
		return new BoldInt(result, result == min || result == max);
	}

	public override string ToString() {
		string data = $"**Roll:** {string.Join(" + ", dices.Select(i => i.ToString()).ToArray())}";
		if (mod < 0) return $"{data} - {-mod}";
		if (mod > 0) return $"{data} + {mod}";
		return data;
	}
}
