using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Events;
using System;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SDG.Framework.Utilities;
using Rocket.Unturned.Chat;
using Steamworks;

namespace walterhcain.CookingCraftingAndCropRaisingContainers
{
    public class CookP : RocketPlugin<CookC>
    {
        public static CookP Instance;
        public DateTime lastcalled;
        private Dictionary<UnityEngine.Transform, DateTime> storages = new Dictionary<UnityEngine.Transform, DateTime>();
        private Dictionary<UnityEngine.Transform, CookC.Recipe> recipes = new Dictionary<UnityEngine.Transform, CookC.Recipe>();
        //private Dictionary<UnityEngine.Transform, int> multiples = new Dictionary<UnityEngine.Transform, int>();
        private Dictionary<UnityEngine.Transform, CSteamID> owners = new Dictionary<UnityEngine.Transform, CSteamID>();
        public Dictionary<UnityEngine.Transform, CSteamID> resets = new Dictionary<UnityEngine.Transform, CSteamID>();
        public List<UnityEngine.Transform> clears = new List<UnityEngine.Transform>();

        protected override void Load()
        {
            Instance = this;
            Logger.Log("Loading Plugin: 0%");
            Logger.Log("Looks like it's time to oil it up");
            Logger.Log("Loading Plugin: 50%");
            Logger.Log("Checking if the bass is cooked");
            lastcalled = DateTime.Now;
            Logger.Log("Loading Plugin: 100%");
            UnturnedPlayerEvents.OnPlayerUpdateGesture += CainGesture;
            Logger.Log("Cain's Cooking Crafting and Crop-Raising Containers has been successfully loaded!");
            Logger.Log("---------------------------------------------------------");
            Logger.Log("Above average thanks to BigDaddyBobbo for coming up with the name");
            Logger.Log("---------------------------------------------------------");
        }

        protected override void Unload()
        {
            Logger.Log("Cain's Cooking Crafting and Crop-Raising Containers has been successfully unloaded!");
        }

        private void CainGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            //Check for gesture is point
            if(gesture == UnturnedPlayerEvents.PlayerGesture.Point)
            {
                //Get RaycastHit
                UnityEngine.RaycastHit hit;
                if (PhysicsUtility.raycast(new UnityEngine.Ray(player.Player.look.aim.position, player.Player.look.aim.forward), out hit, UnityEngine.Mathf.Infinity, RayMasks.BARRICADE))
                {
                    InteractableStorage storage = hit.transform.GetComponent<InteractableStorage>();
                   //Get InteractableStorage
                    if (storage != null)
                    {
                        Logger.Log(storage.name);
                        if (storage.name == "15692" || storage.name == "15686")
                        {
                            //if (Configuration.Instance.storageID.Contains(storage.name)
                            Logger.Log("Got Storage");
                            //Check if they can access the storage
                            if ((storage.owner == player.CSteamID || storage.group == player.SteamGroupID))
                            {
                                //Gets the number of items in the storage
                                byte itemCount = storage.items.getItemCount();
                                if (!storages.ContainsKey(storage.transform))
                                {
                                    storages.Remove(storage.transform);
                                    recipes.Remove(storage.transform);
                                    //multiples.Remove(storage.transform);
                                    owners.Remove(storage.transform);
                                }
                                else
                                {
                                    //ADD CHECK HERE FOR CONFIRMATION
                                    
                                }
                                //If count is equal to 0, no need to continue
                                if (itemCount > 0)
                                {
                                    bool right2 = false;
                                    //rin is the recipe index
                                    int rin = 0;
                                    Logger.Log("Go through recipes");
                                    //Go through the recipes
                                    bool found = false;
                                    foreach (CookC.Recipe r in Configuration.Instance.Recipes)
                                    {
                                        //multiples list to see how many * there are of the recipe
                                        List<int> lint = new List<int>();
                                        //Recipe is correct?

                                        bool right = true;
                                        //Check if the item count is greater than recipe itemcount
                                        if (r.getIngredientCount() <= itemCount)
                                        {
                                            Logger.Log("Go through Ingredients");
                                            //Gets indegrient from recipe
                                            foreach (CookC.Ingredient i in r.Ingredients)
                                            {
                                                //Check if the amount of ingredient is greater than the needed amount
                                                if (sameItemCount(storage.items, i.ItemId) < i.Amount)
                                                {
                                                    //Sets the recipe to false.
                                                    right = false;
                                                    break;
                                                }
                                            }
                                            //Checks if it was the right recipe
                                            if (right == true)
                                            {
                                                found = true;
                                                Logger.Log("Got Recipe");
                                                //Adds the storage to dictionaries
                                                if (!storages.ContainsKey(storage.transform))
                                                {
                                                    storages.Add(storage.transform, DateTime.Now.AddSeconds(Configuration.Instance.Recipes[rin].cookTime));
                                                }
                                                else
                                                {
                                                    storages[storage.transform] = DateTime.Now.AddSeconds(Configuration.Instance.Recipes[rin].cookTime);
                                                }
                                                if (!owners.ContainsKey(storage.transform))
                                                {
                                                    owners.Add(storage.transform, player.CSteamID);
                                                }
                                                else
                                                {
                                                    owners[storage.transform] = player.CSteamID;
                                                }
                                                if (!recipes.ContainsKey(storage.transform))
                                                {
                                                    recipes.Add(storage.transform, r);
                                                }
                                                else
                                                {
                                                    recipes[storage.transform] = r;
                                                }

                                                UnturnedChat.Say(player, "Your " + r.Name + " will be ready in " + r.cookTime + " seconds.");

                                                right2 = true;
                                                break;
                                            }
                                        }
                                        if (!right2)
                                        {
                                            rin++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        Logger.Log("No Recipe Found");
                                        UnturnedChat.Say(player, "Not enough required ingredients.");
                                    }
                                }

                            }
                            else
                            {
                                UnturnedChat.Say(player, "You are not part of the group that owns this still.");
                            }
                        }
                    }
                }
            }
        }

        private byte sameItemCount(Items i, ushort id)
        {
            List<InventorySearch> ins = i.search(new List<InventorySearch>(), id, true, true);
            Logger.Log(ins.Count.ToString());
            return (byte)ins.Count;
        }


        void FixedUpdate()
        {
            if (storages.Count != 0)
            {
                if (lastcalled.AddSeconds(1) < DateTime.Now)
                {
                    foreach (KeyValuePair<UnityEngine.Transform, DateTime> t in storages)
                    {

                        List<byte> multi = new List<byte>();
                        if (DateTime.Now > t.Value)
                        {
                            if(t.Value.AddSeconds(3) < DateTime.Now)
                            {
                                clears.Add(t.Key);
                                break;
                            }

                            

                            InteractableStorage storage = t.Key.GetComponent<InteractableStorage>();
                            
                            foreach (CookC.Ingredient i in recipes[t.Key].Ingredients)
                            {
                                multi.Add((byte)(sameItemCount(storage.items, i.ItemId) / i.Amount));
                            }
                            byte mult = getMin(multi);
                            foreach (CookC.Ingredient i in recipes[t.Key].Ingredients)
                            {
                                RemoveInventory(i.ItemId, i.Amount * mult, storage);
                            }
                            for (int h = 0; h < mult; h++)
                            {
                                storage.items.tryAddItem(new Item(recipes[t.Key].id, true));
                            }
                            UnturnedPlayer playe = UnturnedPlayer.FromCSteamID(storage.owner);
                            storages.Remove(storage.transform);
                            //multiples.Remove(storage.transform);
                            recipes.Remove(storage.transform);
                            if (Provider.clients.Contains(playe.SteamPlayer()))
                            {
                                UnturnedChat.Say(UnturnedPlayer.FromCSteamID(owners[storage.transform]), "Your berry juice has finished distilling.");
                            }
                            owners.Remove(storage.transform);
                            break;

                        }
                    }
                    if (clears.Count > 0)
                    {
                        Logger.Log("Removing");
                        foreach (UnityEngine.Transform trans in clears)
                        {
                            Logger.Log("Going in");
                            storages.Remove(trans);
                        }
                        clears = new List<UnityEngine.Transform>();
                    }
                }
            }
        }
        
        private byte getMin(List<byte> l)
        {
            byte by = 128;
            foreach(byte b in l) 
            {
               if (b < by)
               {
                  by = b;
               }
            }
                return by;
        }

        public void RemoveInventory(ushort ItemID, int Amount, InteractableStorage it)
        {
            int AmountFound = 0;
            Logger.Log(Amount.ToString());
                for (byte w = 0; w < it.items.width; w++)
                {
                    for (byte h = 0; h < it.items.height; h++)
                    {
                        try
                        {
                            byte index = it.items.getIndex(w, h);
                            if (index == 255) continue;
                            if (it.items.getItem(index).item.id == ItemID)
                            {
                                it.items.removeItem(index);
                                AmountFound++;
                                if (AmountFound == Amount)
                                {
                                    return;
                                }
                            }
                        }
                        catch { }
                    }
                }
            return;
            }
        }

    }

