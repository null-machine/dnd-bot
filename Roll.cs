using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class Roll {
	
	List<Dice> dices;
	int mod = 0, min = 0, max = 0;
	
	internal Roll(List<Dice> dices, int mod = 0) {
		this.dices = dices;
		this.mod = mod;
		foreach (Dice dice in dices) { // TODO turn this into an iterator
			min += dice.count;
			max += dice.count * dice.size;
		}
	}
	
	internal BoldInt Reroll() {
		int result = 0;
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
