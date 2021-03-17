struct BoldInt {
	
	internal int value;
	internal bool bold;
	
	// internal BoldInt(int value) : this(value, false) {}
	internal BoldInt(int value, bool bold = false) {
		this.value = value;
		this.bold = bold;
	}
	
	public override string ToString() => bold ? $"**{value}**" : value.ToString();
	public static BoldInt operator +(BoldInt a) => a;
	public static BoldInt operator -(BoldInt a) => new BoldInt(-a.value);
	public static BoldInt operator +(BoldInt a, BoldInt b) => new BoldInt(a.value + b.value);
	public static BoldInt operator -(BoldInt a, BoldInt b) => a + (-b);
	public static BoldInt operator *(BoldInt a, BoldInt b) => new BoldInt(a.value * b.value);
	public static BoldInt operator /(BoldInt a, BoldInt b) => new BoldInt(a.value / b.value);
}
