
public class Main {
	
	public static void main(String[] args) {
		Eliza eliza;
		eliza = new Eliza();
		for (int i = 0; i < args.length; ++i) {
			System.out.println(eliza.processInput(args[i]));
		}
		// System.out.println(eliza.processInput(args[1]));
	}
}