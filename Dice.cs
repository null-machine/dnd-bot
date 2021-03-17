using System;
using System.Linq;
using System.Text;

class Dice {

	internal int count, size;
	StringBuilder data = new StringBuilder();
	BoldInt[] results;
	Random random;

	internal Dice(int count, int size, Random random) {
		this.count = count;
		this.size = size;
		this.random = random;
		results = new BoldInt[count];
	}

	internal BoldInt Reroll() {
		int result = 0;
		for (int i = 0; i < count; i++) {
			int roll = random.Next(size) + 1;
			result += roll;
			results[i] = new BoldInt(roll, roll == 1 || roll == size);
		}
		return new BoldInt(result, result == count || result == count * size);
	}

	public override string ToString() {
		if (count <= 10) return $"{count}d{size} ({string.Join(", ", results.Select(i => i.ToString()).ToArray())})";
		else return $"{count}d{size} ({string.Join(", ", results.Take(10).Select(i => i.ToString()).ToArray())} ... )";
	}
}
