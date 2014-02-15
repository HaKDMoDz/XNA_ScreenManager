﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_ScreenManager.ItemClasses;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace XNA_ScreenManager.PlayerClasses
{
    public enum PlayerStats
    {
        ATK,
        DEF,
        MATK,
        MDEF,
        ASPD,
        HIT,
        FLEE,
        MAXHP,
        MAXSP,
    }

    public sealed class PlayerInfo
    {
        Equipment equipment = Equipment.Instance;   // Equipment

        #region texture properties

        public string body_sprite;
        public string hair_sprite;
        public string faceset_sprite;

        public string hatgear_sprite
        {
            get
            {
                string equip = null;

                if (equipment.item_list.FindAll(delegate(Item item) { return item.itemSlot == ItemSlot.Headgear; }).Count > 0)
                    equip = equipment.item_list.Find(delegate(Item item) { return item.itemSlot == ItemSlot.Headgear; }).equipSpritePath;

                if (equip != null)
                    return equip;
                else
                    return null;
            }
        }
        public string costume_sprite
        {
            get 
            {
                string equip = null;

                if (equipment.item_list.FindAll(delegate(Item item) { return item.itemSlot == ItemSlot.Bodygear; }).Count > 0)
                    equip = equipment.item_list.Find(delegate(Item item) { return item.itemSlot == ItemSlot.Bodygear; }).equipSpritePath;

                if (equip != null)
                    return equip;
                else
                    return null;
            }
        }
        public string weapon_sprite
        {
            get
            {
                string equip = null;

                if (equipment.item_list.FindAll(delegate(Item item) { return item.itemSlot == ItemSlot.Weapon; }).Count > 0)
                    equip = equipment.item_list.Find(delegate(Item item) { return item.itemSlot == ItemSlot.Weapon; }).equipSpritePath;

                if (equip != null)
                    return equip;
                else
                    return null;
            }
        }

        #endregion

        #region properties
        private string name, gender, jobclass;
        private int maxhp, hp, maxsp, sp, exp, nlexp, lvl, gold;
        private int str, agi, vit, intel, dex, luk;
        #endregion

        #region general info
        // General Info
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public string Gender
        {
            get { return this.gender; }
            set { this.gender = value; }
        }
        public string Jobclass
        {
            get { return this.jobclass; }
            set { this.jobclass = value; }
        }
        public int Exp
        {
            get { return this.exp; }
            set { this.exp = value; }
        }
        public int NextLevelExp
        {
            get { return this.nlexp; }
            set { this.nlexp = value; }
        }
        public int Level
        {
            get { return this.lvl; }
            set { this.lvl = value; }
        }
        public int Gold
        {
            get { return this.gold; }
            set { this.gold = value; }
        }
#endregion

        #region battleinfo
        // Battle Info
        // for more info http://irowiki.org/wiki/ATK#Status_ATK
        public int ATK
        {
            get { return (int)(StatusATK * 2 + WeaponATK + EquipATK + MasteryATK); }
        }
        public int MATK
        {
            get { return (int)(intel + (intel/2) + (dex/5) + (luk/3) + (lvl/4)); }
        }
        public int DEF
        {
            get { return (int)(HardDef + SoftDef); }
        } // for equipment screen
        public int BattleDEF
        {
            get { return (int)((4000 + HardDef) / (4000 + HardDef * 10)); }
        } // for Battle calculation
        public int SoftDef
        {
            get { return (int)((lvl /2) + (vit / 2) + (agi /2)); }
        } // for Battle calculation
        public int HardDef
        {
            get
            {
                int defmod = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.itemType == ItemType.Armor; }))
                {
                    defmod += item.defModifier;
                }
                return defmod;
            }
        }
        public float DamageReduced
        {
            get
            {
                return (1 - (600 / (HardDef + RefineDef + 600))) * 100;
            }
        }
        public int RefineDef
        {
            get
            {
                int refmod = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.itemType == ItemType.Armor; }))
                {
                    refmod += item.RefinementBonus;
                }
                return refmod;
            }
        }
        public int MDEF
        {
            get { return (int)(intel + vit / 5 + dex / 5 + lvl / 4); }
        }
        public int HIT
        {
            get { return (int)(this.Level + this.dex + 175); }
        }
        public int FLEE
        {
            get { return (int)((this.Level + this.agi + 100) / 5); }
        }
        public int HP
        {
            get { return this.hp; }
            set { this.hp = value; }
        }
        public int MAXHP
        {
            get { return this.maxhp; }
            set { this.maxhp = value; }
        }
        public int SP
        {
            get { return this.sp; }
            set { this.sp = value; }
        }
        public int MAXSP
        {
            get { return this.maxsp; }
            set { this.maxsp = value; }
        }
        public int StatusATK
        {
            get { return (int)(str + (dex / 5) + (luk / 3) + (lvl / 4)); }
        }
        public int WeaponATK
        {
            get { return (int)((BaseWeaponATK + Variance + STRBonus + RefinementBonus ) * SizePenalty / 100); }
        }
        public int SizePenalty
        {
            get { return 100; } // to do monster size table
        }
        public int EquipATK
        {
            get { return 0; } // to do (buffs + cards etc)
        }
        public int MasteryATK
        {
            get { return 0; } // to do
        }
        public int Variance
        {
            get 
            { 
                int WeaponLevel = 0;
                foreach(Item item in equipment.item_list.FindAll(delegate(Item item) { return item.itemType == ItemType.Weapon; }))
                {
                    WeaponLevel += item.WeaponLevel;
                }
                return (int)(0.05f * WeaponLevel * BaseWeaponATK); 
            }
        }
        public int BaseWeaponATK
        {
            get 
            {
                int atkmod = 0;
                foreach(Item item in equipment.item_list.FindAll(delegate(Item item) { return item.itemType == ItemType.Weapon; }))
                {
                    atkmod += item.atkModifier;
                }
                return atkmod; 
            }
        }
        public int RefinementBonus
        {
            get
            {
                int atkmod = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.itemType == ItemType.Weapon; }))
                {
                    atkmod += item.RefinementBonus;
                }
                return atkmod;
            }
        }
        public int STRBonus
        {
            get { return (int)((200 * str) / 200);}
        }
        public int ASPD
        {
            get { return (int)((Math.Sqrt(Math.Pow(agi, 2) / 2) + Math.Sqrt(Math.Pow(dex, 2) / 5)) / 4); }
        }
        #endregion

        #region player stats
        // Player Stats
        public int Strength
        {
            get { return this.str; }
            set { this.str = value; }
        }
        public int Agility
        {
            get { return this.agi; }
            set { this.agi = value; }
        }
        public int Vitality
        {
            get { return this.vit; }
            set { this.vit = value; }
        }
        public int Intelligence
        {
            get { return this.intel; }
            set { this.intel = value; }
        }
        public int Dexterity
        {
            get { return this.dex; }
            set { this.dex = value; }
        }
        public int Luck
        {
            get { return this.luk; }
            set { this.luk = value; }
        }
        #endregion

        #region constructor
        private static PlayerInfo instance;
        private PlayerInfo(){}

        public static PlayerInfo Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerInfo();
                }
                return instance;
            }
        }
        #endregion

        // init player values
        public void InitNewGame()
        {
            this.name = "Wouter";
            this.jobclass = "Fighter";
            this.gold = 100;
            this.exp = 0;
            this.nlexp = 1200;
            this.hp = 100;
            this.maxhp = 100;
        }
    }
}
