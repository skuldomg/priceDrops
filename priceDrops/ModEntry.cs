using System;
using System.Collections.Generic;
using System.Linq;
using MailFrameworkMod;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;

namespace priceDrops
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {
        // Heart levels and corresponding discounts
        private static int HEART_LEVEL_1 = -1, HEART_LEVEL_2 = -1, HEART_LEVEL_3 = -1;
        private static int DISC_1 = -1, DISC_2 = -1, DISC_3 = -1, BONUS_DISC = -1;
        private List<string> CUSTOM_NPCS = new List<string>();

        // For mails
        private static Letter robin1Letter, robin2Letter, robin3Letter, robinMaruLetter, robinSebastianLetter;        
        private static Letter marnie1Letter, marnie2Letter, marnie3Letter, marnieShaneLetter;        
        private static Letter pierre1Letter, pierre2Letter, pierre3Letter, pierreAbigailLetter, pierreCarolineLetter;        
        private static Letter harvey1Letter, harvey2Letter, harvey3Letter, harveyMarriedLetter;        
        private static Letter gus1Letter, gus2Letter, gus3Letter;        
        private static Letter clint1Letter, clint2Letter, clint3Letter;        
        private static Letter sandy1Letter, sandy2Letter, sandy3Letter;        
        private static Letter willy1Letter, willy2Letter, willy3Letter;        
        private static Letter dwarf1Letter, dwarf2Letter, dwarf3Letter;        
        private static Letter krobus1Letter, krobus2Letter, krobus3Letter;        
        private static Letter wizard1Letter, wizard2Letter, wizard3Letter;        
        
        private List<Letter> myLetters = new List<Letter>();

        private LetterStatus model;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Read config
            ModConfig config = helper.ReadConfig<ModConfig>();
            HEART_LEVEL_1 = config.heartLevel1;
            HEART_LEVEL_2 = config.heartLevel2;
            HEART_LEVEL_3 = config.heartLevel3;
            DISC_1 = config.disc1;
            DISC_2 = config.disc2;
            DISC_3 = config.disc3;
            BONUS_DISC = config.bonusDisc;
            CUSTOM_NPCS = config.customNPCs;

            model = this.Helper.ReadJsonFile<LetterStatus>($"data/{Constants.SaveFolderName}.json") ?? new LetterStatus();

            // Security checks
            // Make sure that hearts are between 0 and 10, if not, assume standard values
            if (HEART_LEVEL_1 < 0 || HEART_LEVEL_1 > 10)
                HEART_LEVEL_1 = 3;
            if (HEART_LEVEL_2 < 0 || HEART_LEVEL_2 > 10)
                HEART_LEVEL_2 = 7;
            if (HEART_LEVEL_3 < 0 || HEART_LEVEL_3 > 10)
                HEART_LEVEL_3 = 10;

            // If relations between hearts are funky
            if(HEART_LEVEL_2 < HEART_LEVEL_1 || HEART_LEVEL_3 < HEART_LEVEL_2 || HEART_LEVEL_3 < HEART_LEVEL_1)
            {
                HEART_LEVEL_1 = 3;
                HEART_LEVEL_2 = 7;
                HEART_LEVEL_3 = 10;
            }

            // if discounts exceed 99% (100% crashes calculations for Krobus)
            if (DISC_1 > 99)
                DISC_1 = 10;
            if (DISC_2 > 99)
                DISC_2 = 25;
            if (DISC_3 > 99)
                DISC_3 = 50;
            if (BONUS_DISC > 99)
                BONUS_DISC = 5;

            // Initialize letters
            // Add to list for easy removal later
            // TODO: Make pretty
            robin1Letter = new Letter("robin1", "Hey there @,^you've been such a good customer since you've arrived here in town. So I've decided to give you a " + DISC_1 + "% discount!^^See you soon,^   -Robin, your favorite carpenter", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Robin") >= HEART_LEVEL_1);            
            myLetters.Add(robin1Letter);
            robin2Letter = new Letter("robin2", "Hi @,^since you've been such a good friend (and customer!) to me, I've decided to give you a " + DISC_2 + "% discount!^^See you soon,^   -Robin, your favorite carpenter (and friend!)", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Robin") >= HEART_LEVEL_2);
            myLetters.Add(robin2Letter);
            robin3Letter = new Letter("robin3", "Hi! I'm really happy to be your friend, @!^From now on, whenever you go shopping at my place, I'll give you " + DISC_3 + "% off!^^I hope you swing by soon! I'll get all your stuff built in no time!^   -Robin, your friend and favorite carpenter", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Robin") >= HEART_LEVEL_3);
            myLetters.Add(robin3Letter);
            robinMaruLetter = new Letter("robinMaru", "Hey @,^since we're family now, I thought I should give you another discount of "+BONUS_DISC+ "% at my store. Take good care of my kid!^^Love,^   -Robin", (letter) => Game1.getCharacterFromName("Maru", true).isMarried());
            myLetters.Add(robinMaruLetter);
            robinSebastianLetter = new Letter("robinSebastian", "Hey @,^since we're family now, I thought I should give you another discount of " + BONUS_DISC + "% at my store. Take good care of my kid!^^Love,^   -Robin", (letter) => Game1.getCharacterFromName("Sebastian", true).isMarried());
            myLetters.Add(robinSebastianLetter); 

            marnie1Letter = new Letter("marnie1", "Hey there @,^thank you for being such a good customer. Since you're going to be a regular, I've decided to give you " + DISC_1 + "% off for all your next purchases!^^Have a nice day,^   -Marnie", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Marnie") >= HEART_LEVEL_1);
            myLetters.Add(marnie1Letter);
            marnie2Letter = new Letter("marnie2", "Hello @, my Aunt made me write this because she's busy. Anyway, she wants you to know that you're really nice or something and that she's giving you another discount. Not even sure if we can afford it. Come buy some hay or something. It's "+DISC_2 +"% off.^   -Shane", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Marnie") >= HEART_LEVEL_2);
            myLetters.Add(marnie2Letter);
            marnie3Letter = new Letter("marnie3", "Hey dear,^you've been a really good friend and I'm really glad we're neighbours! Let me tell you something. From now on, you get "+DISC_3+ "% off of all the things you buy from me!^^Talk to you soon,^   -Marnie", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Marnie") >= HEART_LEVEL_3);
            myLetters.Add(marnie3Letter);
            marnieShaneLetter = new Letter("marnieShane", "Hey @,^thought I should write you a letter. I want you to get a discount on my chickens. I talked to my Aunt and discussed it with her and she said she's fine with it. So we'll give you "+BONUS_DISC+ "% on the chickens.^^Have a nice day,^   -Shane", (letter) => Game1.player.eventsSeen.Contains(3900074));
            myLetters.Add(marnieShaneLetter);

            pierre1Letter = new Letter("pierre1", "Dear valued customer,^Thanks for choosing 'Pierre's', your local produce shoppe! Your loyalty is very appreciated. We are happy to announce that from now on, you will receive a rebate of "+DISC_1+ "% whenever you visit our store. See you soon!^   -Pierre^^P.S. Sorry for the stock message, @. Enjoy!", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Pierre") >= HEART_LEVEL_1);
            myLetters.Add(pierre1Letter);
            pierre2Letter = new Letter("pierre2", "Dear valued customer,^Thanks for choosing 'Pierre's', your local produce shoppe! Your loyalty is very appreciated. We are happy to announce that from now on, you will receive a rebate of "+DISC_2+ "% whenever you visit our store. See you soon!^   -Pierre^^P.S. Sorry for the stock message, @. I'm really busy these days. Enjoy!", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Pierre") >= HEART_LEVEL_2);
            myLetters.Add(pierre2Letter);
            pierre3Letter = new Letter("pierre3", "Dear valued customer,^Thanks for choosing 'Pierre's', your local produce shoppe! Your loyalty is very appreciated. We are happy to announce that from now on, you will receive a rebate of "+DISC_3+ "% whenever you visit our store. See you soon!^   -Pierre^^P.S. Sorry for the stock message, @. How are you, by the way? I'm doing quite well myself, haha! Enjoy!", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Pierre") >= HEART_LEVEL_3);
            myLetters.Add(pierre3Letter);
            pierreAbigailLetter = new Letter("pierreAbigail", "Hey, @!^Since we're kind of family now, Caroline made me give you a "+BONUS_DISC+ "% discount on the store.  Anyway, I know Abby is a little wild, but she has a good heart. Treat her well.^^Take care,^   -Pierre", (letter) => Game1.getCharacterFromName("Abigail", true).isMarried());
            myLetters.Add(pierreAbigailLetter);
            pierreCarolineLetter = new Letter("pierreCaroline", "Hey, @, I didn't know you were best friends with my wife. She told me to give you a "+BONUS_DISC+ "% discount in our shop yesterday. You better stop by and shop often from now on!^^See you soon,^   -Pierre", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Caroline") == 10);
            myLetters.Add(pierreCarolineLetter);

            harvey1Letter = new Letter("harvey1", "Hello, @!^You've been a valued customer at my clinic. So I've decided to give you a "+DISC_1+ "% discount for my OTC medicine. Have a nice day!^   -Harvey^^PS: Don't forget to cover your mouth when you sneeze! Wash your hands often!", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Harvey") >= HEART_LEVEL_1);
            myLetters.Add(harvey1Letter);
            harvey2Letter = new Letter("harvey2", "Hello, @!^I'm so glad to have a friend in town. If you need tonics or remedies, just come by the clinic. I'll give you a "+DISC_2+ "% discount. See you soon!^   -Harvey^^PS: Don't forget to cover your mouth when you sneeze! Wash your hands often! If you need anything, just visit the clinic.", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Harvey") >= HEART_LEVEL_2);
            myLetters.Add(harvey2Letter);
            harvey3Letter = new Letter("harvey3", "Hello, @!^You've been such a good friend to me since you've arrived in town. If you need anything, just visit the clinic. I can give you a check-up before you go into the mines if you want. Oh and you get a "+DISC_3+ "% discount for my OTC medicines.^^Take care.^   -Harvey^^PS: Don't forget to cover your mouth when you sneeze! Wash your hands often! Don't forget to eat well! Oh, and don't overwork yourself. ", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Harvey") >= HEART_LEVEL_3);
            myLetters.Add(harvey3Letter);
            harveyMarriedLetter = new Letter("harveyMarried", "Hey darling, I'm writing from my office because I forgot to tell you yesterday. Since we're married now, I'd like you to have a "+BONUS_DISC+ "% discount on my over-the-counter medicines. It's not much, but I hope it helps you when you run off to the mines again.^^Take care,^   -your husband", (letter) => Game1.getCharacterFromName("Harvey", true).isMarried());
            myLetters.Add(harveyMarriedLetter);

            gus1Letter = new Letter("gus1", "Hello, @!^Since you've become a regular at my place, I'll give you "+DISC_1+ "% off! Come by the Stardrop Saloon when you need any refreshments. I make different dishes every week!^   -Gus", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Gus") >= HEART_LEVEL_1);
            myLetters.Add(gus1Letter);
            gus2Letter = new Letter("gus2", "Hey there, @,^thanks for being a loyal friend and customer. I really appreciate you coming by so often. Listening to your stories is never dull, haha! Anyway, I want to give you "+DISC_2+ "% off when you buy refreshments at the bar.^^See you soon!^   -Gus", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Gus") >= HEART_LEVEL_2);
            myLetters.Add(gus2Letter);
            gus3Letter = new Letter("gus3", "Hey, @!^I'm really glad you moved into town. Want to hear great news? I went through the numbers and am now able to give you your deserved "+DISC_3+ "% rebate for everything you purchase at my Saloon! I hope you come by more often from now on.^   -Your friend Gus", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Gus") >= HEART_LEVEL_3);
            myLetters.Add(gus3Letter);

            clint1Letter = new Letter("clint1", "Hello, @^thanks for coming by my shop so often. You know what, I'll give you "+DISC_1+ "% off from now on. If you need your tools upgraded, I'm your man. You know, being a blacksmith and all. Anyway. Bye.^   -Clint", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Clint") >= HEART_LEVEL_1);
            myLetters.Add(clint1Letter);
            clint2Letter = new Letter("clint2", "Hey, @^thanks for helping me with Emily the other day. Even though it didn't help. Anyway, I wanted to tell you that you'll get a "+DISC_2+ "% discount on your upgrades and my shop. Take care.^   -Clint", (letter) => ((Game1.player.getFriendshipHeartLevelForNPC("Clint") >= HEART_LEVEL_2) && (Game1.player.eventsSeen.Contains(97))));
            myLetters.Add(clint2Letter);
            clint3Letter = new Letter("clint3", "Hey, @!^You're really hitting the mines! Thanks to you, my business is going quite well. So well actually, that I can give you "+DISC_3+ "% off from now on! Don't tell anyone. Take care.^   -Clint", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Clint") >= HEART_LEVEL_3);
            myLetters.Add(clint3Letter);

            sandy1Letter = new Letter("sandy1", "Hello, hello, sweetie! I'm so glad you come by so often, It's so boring out here. Just me and the heat and those coconuts...^Anyway, I'll give you "+DISC_1+ "% off, so come by more often, and buy lots of seeds! Bye, kid!^   ~Sandy^^PS: Please say hi to Emily from me!", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Sandy") >= HEART_LEVEL_1);
            myLetters.Add(sandy1Letter);
            sandy2Letter = new Letter("sandy2", "Hi!~ How are you, sweetie? I'm writing you this because you're such a good friend. See, I've been doing better thanks to you! Come and get your "+DISC_2+ "% off now! Just kidding. But seriously, it's so boring and hot here. Come visit me!^And buy a bunch of seeds!^   ~Sandy^^PS: Say hi to Emily! She hasn't been here in ages!", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Sandy") >= HEART_LEVEL_2);
            myLetters.Add(sandy2Letter);
            sandy3Letter = new Letter("sandy3", "Hey, hey! It's your bestie Sandy from the desert!^Hope the valley is cooler than here, hehe!^You know that you're a valued customer and friend, right? You should come by more often. I'll give you a "+DISC_3+ "% discount! Nothing happens out here, it's depressing, really. Anyway, see you soon, honey!^   ~Sandy^^PS: Greet Emily for me, will you!", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Sandy") >= HEART_LEVEL_3);
            myLetters.Add(sandy3Letter);

            willy1Letter = new Letter("willy1", "Hey there, @,^thanks for dropping by so often. I've considered expanding my shop, but let's leave that for now. I've decided to give you a "+DISC_1+ "% discount since you've been such a loyal customer. Take care.^   -Willy", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Willy") >= HEART_LEVEL_1);
            myLetters.Add(willy1Letter);
            willy2Letter = new Letter("willy2", "Hey there, fellow fishing enthusiast! Glad you decided to come here. If you ever need a new fishing pole or bait, swing by my shop by the beach, I'll give you "+DISC_2+ "% off!^^Have fun fishing and good luck!^   -Willy", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Willy") >= HEART_LEVEL_2);
            myLetters.Add(willy2Letter);
            willy3Letter = new Letter("willy3", "Hello, @!^The ocean's been exciting the past few days, fishing-wise!^The local fishing-community has expanded quite a lot since you've arrived here. I want to thank you by giving you a "+DISC_3+ "% discount in my shop. Have fun fishing!^   -Willy", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Willy") >= HEART_LEVEL_3);
            myLetters.Add(willy3Letter);

            String dwarfDisc1 = "a tenth";
            String dwarfDisc2 = "a quarter";
            String dwarfDisc3 = "a half";

            if (DISC_1 == 10)
                dwarfDisc1 = "a tenth";
            else if (DISC_1 == 20)
                dwarfDisc1 = "a fifth";
            else if (DISC_1 == 25)
                dwarfDisc1 = "a quarter";
            else if (DISC_1 == 50)
                dwarfDisc1 = "a half";
            else if (DISC_1 == 75)
                dwarfDisc1 = "a three-quart";
            else
                dwarfDisc1 = DISC_1 + "%";

            if (DISC_2 == 10)
                dwarfDisc2 = "a tenth";
            else if (DISC_2 == 20)
                dwarfDisc2 = "a fifth";
            else if (DISC_2 == 25)
                dwarfDisc2 = "a quarter";
            else if (DISC_2 == 50)
                dwarfDisc2 = "a half";
            else if (DISC_2 == 75)
                dwarfDisc2 = "a three-quart";
            else
                dwarfDisc2 = DISC_2 + "%";

            if (DISC_3 == 10)
                dwarfDisc3 = "a tenth";
            else if (DISC_3 == 20)
                dwarfDisc3 = "a fifth";
            else if (DISC_3 == 25)
                dwarfDisc3 = "a quarter";
            else if (DISC_3 == 50)
                dwarfDisc3 = "a half";
            else if (DISC_3 == 75)
                dwarfDisc3 = "a three-quart";
            else
                dwarfDisc3 = DISC_3 + "%";

            dwarf1Letter = new Letter("dwarf1", "I don't trust you, but you're a good customer. Keep " + dwarfDisc1 + " when you buy at my shop in the mines.", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Dwarf") >= HEART_LEVEL_1);
            myLetters.Add(dwarf1Letter);
            dwarf2Letter = new Letter("dwarf2", "I have lots of things for your mining pleasure. You can keep "+dwarfDisc2+".", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Dwarf") >= HEART_LEVEL_2);
            myLetters.Add(dwarf2Letter);
            dwarf3Letter = new Letter("dwarf3", "Come by my shop any time. The mine is lonely. You can keep "+dwarfDisc3+" of your money when shopping.", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Dwarf") >= HEART_LEVEL_3);
            myLetters.Add(dwarf3Letter);

            krobus1Letter = new Letter("krobus1", "You are not like other humans. You are very friendly. I will give you "+dwarfDisc1+" for that.^   -Krobus", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Krobus") >= HEART_LEVEL_1);
            myLetters.Add(krobus1Letter);
            krobus2Letter = new Letter("krobus2", "You are special, human. I get few visitors, and you're the good kind. Keep "+dwarfDisc2+ " when you shop.^   -Krobus", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Krobus") >= HEART_LEVEL_2);
            myLetters.Add(krobus2Letter);
            krobus3Letter = new Letter("krobus3", "You are a good friend. Thank you, @. I will grant you "+dwarfDisc3+ " of the price.^   -Krobus", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Krobus") >= HEART_LEVEL_3);
            myLetters.Add(krobus3Letter);

            wizard1Letter = new Letter("wizard1", "Greetings, young adept.^It is time. I have foreseen what must be done and that you will be in need of my help. In other words, I'll give you a "+DISC_1+ "% discount when the time comes.^   -M. Rasmodius, Wizard", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Wizard") >= HEART_LEVEL_1);
            myLetters.Add(wizard1Letter);
            wizard2Letter = new Letter("wizard2", "Greetings, young adept.^The spirits have changed their minds. You are in need of a greater discount when the time has arrived! I'll give you "+DISC_2+ "% off.^   -M. Rasmodius, Wizard", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Wizard") >= HEART_LEVEL_2);
            myLetters.Add(wizard2Letter);
            wizard3Letter = new Letter("wizard3", "Greetings, young adept.^My arcane powers have increased. I have foreseen that I must grant you "+DISC_3+ "% off in total if the prophecies are to be fulfilled. Beware!^   -M. Rasmodius, Wizard", (letter) => Game1.player.getFriendshipHeartLevelForNPC("Wizard") >= HEART_LEVEL_3);
            myLetters.Add(wizard3Letter);            

            // When menus change (ie. a shop window is opened), go do the magic
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;
        }        

        public void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            // TODO: This may lead to buggy behavior in that all unread letters get removed from the mailbox
            // For now we'll assume every player will read his mail every day like a good boy
            for(int i=0; i<myLetters.Count; i++)
                MailDao.RemoveLetter(myLetters.ElementAt(i));

            // Save mail status
            this.Helper.WriteJsonFile($"data/{Constants.SaveFolderName}.json", model);
        }

        public void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            // Check if any mail has to be sent
            if (Game1.player.getFriendshipHeartLevelForNPC("Robin") >= HEART_LEVEL_1 && !model.robinMail1 && DISC_1 > 0)
            {
                // First mail from Robin
                MailDao.SaveLetter(robin1Letter);
                model.robinMail1 = true; 
                //Monitor.Log("First Robin letter received ... " + robinMail1);                
            }            
            if (Game1.player.getFriendshipHeartLevelForNPC("Robin") >= HEART_LEVEL_2 && !model.robinMail2 && DISC_2 > 0)
            {
                // Second mail from Robin
                MailDao.SaveLetter(robin2Letter);
                model.robinMail2 = true; 
                //Monitor.Log("Second Robin letter received ... " + robinMail2);
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Robin") >= HEART_LEVEL_3 && !model.robinMail3 && DISC_3 > 0)
            {
                // Third mail from Robin
                MailDao.SaveLetter(robin3Letter);
                model.robinMail3 = true; 
               // Monitor.Log("Third Robin letter received ... " + robinMail3);
            }
            if(Game1.getCharacterFromName("Maru", true).isMarried() && !model.robinMailMaru && BONUS_DISC > 0)
            {
                MailDao.SaveLetter(robinMaruLetter);
                model.robinMailMaru = true;
            }
            if(Game1.getCharacterFromName("Sebastian", true).isMarried() && !model.robinMailSebastian && BONUS_DISC > 0)            
            {
                MailDao.SaveLetter(robinSebastianLetter);
                model.robinMailSebastian = true;
            }


            if (Game1.player.getFriendshipHeartLevelForNPC("Marnie") >= HEART_LEVEL_1 && !model.marnieMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(marnie1Letter);
                model.marnieMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Marnie") >= HEART_LEVEL_2 && !model.marnieMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(marnie2Letter);
                model.marnieMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Marnie") >= HEART_LEVEL_3 && !model.marnieMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(marnie3Letter);
                model.marnieMail3 = true;
            }
            // If the player has seen the blue chicken event
            if(Game1.player.eventsSeen.Contains(3900074) && !model.marnieMailShane && BONUS_DISC > 0)
            {
                //this.Monitor.Log("Player has seen the blue chickens.");
                MailDao.SaveLetter(marnieShaneLetter);
                model.marnieMailShane = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Pierre") >= HEART_LEVEL_1 && !model.pierreMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(pierre1Letter);
                model.pierreMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Pierre") >= HEART_LEVEL_2 && !model.pierreMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(pierre2Letter);
                model.pierreMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Pierre") >= HEART_LEVEL_3 && !model.pierreMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(pierre3Letter);
                model.pierreMail3 = true;
            }
            if (Game1.getCharacterFromName("Abigail", true).isMarried() && !model.pierreMailAbigail && BONUS_DISC > 0)
            {
                MailDao.SaveLetter(pierreAbigailLetter);
                model.pierreMailAbigail = true;
            }
            if(Game1.player.getFriendshipHeartLevelForNPC("Caroline") == 10 && !model.pierreMailCaroline && BONUS_DISC > 0)
            {
                MailDao.SaveLetter(pierreCarolineLetter);
                model.pierreMailCaroline = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Harvey") >= HEART_LEVEL_1 && !model.harveyMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(harvey1Letter);
                model.harveyMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Harvey") >= HEART_LEVEL_2 && !model.harveyMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(harvey2Letter);
                model.harveyMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Harvey") >= HEART_LEVEL_3 && !model.harveyMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(harvey3Letter);
                model.harveyMail3 = true;
            }
            if(Game1.getCharacterFromName("Harvey", true).isMarried() && !model.harveyMailMarried && BONUS_DISC > 0)
            {
                MailDao.SaveLetter(harveyMarriedLetter);
                model.harveyMailMarried = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Gus") >= HEART_LEVEL_1 && !model.gusMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(gus1Letter);
                model.gusMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Gus") >= HEART_LEVEL_2 && !model.gusMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(gus2Letter);
                model.gusMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Gus") >= HEART_LEVEL_3 && !model.gusMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(gus3Letter);
                model.gusMail3 = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Clint") >= HEART_LEVEL_1 && !model.clintMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(clint1Letter);
                model.clintMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Clint") >= HEART_LEVEL_2 && Game1.player.eventsSeen.Contains(97) && !model.clintMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(clint2Letter);
                model.clintMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Clint") >= HEART_LEVEL_3 && !model.clintMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(clint3Letter);
                model.clintMail3 = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Sandy") >= HEART_LEVEL_1 && !model.sandyMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(sandy1Letter);
                model.sandyMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Sandy") >= HEART_LEVEL_2 && !model.sandyMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(sandy2Letter);
                model.sandyMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Sandy") >= HEART_LEVEL_3 && !model.sandyMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(sandy3Letter);
                model.sandyMail3 = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Willy") >= HEART_LEVEL_1 && !model.willyMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(willy1Letter);
                model.willyMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Willy") >= HEART_LEVEL_2 && !model.willyMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(willy2Letter);
                model.willyMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Willy") >= HEART_LEVEL_3 && !model.willyMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(willy3Letter);
                model.willyMail3 = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Dwarf") >= HEART_LEVEL_1 && !model.dwarfMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(dwarf1Letter);
                model.dwarfMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Dwarf") >= HEART_LEVEL_2 && !model.dwarfMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(dwarf2Letter);
                model.dwarfMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Dwarf") >= HEART_LEVEL_3 && !model.dwarfMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(dwarf3Letter);
                model.dwarfMail3 = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Krobus") >= HEART_LEVEL_1 && !model.krobusMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(krobus1Letter);
                model.krobusMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Krobus") >= HEART_LEVEL_2 && !model.krobusMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(krobus2Letter);
                model.krobusMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Krobus") >= HEART_LEVEL_3 && !model.krobusMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(krobus3Letter);
                model.krobusMail3 = true;
            }

            if (Game1.player.getFriendshipHeartLevelForNPC("Wizard") >= HEART_LEVEL_1 && !model.wizardMail1 && DISC_1 > 0)
            {
                MailDao.SaveLetter(wizard1Letter);
                model.wizardMail1 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Wizard") >= HEART_LEVEL_2 && !model.wizardMail2 && DISC_2 > 0)
            {
                MailDao.SaveLetter(wizard2Letter);
                model.wizardMail2 = true;
            }
            if (Game1.player.getFriendshipHeartLevelForNPC("Wizard") >= HEART_LEVEL_3 && !model.wizardMail3 && DISC_3 > 0)
            {
                MailDao.SaveLetter(wizard3Letter);
                model.wizardMail3 = true;
            }


            MailController.UpdateMailBox();
            //this.Monitor.Log("Updated mailbox.");
        }
        
        // When menus change (ie. a shop window is opened), go do the magic
        public void MenuEvents_MenuChanged(object sender, EventArgs e)
        {
            // Force a reload of Robin's prices
            Helper.Content.InvalidateCache("Data\\Blueprints.xnb");

            // Marnie's animal shop menu
            if (Game1.activeClickableMenu is PurchaseAnimalsMenu animalsMenu)
            {               
                //this.Monitor.Log($"Animals are up.");

                // Get all animals in the menu
                List<ClickableTextureComponent> entries = animalsMenu.animalsToPurchase;

                // Get player's relationship with Marnie and set discount accordingly
                int hearts = Game1.player.getFriendshipHeartLevelForNPC("Marnie");

                //this.Monitor.Log($"Player has " + hearts + " hearts with Marnie.");

                int percentage = 0;

                if (hearts >= HEART_LEVEL_3)
                    percentage = DISC_3;
                else if (hearts >= HEART_LEVEL_2)
                    percentage = DISC_2;
                else if (hearts >= HEART_LEVEL_1)
                    percentage = DISC_1;

                //this.Monitor.Log($"Player gets " + percentage + "% off.");

                // Now update prices
                // Get through the animals and update their respective prices
                for (int i = 0; i < entries.Count; i++)
                {
                    StardewValley.Object currentAnimal = (StardewValley.Object)entries[i].item;

                    //this.Monitor.Log($"Animal " + i + " costs " + currentAnimal.price);

                    // Bonus discount on chickens if the player has seen the blue chicken event with Shane
                    if (Game1.player.eventsSeen.Contains(3900074) && i == 0)
                        currentAnimal.Price = getPercentage(currentAnimal.Price, (percentage + BONUS_DISC));
                    else
                        currentAnimal.Price = getPercentage(currentAnimal.Price, percentage);

                    //this.Monitor.Log($"Now it costs " + currentAnimal.price);                    
                }             
            }

            // Regular shops
            applyShopDiscounts("Marnie");
            applyShopDiscounts("Pierre");
            applyShopDiscounts("Robin");
            applyShopDiscounts("Harvey");
            applyShopDiscounts("Gus");
            applyShopDiscounts("Clint");
            applyShopDiscounts("Sandy");
            applyShopDiscounts("Willy");            
            applyShopDiscounts("Dwarf");
            applyShopDiscounts("Krobus");            

            // Custom NPCs
            foreach (string name in CUSTOM_NPCS)
            {
                if(!name.StartsWith("placeHolder"))
                    applyShopDiscounts(name);
            }
        }

        // AssetEditor for Robin's construction prices        
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Data\Blueprints");
        }

        // Edit building prices
        public void Edit<T>(IAssetData asset)
        {
            asset
                .AsDictionary<string, string>()
                .Set((id, data) =>
                {
                    string[] fields = data.Split('/');

                    if (!fields[0].Equals("animal"))
                    {
                        int buildingPrice = -1;
                        string resources = "";

                        if (fields.Length >= 18 && fields[17] != null)
                        {
                            buildingPrice = Int32.Parse(fields[17]);
                            resources = fields[0];

                            // Check for friendship with Robin
                            int hearts = Game1.player.getFriendshipHeartLevelForNPC("Robin");

                            int percentage = 0;

                            if (hearts >= HEART_LEVEL_3)
                                percentage = DISC_3;
                            else if (hearts >= HEART_LEVEL_2)
                                percentage = DISC_2;
                            else if (hearts >= HEART_LEVEL_1)
                                percentage = DISC_1;

                            // Bonus discount for marriage to Maru/Sebastian
                            if(Game1.getCharacterFromName("Maru", true).isMarried() || Game1.getCharacterFromName("Sebastian", true).isMarried())
                            {
                                percentage += BONUS_DISC;
                            }

                            //this.Monitor.Log($"Player has " + hearts + " hearts with Robin and receives " + percentage + "% off.");

                            // Blueprints also contains prices for Wizard buildings, so we need him too
                            int hearts_wiz = Game1.player.getFriendshipHeartLevelForNPC("Wizard");

                            int percentage_wiz = 0;

                            if (hearts_wiz >= HEART_LEVEL_3)
                                percentage_wiz = DISC_3;
                            else if (hearts_wiz >= HEART_LEVEL_2)
                                percentage_wiz = DISC_2;
                            else if (hearts_wiz >= HEART_LEVEL_1)
                                percentage_wiz = DISC_1;

                            //this.Monitor.Log($"Player has " + hearts_wiz + " hearts with the Wizard and receives " + percentage_wiz + "% off.");

                            // Beware evil hardcoding of doom
                            if (fields[8].Equals("Silo"))
                            {
                                // Update price and resource cost
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "390 " + getPercentage(100, percentage).ToString() + " 330 " + getPercentage(10, percentage).ToString() + " 334 " + getPercentage(5, percentage).ToString();
                            }
                            else if (fields[8].Equals("Mill"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(150, percentage).ToString() + " 390 " + getPercentage(50, percentage).ToString() + " 428 " + getPercentage(4, percentage).ToString();
                            }
                            else if (fields[8].Equals("Stable"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "709 " + getPercentage(100, percentage).ToString() + " 335 " + getPercentage(5, percentage).ToString();
                            }
                            else if (fields[8].Equals("Well"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "390 " + getPercentage(75, percentage).ToString();
                            }
                            else if (fields[8].Equals("Coop"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(300, percentage).ToString() + " 390 " + getPercentage(100, percentage).ToString();
                            }
                            else if (fields[8].Equals("Big Coop"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(400, percentage).ToString() + " 390 " + getPercentage(150, percentage).ToString();
                            }
                            else if (fields[8].Equals("Deluxe Coop"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(500, percentage).ToString() + " 390 " + getPercentage(200, percentage).ToString();
                            }
                            else if (fields[8].Equals("Barn"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(350, percentage).ToString() + " 390 " + getPercentage(150, percentage).ToString();
                            }
                            else if (fields[8].Equals("Big Barn"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(450, percentage).ToString() + " 390 " + getPercentage(200, percentage).ToString();
                            }
                            else if (fields[8].Equals("Deluxe Barn"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(550, percentage).ToString() + " 390 " + getPercentage(300, percentage).ToString();
                            }
                            else if (fields[8].Equals("Slime Hutch"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "390 " + getPercentage(500, percentage).ToString() + " 338 " + getPercentage(10, percentage).ToString() + " 337 1";
                            }
                            else if (fields[8].Equals("Shed"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(300, percentage).ToString();
                            }
                            else if(fields[8].Equals("Stone Cabin"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "390 " + getPercentage(10, percentage).ToString();
                            }
                            else if(fields[8].Equals("Plank Cabin"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(5, percentage).ToString() + " 771 "+getPercentage(10, percentage).ToString();
                            }
                            else if(fields[8].Equals("Log Cabin"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(10, percentage).ToString();
                            }
                            else if(fields[8].Equals("Shipping Bin"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage).ToString();
                                fields[0] = "388 " + getPercentage(150, percentage).ToString();
                            }
                            // Wizard buildings
                            else if(fields[8].Equals("Earth Obelisk"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage_wiz).ToString();
                                fields[0] = "337 " + getPercentage(10, percentage_wiz).ToString() + " 86 "+getPercentage(10, percentage_wiz).ToString();
                            }
                            else if (fields[8].Equals("Water Obelisk"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage_wiz).ToString();
                                fields[0] = "337 " + getPercentage(5, percentage_wiz).ToString() + " 372 " + getPercentage(10, percentage_wiz).ToString() + " 393 " + getPercentage(10, percentage_wiz).ToString();
                            }
                            else if (fields[8].Equals("Junimo Hut"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage_wiz).ToString();
                                fields[0] = "390 " + getPercentage(200, percentage_wiz).ToString() + " 268 " + getPercentage(9, percentage_wiz).ToString() + " 771 "+getPercentage(100, percentage_wiz).ToString();
                            }
                            else if (fields[8].Equals("Gold Clock"))
                            {
                                fields[17] = getPercentage(buildingPrice, percentage_wiz).ToString();                                
                            }
                        }
                    }

                    return string.Join("/", fields);
                });
        }

        /* ***************************************************************************************************************************** */

        // Returns a percentage of a base value
        private int getPercentage(double val, double perc)
        {

            double subst = (val / 100) * perc;
            int result = (int)val - (int)subst;

            return result;
        }
        
        private void applyShopDiscounts(string characterName)
        {
            if (Game1.activeClickableMenu is ShopMenu shopMenu && shopMenu.portraitPerson == Game1.getCharacterFromName(characterName, true))
            {         
                // Get player's relationship with character and set discount accordingly
                int hearts = Game1.player.getFriendshipHeartLevelForNPC(characterName);

                int percentage = 0;

                if (hearts >= HEART_LEVEL_3)
                    percentage = DISC_3;
                else if (hearts >= HEART_LEVEL_2)
                    percentage = DISC_2;
                else if (hearts >= HEART_LEVEL_1)
                    percentage = DISC_1;

                // Special additional discounts                
                if (characterName.Equals("Robin"))
                {
                    // If player is married to Maru or Sebastian, give additional 5% discount
                    if (Game1.getCharacterFromName("Maru", true).isMarried() || Game1.getCharacterFromName("Sebastian", true).isMarried())
                    {
                        percentage += BONUS_DISC;
                    }
                }
                if(characterName.Equals("Pierre"))
                {
                    // If player is married to Abigail, give 5%. If he's also best friends with Caroline, give an additional 5%
                    if (Game1.getCharacterFromName("Abigail", true).isMarried())
                        percentage += BONUS_DISC;
                    if (Game1.player.getFriendshipHeartLevelForNPC("Caroline") == 10)
                        percentage += BONUS_DISC;
                }
                if(characterName.Equals("Harvey"))
                {
                    // If player is married to Harvey, give an additional 5%
                    if (Game1.getCharacterFromName("Harvey", true).isMarried())
                        percentage += BONUS_DISC;
                }

                // Prices are in the itemPriceAndStock dictionary. The first number of the int[] is the price. Second is stock probably?
                Dictionary<Item, int[]> priceAndStock = this.Helper.Reflection.GetField<Dictionary<Item, int[]>>(shopMenu, "itemPriceAndStock").GetValue();

                //this.Monitor.Log($"Player has " + hearts + " hearts with " + characterName+" and receives "+percentage+"% off.");

                // Change supply shop prices here
                foreach (KeyValuePair<Item, int[]> kvp in priceAndStock)
                {
                    //this.Monitor.Log($"Old price: " + kvp.Value.GetValue(0));
                    kvp.Value.SetValue(getPercentage(double.Parse(kvp.Value.GetValue(0).ToString()), percentage), 0);
                    //this.Monitor.Log($"New price: " + kvp.Value.GetValue(0));
                }
            }
        }
    }
    
}