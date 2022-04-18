using System;

class KarmicRandom : Random {

	int karma = 0;
	int threshold = 4;

	internal KarmicRandom(int threshold = 4) {
		this.threshold = threshold;
	}

	public override int Next(int maxValue) {
		if (maxValue != 20) return base.Next(maxValue);
		if (Math.Abs(karma) >= threshold) {
			if (karma > 0) {
				karma = -1;
				int temp = maxValue / 2 + base.Next(maxValue / 2);
				Console.WriteLine($"karmic proc: {temp}");
				return temp;
			} else if (karma < 0) {
				karma = 1;
				int temp = base.Next(maxValue / 2);
				Console.WriteLine($"karmic proc: {temp}");
				return temp;
			}
		}
		int value = base.Next(maxValue);
		if (value < maxValue / 2) {
			if (karma >= 0) karma++;
			else karma = 1;
		} else {
			if (karma <= 0) karma--;
			else karma = -1;
		}
		return value;
	}
}
