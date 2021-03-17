using System.Collections.Generic;
using System;

class Initiator {
	
	public static void Main(string[] args) {
		Dice dice = new Dice(1, 2, new Random());
		Dice dice2 = new Dice(1, 3, new Random());
		List<Dice> list = new List<Dice>();
		list.Add(dice);
		list.Add(dice2);
		Roll roll = new Roll(list, 5);
		Console.WriteLine(roll.Reroll());
	}
}
