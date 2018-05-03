using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace priceDrops
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {
        // Heart levels and corresponding discounts
        private int HEART_LEVEL_1 = -1;
        private int HEART_LEVEL_2 = -1;
        private int HEART_LEVEL_3 = -1;
        private int DISC_1 = -1;
        private int DISC_2 = -1;
        private int DISC_3 = -1;
        private List<string> CUSTOM_NPCS = new List<string>();
        // Krobus will incrementally apply discounts for some arcane reason, so discounts should only be applied once a day
        private Boolean krobusToday = false;
        private int krobusHearts = -1;

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
            CUSTOM_NPCS = config.customNPCs;

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


            // When menus change (ie. a shop window is opened), go do the magic
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;

            // Krobus is bugged, so he needs special care
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
        }       

        // Reset detector for if Krobus was talked to today and get his current heart level
        public void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            //this.Monitor.Log($"Resetting Krobus settings for the day.");
            krobusToday = false;
            krobusHearts = Game1.player.getFriendshipHeartLevelForNPC("Krobus");
            //this.Monitor.Log($"Player has " + krobusHearts + " hearts with Krobus.");
        }

        // When menus change (ie. a shop window is opened), go do the magic
        public void MenuEvents_MenuChanged(object sender, EventArgs e)
        {
            // Force a reload of Robin's prices
            Helper.Content.InvalidateCache("Data\\Blueprints.xnb");

            // Marnie's animal shop menu
            if (Game1.activeClickableMenu is PurchaseAnimalsMenu animalsMenu)
            {



                // --> //Game1.NPCGiftTastes <--
               // Game1.NPCGiftTastes;


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

                            //this.Monitor.Log($"Player has " + hearts + " hearts with Robin and receives " + percentage + "% off.");

                            // Update price and resource cost
                            fields[17] = getPercentage(buildingPrice, percentage).ToString();

                            // Beware evil hardcoding of doom
                            if (fields[8].Equals("Silo"))
                            {
                                fields[0] = "390 " + getPercentage(100, percentage).ToString() + " 330 " + getPercentage(10, percentage).ToString() + " 334 " + getPercentage(5, percentage).ToString();
                            }
                            else if (fields[8].Equals("Mill"))
                            {
                                fields[0] = "388 " + getPercentage(150, percentage).ToString() + " 390 " + getPercentage(50, percentage).ToString() + " 428 " + getPercentage(4, percentage).ToString();
                            }
                            else if (fields[8].Equals("Stable"))
                            {
                                fields[0] = "709 " + getPercentage(100, percentage).ToString() + " 335 " + getPercentage(5, percentage).ToString();
                            }
                            else if (fields[8].Equals("Well"))
                            {
                                fields[0] = "390 " + getPercentage(75, percentage).ToString();
                            }
                            else if (fields[8].Equals("Coop"))
                            {
                                fields[0] = "388 " + getPercentage(300, percentage).ToString() + " 390 " + getPercentage(100, percentage).ToString();
                            }
                            else if (fields[8].Equals("Big Coop"))
                            {
                                fields[0] = "388 " + getPercentage(400, percentage).ToString() + " 390 " + getPercentage(150, percentage).ToString();
                            }
                            else if (fields[8].Equals("Deluxe Coop"))
                            {
                                fields[0] = "388 " + getPercentage(500, percentage).ToString() + " 390 " + getPercentage(200, percentage).ToString();
                            }
                            else if (fields[8].Equals("Barn"))
                            {
                                fields[0] = "388 " + getPercentage(350, percentage).ToString() + " 390 " + getPercentage(150, percentage).ToString();
                            }
                            else if (fields[8].Equals("Big Barn"))
                            {
                                fields[0] = "388 " + getPercentage(450, percentage).ToString() + " 390 " + getPercentage(200, percentage).ToString();
                            }
                            else if (fields[8].Equals("Deluxe Barn"))
                            {
                                fields[0] = "388 " + getPercentage(550, percentage).ToString() + " 390 " + getPercentage(300, percentage).ToString();
                            }
                            else if (fields[8].Equals("Slime Hutch"))
                            {
                                fields[0] = "390 " + getPercentage(500, percentage).ToString() + " 338 " + getPercentage(10, percentage).ToString() + " 337 1";
                            }
                            else if (fields[8].Equals("Shed"))
                            {
                                fields[0] = "388 " + getPercentage(300, percentage).ToString();
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

                // Prices are in the itemPriceAndStock dictionary. The first number of the int[] is the price. Second is stock probably?
                Dictionary<Item, int[]> priceAndStock = this.Helper.Reflection.GetField<Dictionary<Item, int[]>>(shopMenu, "itemPriceAndStock").GetValue();

                // If friendship with Krobus changed, recalculate base values of his items and re-apply new discounts
                if (characterName.Equals("Krobus"))
                {
                    //this.Monitor.Log($"Yep, it's Krobus.");

                    if(krobusHearts != hearts)
                    {
                        //this.Monitor.Log($"Krobus' hearts changed during the day! Hearts at day start: " + krobusHearts + " .. hearts now: " + hearts);

                        // Get the old discount
                        double perc = 0;

                        if (krobusHearts >= HEART_LEVEL_3)
                            perc = DISC_3;
                        else if (krobusHearts >= HEART_LEVEL_2)
                            perc = DISC_2;
                        else if (krobusHearts >= HEART_LEVEL_1)
                            perc = DISC_1;

                        //this.Monitor.Log($"Percentage off: " + perc);

                        // Calculate up the original price                        

                        // To get original price from discounted price:
                        // (( 100 - discount ) / 100) * pOriginal = pDiscounted
                        // => pOriginal = pDiscounted * (100 / (100 - d))

                        //this.Monitor.Log($"Recalculating original prices... multiplying by "+perc);

                        // Multiply reduced prices with what we calculated above to go back to original prices
                        foreach(KeyValuePair<Item, int[]> kvp in priceAndStock)
                        {
                            int discountedPrice = int.Parse(kvp.Value.GetValue(0).ToString());
                            int originalPrice = 0;

                            if (perc == 100)
                                //this.Monitor.Log($"100% off? Really? You deserve this crash :3");

                            originalPrice = (int)((double)discountedPrice * (100 / (100 - perc)));

                            kvp.Value.SetValue(originalPrice, 0);
                        }

                        //this.Monitor.Log($"Reset prices.");

                        krobusToday = false;
                        krobusHearts = hearts;
                    }
                }                

                // If player has not talked to Krobus today yet, apply discounts
                if(characterName.Equals("Krobus") && !krobusToday || !characterName.Equals("Krobus"))
                { 
                    //this.Monitor.Log($"Player has " + hearts + " hearts with " + characterName+" and receives "+percentage+"% off.");

                    // Change supply shop prices here
                    foreach (KeyValuePair<Item, int[]> kvp in priceAndStock)
                    {
                        //this.Monitor.Log($"Old price: " + kvp.Value.GetValue(0));
                        kvp.Value.SetValue(getPercentage(double.Parse(kvp.Value.GetValue(0).ToString()), percentage), 0);
                        //this.Monitor.Log($"New price: " + kvp.Value.GetValue(0));
                    }

                    if (characterName.Equals("Krobus"))
                        krobusToday = true;
                }
            }
        }
    }
    
}