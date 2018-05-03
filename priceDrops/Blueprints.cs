using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace priceDrops
{
    class Blueprints : IAssetEditor
    {
        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Data\Blueprints");
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
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

                            if (hearts >= 10)
                                percentage = 50;
                            else if (hearts >= 7)
                                percentage = 25;
                            else if (hearts >= 3)
                                percentage = 10;

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

        /*********
        ** Private methods
        *********/

        // Returns a percentage of a base value
        private int getPercentage(double val, double perc)        {
            
            double subst = (val / 100) * perc;
            int result = (int)val - (int)subst;
            
            return result;
        }
    }
}
