

import java.util.Vector;
// import java.util.Random;

/**
 *  Eliza reassembly list.
 */
public class ReasembList extends Vector {

    /**
     *  Add an element to the reassembly list.
     */
    public void add(String reasmb) {
        addElement(reasmb);
    }
	
	// public Object get(int index) {
	// 	Random random = new Random();
	// 	return super.get(random.nextInt(capacity()));
	// }

    /**
     *  Print the reassembly list.
     */
    public void print(int indent) {
        for (int i = 0; i < size(); i++) {
            for (int j = 0; j < indent; j++) System.out.print(" ");
            String s = (String)elementAt(i);
            System.out.println("reasemb: " + s);
        }
    }
}

