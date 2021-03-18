using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class Roll {
	
	internal List<Dice> dices;
	internal int mod = 0, min = 0, max = 0;
	internal float Average => min + (max - min) * 0.5f;
	
	internal Roll(List<Dice> dices, int mod = 0) {
		this.dices = dices;
		this.mod = mod;
		foreach (Dice dice in dices) { // TODO turn this into an iterator
			min += dice.count + mod;
			max += dice.count * dice.size + mod;
		}
	}
	
	internal BoldInt Reroll() {
		int result = mod;
		foreach (Dice dice in dices) result += dice.Reroll().value;
		return new BoldInt(result, result == min || result == max);
	}
	
	public override string ToString() {
		string data = $"**Roll:** {string.Join(" + ", dices.Select(i => i.ToString()).ToArray())}";
		if (mod < 0) return $"{data} - {-mod}";
		if (mod > 0) return $"{data} + {mod}";
		return data;
	}
}
