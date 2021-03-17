using System;
using System.Text;

class Dice {
	
	StringBuilder data = new StringBuilder();
	int count, size;
	Random random;
	
	internal Dice(int count, int size, Random random) {
		this.count = count;
		this.size = size;
		this.random = random;
		for (int i = 0; i < count; i++) {
			int roll = random.Next(size) + 1;
			dataBuilder.Append($"{roll}, ");
			result += roll;
		}
		dataBuilder.Remove(dataBuilder.Length - 2, dataBuilder.Length);
		dataBuilder.Append(") ");
		data = dataBuilder;
	}
	
	internal int Reroll() {
		data.Clear();
		data.Append($"{repeats}d{size} (");
		
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
	
	public override void ToString() {
		return data;
	}
}
