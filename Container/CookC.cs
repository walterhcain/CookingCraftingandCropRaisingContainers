using Rocket.API;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace walterhcain.CookingCraftingAndCropRaisingContainers
{
    public class CookC : IRocketPluginConfiguration
    {
        [XmlArray("Stills")]
        public List<ushort> storageID;

        [XmlArray("Recipes")]
        public List<Recipe> Recipes;

        public void LoadDefaults()
        {
            storageID = new List<ushort>
            {
                15692,
                15686
            };


            Recipes = new List<Recipe>
            {
                new Recipe() { Name = "Berry Juice",id = 271, cookTime = 30, Ingredients = new List<Ingredient>() { new Ingredient(1105, 1) } }
            };
            
        }

        public class Recipe
        {
            public Recipe()
            { }
            public string Name;
            public ushort id;
            public int cookTime;

            [XmlArrayItem(ElementName = "Ingredient")]
            public List<Ingredient> Ingredients;

            public byte getIngredientCount()
            {
                byte num = 0;
                foreach(Ingredient i in Ingredients)
                {
                    num += i.Amount;
                }
                return num;
            }
        }

        public class Ingredient
        {
            public Ingredient()
            { }
            public Ingredient(ushort itemId, byte amount)
            {
                ItemId = itemId;
                Amount = amount;
            }

            [XmlAttribute("id")]
            public ushort ItemId;

            [XmlAttribute("amount")]
            public byte Amount;
        }
    }
}
