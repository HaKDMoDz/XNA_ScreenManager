﻿using System;
using System.Collections.Generic;

namespace XNA_ScreenManager.ItemClasses
{
    public enum ItemType
    {
        Cash,
        Collectable,
        Consumable,
        Weapon,
        Armor,
        Accessory,
        KeyItem,
        Equipable = ItemType.Weapon | ItemType.Armor | ItemType.Accessory
    };

    public enum WeaponType
    {
        Dagger,
        One_handed_Sword,
        Two_handed_Sword,
        One_handed_Spear,
        Two_handed_Spear,
        One_handed_Axe,
        Two_handed_Axe,
        Mace,
        Staff,
        Bow,
        None
    };

    public enum ItemClass
    {
        Bowman,
        Warrior,
        Magician,
        Priest,
        Thief,
        Pirate,
        All
    };

    public enum ItemSlot
    {
        Weapon,         // both hands i.e. two-handed sword and bows
        Shield,         // shoulds i.e. cape
        Headgear,       // complete head i.e. helmet
        Neck,           // necklace and scarf
        Bodygear,       // complete body i.e. cloak
        Feet,           // Feet i.e. boots
        Accessory,      // rings etc..
        None           // not applicable
    };

    [Serializable]
    public struct spriteOffset
    {
        public string Name;
        public int ID, X, Y;

        public spriteOffset(int id, string name = "", int x = 0, int y = 0)
        {
            this.ID = id;
            this.Name = name;
            this.X = x;
            this.Y = y;
        }
    }

    [Serializable]
    public class Item
    {
        public int itemID { get; set; }
        public string itemName { get; set; }
        public string itemDescription { get; set; }

        public string itemSpritePath { get; set; }
        public string equipSpritePath { get; set; }
        public int SpriteFrameX { get; set; }
        public int SpriteFrameY { get; set; }

        public int DEF { get; set; }
        public int ATK { get; set; }
        public int Magic { get; set; }
        public int Speed { get; set; }
        public int Price { get; set; }
        public int WeaponLevel { get; set; }
        public int RefinementLevel { get; set; }

        public ItemType Type { get; set; }
        public ItemClass Class { get; set; }
        public ItemSlot Slot { get; set; }
        public WeaponType WeaponType { get; set; }

        public string Script { get; set; }
                
        public List<spriteOffset> list_offsets = new List<spriteOffset>();

        public static Item create(int identifier, string name, ItemType type)
        {
            var results = new Item();

            results.itemID = identifier;
            results.Type = type;
            results.itemName = name;
            results.WeaponLevel = 1;
            results.RefinementLevel = 0;
            results.WeaponType = WeaponType.None;

            return results;
        }
    }
}
