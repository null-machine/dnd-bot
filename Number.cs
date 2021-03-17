class Number {
	
	int value;
	bool bold;
	
	Number(int value, bool bold) {
		this.value = value;
		this.bold = bold;
	}
	
	Number(int value) : this(value, false) {}
	
	public override string ToString() {
		return (bold ? $"**{value}**" : value.ToString());
	}
	
}
