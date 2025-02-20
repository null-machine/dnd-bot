using System;
using System.Linq;
using System.Text;

class Dice {

	internal int count, size;
	internal float Average => count + count * size * 0.5f;
	StringBuilder data = new StringBuilder();
	BoldInt[] results;

	internal Dice(int count, int size) {
		this.count = count;
		this.size = size;
		results = new BoldInt[count];
	}

	internal BoldInt Roll(Random random) {
		int result = 0;
		for (int i = 0; i < count; i++) {
			int roll = random.Next(size) + 1;
			result += roll;
			// results[i] = new BoldInt(roll, roll == 1 || roll == size);
			results[i] = new BoldInt(roll, roll == size || roll == 5);
		}
		return new BoldInt(result, result == count || result == count * size);
	}

	public override string ToString() {
		if (count <= 10) return $"{count}d{size} ({string.Join(", ", results.Select(i => i.ToString()).ToArray())})";
		else return $"{count}d{size} ({string.Join(", ", results.Take(10).Select(i => i.ToString()).ToArray())} ... )";
	}
}
