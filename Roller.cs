using System;

class Roller : Random {
	
	internal double bias = 0.0;
	
	public override int Next(int maxValue) {
		double sample = Sample() - Double.Epsilon;
		if (bias > 0.0) sample = 1.0 - Math.Pow(sample, 1.0 + bias);
		else if (bias < 0.0) sample = Math.Pow(sample, 1.0 + Math.Abs(bias));
		return (int)(maxValue * sample);
	}
	
}
