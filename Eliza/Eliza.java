/**
* Eliza
* Author: Charles Hayden
* http://www.chayden.net/eliza/Eliza.html
* Modified by Andres Colubri to use it as a Processing library
*/

import java.io.BufferedReader;
import java.io.IOException;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

/**
 *  Eliza main class.
 *  Stores the processed script.
 *  Does the input transformations.
 */
public class Eliza {
  public Eliza() {
		
		readDefaultScript();
  }

    public boolean finished() {
        return finished;
    }

    /**
     *  Process a line of script input.
     */
    public void collect(String s) {
        String lines[] = new String[4];

        if (EString.match(s, "*reasmb: *", lines)) {
            if (lastReasemb == null) {
                System.out.println("Error: no last reasemb");
                return;
            }
            lastReasemb.add(lines[1]);
        }
        else if (EString.match(s, "*decomp: *", lines)) {
            if (lastDecomp == null) {
                System.out.println("Error: no last decomp");
                return;
            }
            lastReasemb = new ReasembList();
            String temp = new String(lines[1]);
            if (EString.match(temp, "$ *", lines)) {
                lastDecomp.add(lines[0], true, lastReasemb);
            } else {
                lastDecomp.add(temp, false, lastReasemb);
            }
        }
        else if (EString.match(s, "*key: * #*", lines)) {
            lastDecomp = new DecompList();
            lastReasemb = null;
            int n = 0;
            if (lines[2].length() != 0) {
                try {
                    n = Integer.parseInt(lines[2]);
                } catch (NumberFormatException e) {
                    System.out.println("Number is wrong in key: " + lines[2]);
                }
            }
            keys.add(lines[1], n, lastDecomp);
        }
        else if (EString.match(s, "*key: *", lines)) {
            lastDecomp = new DecompList();
            lastReasemb = null;
            keys.add(lines[1], 0, lastDecomp);
        }
        else if (EString.match(s, "*synon: * *", lines)) {
            WordList words = new WordList();
            words.add(lines[1]);
            s = lines[2];
            while (EString.match(s, "* *", lines)) {
                words.add(lines[0]);
                s = lines[1];
            }
            words.add(s);
            syns.add(words);
        }
        else if (EString.match(s, "*pre: * *", lines)) {
            pre.add(lines[1], lines[2]);
        }
        else if (EString.match(s, "*post: * *", lines)) {
            post.add(lines[1], lines[2]);
        }
        else if (EString.match(s, "*initial: *", lines)) {
            initial = lines[1];
        }
        else if (EString.match(s, "*final: *", lines)) {
            finl = lines[1];
        }
        else if (EString.match(s, "*quit: *", lines)) {
            quit.add(" " + lines[1]+ " ");
        }
        else {
            System.out.println("Unrecognized input: " + s);
        }
    }

    /**
     *  Print the stored script.
     */
    public void print() {
        if (printKeys) keys.print(0);
        if (printSyns) syns.print(0);
        if (printPrePost) {
            pre.print(0);
            post.print(0);
        }
        if (printInitialFinal) {
            System.out.println("initial: " + initial);
            System.out.println("final:   " + finl);
            quit.print(0);
            quit.print(0);
        }
    }

    /**
     *  Process a line of input.
     */
    public String processInput(String s) {
        String reply;
        //  Do some input transformations first.
        s = EString.translate(s, "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                                 "abcdefghijklmnopqrstuvwxyz");
        s = EString.translate(s, "@#$%^&*()_-+=~`{[}]|:;<>\\\"",
                                 "                          "  );
        s = EString.translate(s, ",?!", "...");
        //  Compress out multiple speace.
        s = EString.compress(s);
        String lines[] = new String[2];
        //  Break apart sentences, and do each separately.
        while (EString.match(s, "*.*", lines)) {
            reply = sentence(lines[0]);
            if (reply != null) return reply;
            s = EString.trim(lines[1]);
        }
        if (s.length() != 0) {
            reply = sentence(s);
            if (reply != null) return reply;
        }
        //  Nothing matched, so try memory.
        String m = mem.get();
        if (m != null) return m;

        //  No memory, reply with xnone.
        Key key = keys.getKey("xnone");
        if (key != null) {
            Key dummy = null;
            reply = decompose(key, s, dummy);
            if (reply != null) return reply;
        }
        //  No xnone, just say anything.
        return "I am at a loss for words.";
    }

	public boolean readDefaultScript()	{
		clearScript();
		return readScript("eliza.script");
	}	
    
	public boolean readScript(String script) {
		clearScript();
		

    String[] lines = {"initial: How do you do.  Please tell me your problem.", "final: Goodbye.  Thank you for talking to me.", "quit: bye", "quit: goodbye", "quit: quit", "pre: dont don't", "pre: cant can't", "pre: wont won't", "pre: recollect remember", "pre: dreamt dreamed", "pre: dreams dream", "pre: maybe perhaps", "pre: how what", "pre: when what", "pre: certainly yes", "pre: machine computer", "pre: computers computer", "pre: were was", "pre: you're you are", "pre: i'm i am", "pre: same alike", "post: am are", "post: your my", "post: me you", "post: myself yourself", "post: yourself myself", "post: i you", "post: you I", "post: my your", "post: i'm you are", "synon: belief feel think believe wish", "synon: family mother mom father dad sister brother wife children child", "synon: desire want need", "synon: sad unhappy depressed sick", "synon: happy elated glad better", "synon: cannot can't", "synon: everyone everybody nobody noone", "synon: be am is are was", "key: xnone", "decomp: *", "reasmb: I'm not sure I understand you fully.", "reasmb: Please go on.", "reasmb: What does that suggest to you ?", "reasmb: Do you feel strongly about discussing such things ?", "key: sorry", "decomp: *", "reasmb: Please don't apologise.", "reasmb: Apologies are not necessary.", "reasmb: I've told you that apologies are not required.", "key: apologise", "decomp: *", "reasmb: goto sorry", "key: remember 5", "decomp: * i remember *", "reasmb: Do you often think of (2) ?", "reasmb: Does thinking of (2) bring anything else to mind ?", "reasmb: What else do you recollect ?", "reasmb: Why do you recollect (2) just now ?", "reasmb: What in the present situation reminds you of (2) ?", "reasmb: What is the connection between me and (2) ?", "decomp: * do you remember *", "reasmb: Did you think I would forget (2) ?", "reasmb: Why do you think I should recall (2) now ?", "reasmb: What about (2) ?", "reasmb: goto what", "reasmb: You mentioned (2) ?", "key: if 3", "decomp: * if *", "reasmb: Do you think its likely that (2) ?", "reasmb: Do you wish that (2) ?", "reasmb: What do you know about (2) ?", "reasmb: Really, if (2) ?", "key: dreamed 4", "decomp: * i dreamed *", "reasmb: Really, (2) ?", "reasmb: Have you ever fantasized (2) while you were awake ?", "reasmb: Have you ever dreamed (2) before ?", "reasmb: goto dream", "key: dream 3", "decomp: *", "reasmb: What does that dream suggest to you ?", "reasmb: Do you dream often ?", "reasmb: What persons appear in your dreams ?", "reasmb: Do you believe that dreams have something to do with your problems ?", "key: perhaps", "decomp: *", "reasmb: You don't seem quite certain.", "reasmb: Why the uncertain tone ?", "reasmb: Can't you be more positive ?", "reasmb: You aren't sure ?", "reasmb: Don't you know ?", "key: name 15", "decomp: *", "reasmb: I am not interested in names.", "reasmb: I've told you before, I don't care about names -- please continue.", "key: deutsch", "decomp: *", "reasmb: goto xforeign", "reasmb: I told you before, I don't understand German.", "key: francais", "decomp: *", "reasmb: goto xforeign", "reasmb: I told you before, I don't understand French.", "key: italiano", "decomp: *", "reasmb: goto xforeign", "reasmb: I told you before, I don't understand Italian.", "key: espanol", "decomp: *", "reasmb: goto xforeign", "reasmb: I told you before, I don't understand Spanish.", "key: xforeign", "decomp: *", "reasmb: I speak only English.", "key: hello", "decomp: *", "reasmb: How do you do.  Please state your problem.", "reasmb: Hi.  What seems to be your problem ?", "key: computer 50", "decomp: *", "reasmb: Do computers worry you ?", "reasmb: Why do you mention computers ?", "reasmb: What do you think machines have to do with your problem ?", "reasmb: Don't you think computers can help people ?", "reasmb: What about machines worrys you ?", "reasmb: What do you think about machines ?", "key: am", "decomp: * am i *", "reasmb: Do you believe you are (2) ?", "reasmb: Would you want to be (2) ?", "reasmb: Do you wish I would tell you you are (2) ?", "reasmb: What would it mean if you were (2) ?", "reasmb: goto what", "decomp: *", "reasmb: Why do you say 'am' ?", "reasmb: I don't understand that.", "key: are", "decomp: * are you *", "reasmb: Why are you interested in whether I am (2) or not ?", "reasmb: Would you prefer if I weren't (2) ?", "reasmb: Perhaps I am (2) in your fantasies.", "reasmb: Do you sometimes think I am (2) ?", "reasmb: goto what", "decomp: * are *", "reasmb: Did you think they might not be (2) ?", "reasmb: Would you like it if they were not (2) ?", "reasmb: What if they were not (2) ?", "reasmb: Possibly they are (2).", "key: your", "decomp: * your *", "reasmb: Why are you concerned over my (2) ?", "reasmb: What about your own (2) ?", "reasmb: Are you worried about someone else's (2) ?", "reasmb: Really, my (2) ?", "key: was 2", "decomp: * was i *", "reasmb: What if you were (2) ?", "reasmb: Do you think you were (2) ?", "reasmb: Were you (2) ?", "reasmb: What would it mean if you were (2) ?", "reasmb: What does (2) suggest to you ?", "reasmb: goto what", "decomp: * i was *", "reasmb: Were you really ?", "reasmb: Why do you tell me you were (2) now ?", "reasmb: Perhaps I already know you were (2).", "decomp: * was you *", "reasmb: Would you like to believe I was (2) ?", "reasmb: What suggests that I was (2) ?", "reasmb: What do you think ?", "reasmb: Perhaps I was (2).", "reasmb: What if I had been (2) ?", "key: i", "decomp: * i @desire *", "reasmb: What would it mean to you if you got (3) ?", "reasmb: Why do you want (3) ?", "reasmb: Suppose you got (3) soon ?", "reasmb: What if you never got (3) ?", "reasmb: What would getting (3) mean to you ?", "reasmb: What does wanting (3) have to do with this discussion ?", "decomp: * i am* @sad *", "reasmb: I am sorry to hear that you are (3).", "reasmb: Do you think that coming here will help you not to be (3) ?", "reasmb: I'm sure it's not pleasant to be (3).", "reasmb: Can you explain what made you (3) ?", "decomp: * i am* @happy *", "reasmb: How have I helped you to be (3) ?", "reasmb: Has your treatment made you (3) ?", "reasmb: What makes you (3) just now ?", "reasmb: Can you explan why you are suddenly (3) ?", "decomp: * i was *", "reasmb: goto was", "decomp: * i @belief * i *", "reasmb: Do you really think so ?", "reasmb: But you are not sure you (3).", "reasmb: Do you really doubt you (3) ?", "decomp: * i* @belief *you *", "reasmb: goto you", "decomp: * i am *", "reasmb: Is it because you are (2) that you came to me ?", "reasmb: How long have you been (2) ?", "reasmb: Do you believe it is normal to be (2) ?", "reasmb: Do you enjoy being (2) ?", "decomp: * i @cannot *", "reasmb: How do you think that you can't (3) ?", "reasmb: Have you tried ?", "reasmb: Perhaps you could (3) now.", "reasmb: Do you really want to be able to (3) ?", "decomp: * i don't *", "reasmb: Don't you really (2) ?", "reasmb: Why don't you (2) ?", "reasmb: Do you wish to be able to (2) ?", "reasmb: Does that trouble you ?", "decomp: * do i feel *", "reasmb: Tell me more about such feelings.", "reasmb: Do you often feel (2) ?", "reasmb: Do you enjoy feeling (2) ?", "reasmb: Of what does feeling (2) remind you ?", "decomp: * i * you *", "reasmb: Perhaps in your fantasies we (2) each other.", "reasmb: Do you wish to (2) me ?", "reasmb: You seem to need to (2) me.", "reasmb: Do you (2) anyone else ?", "decomp: *", "reasmb: You say (1) ?", "reasmb: Can you elaborate on that ?", "reasmb: Do you say (1) for some special reason ?", "reasmb: That's quite interesting.", "key: you", "decomp: * you remind me of *", "reasmb: goto alike", "decomp: * you are *", "reasmb: What makes you think I am (2) ?", "reasmb: Does it please you to believe I am (2) ?", "reasmb: Do you sometimes wish you were (2) ?", "reasmb: Perhaps you would like to be (2).", "decomp: * you* me *", "reasmb: Why do you think I (2) you ?", "reasmb: You like to think I (2) you -- don't you ?", "reasmb: What makes you think I (2) you ?", "reasmb: Really, I (2) you ?", "reasmb: Do you wish to believe I (2) you ?", "reasmb: Suppose I did (2) you -- what would that mean ?", "reasmb: Does someone else believe I (2) you ?", "decomp: * you *", "reasmb: We were discussing you -- not me.", "reasmb: Oh, I (2) ?", "reasmb: You're not really talking about me -- are you ?", "reasmb: What are your feelings now ?", "key: yes", "decomp: *", "reasmb: You seem to be quite positive.", "reasmb: You are sure.", "reasmb: I see.", "reasmb: I understand.", "key: no", "decomp: *", "reasmb: Are you saying no just to be negative?", "reasmb: You are being a bit negative.", "reasmb: Why not ?", "reasmb: Why 'no' ?", "key: my 2", "decomp: $ * my *", "reasmb: Lets discuss further why your (2).", "reasmb: Earlier you said your (2).", "reasmb: But your (2).", "reasmb: Does that have anything to do with the fact that your (2) ?", "decomp: * my* @family *", "reasmb: Tell me more about your family.", "reasmb: Who else in your family (4) ?", "reasmb: Your (3) ?", "reasmb: What else comes to mind when you think of your (3) ?", "decomp: * my *", "reasmb: Your (2) ?", "reasmb: Why do you say your (2) ?", "reasmb: Does that suggest anything else which belongs to you ?", "reasmb: Is it important that your (2) ?", "key: can", "decomp: * can you *", "reasmb: You believe I can (2) don't you ?", "reasmb: goto what", "reasmb: You want me to be able to (2).", "reasmb: Perhaps you would like to be able to (2) yourself.", "decomp: * can i *", "reasmb: Whether or not you can (2) depends on you more than me.", "reasmb: Do you want to be able to (2) ?", "reasmb: Perhaps you don't want to (2).", "reasmb: goto what", "key: what", "decomp: *", "reasmb: Why do you ask ?", "reasmb: Does that question interest you ?", "reasmb: What is it you really wanted to know ?", "reasmb: Are such questions much on your mind ?", "reasmb: What answer would please you most ?", "reasmb: What do you think ?", "reasmb: What comes to mind when you ask that ?", "reasmb: Have you asked such questions before ?", "reasmb: Have you asked anyone else ?", "key: because", "decomp: *", "reasmb: Is that the real reason ?", "reasmb: Don't any other reasons come to mind ?", "reasmb: Does that reason seem to explain anything else ?", "reasmb: What other reasons might there be ?", "key: why", "decomp: * why don't you *", "reasmb: Do you believe I don't (2) ?", "reasmb: Perhaps I will (2) in good time.", "reasmb: Should you (2) yourself ?", "reasmb: You want me to (2) ?", "reasmb: goto what", "decomp: * why can't i *", "reasmb: Do you think you should be able to (2) ?", "reasmb: Do you want to be able to (2) ?", "reasmb: Do you believe this will help you to (2) ?", "reasmb: Have you any idea why you can't (2) ?", "reasmb: goto what", "decomp: *", "reasmb: goto what", "key: everyone 2", "decomp: * @everyone *", "reasmb: Realy, (2) ?", "reasmb: Surely not (2).", "reasmb: Can you think of anyone in particular ?", "reasmb: Who, for example?", "reasmb: Are you thinking of a very special person ?", "reasmb: Who, may I ask ?", "reasmb: Someone special perhaps ?", "reasmb: You have a particular person in mind, don't you ?", "reasmb: Who do you think you're talking about ?", "key: everybody 2", "decomp: *", "reasmb: goto everyone", "key: nobody 2", "decomp: *", "reasmb: goto everyone", "key: noone 2", "decomp: *", "reasmb: goto everyone", "key: always 1", "decomp: *", "reasmb: Can you think of a specific example ?", "reasmb: When ?", "reasmb: What incident are you thinking of ?", "reasmb: Really, always ?", "key: alike 10", "decomp: *", "reasmb: In what way ?", "reasmb: What resemblence do you see ?", "reasmb: What does that similarity suggest to you ?", "reasmb: What other connections do you see ?", "reasmb: What do you suppose that resemblence means ?", "reasmb: What is the connection, do you suppose ?", "reasmb: Could here really be some connection ?", "reasmb: How ?", "key: like 10", "decomp: * @be *like *", "reasmb: goto alike"};
    if (lines == null || lines.length == 0) {
      System.err.println("Cannot load Eliza script!");
      return false;
    } else {
      for (int i = 0; i < lines.length; i++) {
        collect(lines[i]);
      }
      return true;
    }
  }	
	
	void clearScript() {
		keys.clear();
		syns.clear();
		pre.clear();
		post.clear();
		initial = "";
		finl = "";
		quit.clear();
		keyStack.reset();
	}
	
    /**
     *  Process a sentence.
     *  (1) Make pre transformations.
     *  (2) Check for quit word.
     *  (3) Scan sentence for keys, build key stack.
     *  (4) Try decompositions for each key.
     */
    String sentence(String s) {
        s = pre.translate(s);
        s = EString.pad(s);
        if (quit.find(s)) {
            finished = true;
            return finl;
        }
        keys.buildKeyStack(keyStack, s);
        for (int i = 0; i < keyStack.keyTop(); i++) {
            Key gotoKey = new Key();
            String reply = decompose(keyStack.key(i), s, gotoKey);
            if (reply != null) return reply;
            //  If decomposition returned gotoKey, try it
            while (gotoKey.key() != null) {
                reply = decompose(gotoKey, s, gotoKey);
                if (reply != null) return reply;
            }
        }
        return null;
    }

    /**
     *  Decompose a string according to the given key.
     *  Try each decomposition rule in order.
     *  If it matches, assemble a reply and return it.
     *  If assembly fails, try another decomposition rule.
     *  If assembly is a goto rule, return null and give the key.
     *  If assembly succeeds, return the reply;
     */
    String decompose(Key key, String s, Key gotoKey) {
        String reply[] = new String[10];
        for (int i = 0; i < key.decomp().size(); i++) {
            Decomp d = (Decomp)key.decomp().elementAt(i);
            String pat = d.pattern();
            if (syns.matchDecomp(s, pat, reply)) {
                String rep = assemble(d, reply, gotoKey);
                if (rep != null) return rep;
                if (gotoKey.key() != null) return null;
            }
        }
        return null;
    }

    /**
     *  Assembly a reply from a decomp rule and the input.
     *  If the reassembly rule is goto, return null and give
     *    the gotoKey to use.
     *  Otherwise return the response.
     */
    String assemble(Decomp d, String reply[], Key gotoKey) {
        String lines[] = new String[3];
        d.stepRule();
        String rule = d.nextRule();
        if (EString.match(rule, "goto *", lines)) {
            //  goto rule -- set gotoKey and return false.
            gotoKey.copy(keys.getKey(lines[0]));
            if (gotoKey.key() != null) return null;
            System.out.println("Goto rule did not match key: " + lines[0]);
            return null;
        }
        String work = "";
        while (EString.match(rule, "* (#)*", lines)) {
            //  reassembly rule with number substitution
            rule = lines[2];        // there might be more
            int n = 0;
            try {
                n = Integer.parseInt(lines[1]) - 1;
            } catch (NumberFormatException e) {
                System.out.println("Number is wrong in reassembly rule " + lines[1]);
            }
            if (n < 0 || n >= reply.length) {
                System.out.println("Substitution number is bad " + lines[1]);
                return null;
            }
            reply[n] = post.translate(reply[n]);
            work += lines[0] + " " + reply[n];
        }
        work += rule;
        if (d.mem()) {
            mem.save(work);
            return null;
        }
        return work;
    }
    
    final boolean echoInput = false;
    final boolean printData = false;

    final boolean printKeys = false;
    final boolean printSyns = false;
    final boolean printPrePost = false;
    final boolean printInitialFinal = false;

    /** The key list */
    KeyList keys = new KeyList();
    /** The syn list */
    SynList syns = new SynList();
    /** The pre list */
    PrePostList pre = new PrePostList();
    /** The post list */
    PrePostList post = new PrePostList();
    /** Initial string */
    String initial = "Hello.";
    /** Final string */
    String finl = "Goodbye.";
    /** Quit list */
    WordList quit = new WordList();

    /** Key stack */
    KeyStack keyStack = new KeyStack();

    /** Memory */
    Mem mem = new Mem();

    DecompList lastDecomp;
    ReasembList lastReasemb;
    boolean finished = false;

    static final int success = 0;
    static final int failure = 1;
    static final int gotoRule = 2;
}
