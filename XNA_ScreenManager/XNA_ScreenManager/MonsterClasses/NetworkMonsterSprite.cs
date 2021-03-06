﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA_ScreenManager.MapClasses;
using XNA_ScreenManager.PlayerClasses;
using XNA_ScreenManager.ItemClasses;
using System.Collections.Generic;
using System.Reflection;
using XNA_ScreenManager.MonsterClasses;
using XNA_ScreenManager.ScreenClasses.MainClasses;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Text.RegularExpressions;
using XNA_ScreenManager.CharacterClasses;

namespace XNA_ScreenManager.MonsterClasses
{
    public class NetworkMonsterSprite : Entity
    {
        #region properties

        // static randomizer
        ContentManager Content = ResourceManager.GetInstance.Content;
        ResourceManager.randomizer Randomizer = ResourceManager.randomizer.Instance;                // generate unique random ID
        PlayerStore PlayerInfo = PlayerStore.Instance;                                              // get battle information of player

        // Monster Store ID
        public int MonsterID = 0;
        List<int[]> ItemDrop = new List<int[]>();

        // Drawing properties
        private int spriteframe, prevspriteframe;
        private string spritepath, spritename;
        public Vector2 spriteOfset;
        public List<spriteOffset> list_offsets = new List<spriteOffset>();
        public SpriteEffects spriteEffect = SpriteEffects.None;
        private float transperancy = 0;
        private bool debug = false;

        // Respawn properties
        private Vector2 resp_pos = Vector2.Zero,                                                    // Respawn Position
                        resp_bord = Vector2.Zero;                                                   // Walking Border

        // Sprite Animation Properties
        Color color = Color.White;                                                                  // Sprite color
        private Vector2 Direction = Vector2.Zero;                                                   // Sprite Move direction
        private Vector2 Velocity = Vector2.Zero;                                                    // Jump Movement
        private float Speed;                                                                        // Speed used in functions
        float previousAnimateTimeSec;                                                               // Animation in Miliseconds

        // Movement properties
        public int WALK_SPEED = 97;                                                                 // The actual speed of the entity
        public int RUN_SPEED = 133;                                                                 // The actual speed of the entity
        const int ANIMATION_SPEED = 120;                                                            // Animation speed, 120 = default
        Border Borders = new Border(0, 0);                                                          // max tiles to walk from center (avoid falling)

        // Server Updates        
        EntityState ServerUpdate_state;
        Vector2 ServerUpdate_position;
        SpriteEffects ServerUpdate_spriteEffect;

        #endregion

        public NetworkMonsterSprite(int ID, string guid, Vector2 position, Vector2 borders)
            : base()
        {
            // Derived properties
            Position = position;
            OldPosition = position;
            EntityType = "monster";

            // Set first server update values
            ServerUpdate_position = position;
            ServerUpdate_state = EntityState.Spawn;
            ServerUpdate_spriteEffect = SpriteEffects.None;

            spriteframe = 0;
            spritepath = MonsterStore.Instance.getMonster(ID).monsterSprite;
            spritename = "stand_" + spriteframe.ToString();
            sprite = Content.Load<Texture2D>(spritepath + spritename);
            SpriteFrame = new Rectangle(0, 0, sprite.Width, sprite.Width);

            loadoffsetfromXML();

            // Save for respawning
            resp_pos = position;
            resp_bord = borders;

            // get battle information from monster database
            HP = MonsterStore.Instance.getMonster(ID).HP;
            MP = MonsterStore.Instance.getMonster(ID).Magic;
            ATK = MonsterStore.Instance.getMonster(ID).ATK;
            DEF = MonsterStore.Instance.getMonster(ID).DEF;
            LVL = MonsterStore.Instance.getMonster(ID).Level;
            HIT = MonsterStore.Instance.getMonster(ID).Hit;
            FLEE = MonsterStore.Instance.getMonster(ID).Flee;
            EXP = MonsterStore.Instance.getMonster(ID).EXP;
            SIZE = MonsterStore.Instance.getMonster(ID).Size;
            Speed = MonsterStore.Instance.getMonster(ID).Speed;

            // read the items drops (see region functions)
            ReadDrops(ID);

            // Local properties
            instanceID = Guid.Parse(guid);
            MonsterID = ID;
            Direction = new Vector2();                                                              // Move direction
            state = EntityState.Spawn;                                                              // Player state
            Borders = new Border(borders.X, borders.Y);                                             // Max Tiles from center
            Active = true;
        }

        #region update
        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                get_server_update();
                update_animation(gameTime);
            }

            if (state != EntityState.Spawn && state != EntityState.Died)
                transperancy = 1;
        }

        public void update_server(Vector2 newPosition, EntityState newState, SpriteEffects newEffect)
        {
            if(Active)
            {
                this.ServerUpdate_state = newState;
                this.ServerUpdate_position = newPosition;
                this.ServerUpdate_spriteEffect = newEffect;
            }
        }

        private void get_server_update()
        {
            if (this.state != ServerUpdate_state || 
                this.spriteEffect != ServerUpdate_spriteEffect)
            {
                this.previousState = this.state;
                this.state = ServerUpdate_state;
                this.position.X = ServerUpdate_position.X;
                this.spriteEffect = ServerUpdate_spriteEffect;
            }
        }

        private void update_animation(GameTime gameTime)
        {
            switch (state)
            {
                #region stand
                case EntityState.Stand:

                    Speed = 0;
                    Direction = Vector2.Zero;

                    // Check if Monster is steady standing
                    if (previousState == EntityState.Frozen)
                        spritename = "stand_" + spriteframe.ToString();

                    // Move the Monster
                    OldPosition = Position;

                    // monster animation
                    spriteOfset = new Vector2(0, 0);
                    spriteFrame.Y = Convert.ToInt32(spriteOfset.Y);

                    // reduce timer
                    previousAnimateTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // set sprite frames
                    if (previousAnimateTimeSec < 0)
                    {
                        previousAnimateTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                        spriteframe++;
                    }

                    if (spriteframe > list_offsets.FindAll(x => x.Name.StartsWith("stand_")).Count - 1)
                        spriteframe = 0;

                    // Player animation
                    if (prevspriteframe != spriteframe)
                    {
                        prevspriteframe = spriteframe;
                        spritename = "stand_" + spriteframe.ToString();
                        spriteOfset = getoffset();
                    }

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region walk
                case EntityState.Walk:

                    Speed = 0;
                    Direction = Vector2.Zero;

                    if (spriteEffect == SpriteEffects.FlipHorizontally)
                    {
                        // walk right
                        this.Direction.X = 1;
                        this.Speed = WALK_SPEED;
                    }
                    else if (spriteEffect == SpriteEffects.None)
                    {
                        // walk left
                        this.Direction.X = -1;
                        this.Speed = WALK_SPEED;
                    }

                    // reduce timer
                    previousAnimateTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // set sprite frames
                    if (previousAnimateTimeSec < 0)
                    {
                        previousAnimateTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                        spriteframe++;
                    }

                    if (spriteframe > list_offsets.FindAll(x => x.Name.StartsWith("move_")).Count - 1)
                        spriteframe = 0;

                    // Player animation
                    if (prevspriteframe != spriteframe)
                    {
                        prevspriteframe = spriteframe;
                        spritename = "move_" + spriteframe.ToString();
                        spriteOfset = getoffset();
                    }

                    // Check if monster is steady standing
                    //if (Position.Y > OldPosition.Y && collideSlope == false)
                    //    state = EntityState.Falling;

                    // Update the Position Monster
                    OldPosition = Position;

                    // Walk speed
                    Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Walking Border for monster
                    if (Position.X <= Borders.Min)
                    {
                        Position = OldPosition;
                        //spriteEffect = SpriteEffects.FlipHorizontally; // replaced by server update
                    }
                    else if (Position.X >= Borders.Max)
                    {
                        Position = OldPosition;
                        //spriteEffect = SpriteEffects.None; // replaced by server update
                    }

                    break;
                #endregion
                #region falling
                case EntityState.Falling:

                    if (OldPosition.Y < position.Y)
                    {
                        // Move the Character
                        OldPosition = Position;

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                        state = EntityState.Stand;

                    break;
                #endregion
                #region hit
                case EntityState.Hit:
                    spriteframe = 0;

                    break;
                #endregion
                #region frozen
                case EntityState.Frozen:

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // monster animation
                    spritename = "hit1_0";
                    spriteOfset = getoffset();
                    spriteframe = 0;

                    break;
                #endregion
                #region died
                case EntityState.Died:

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // monster animation
                    spriteOfset = new Vector2(0, 270);
                    spriteFrame.Y = Convert.ToInt32(spriteOfset.Y);

                    // Monster fades away
                    transperancy -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // reduce timer
                    previousAnimateTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // set sprite frames
                    if (previousAnimateTimeSec < 0)
                    {
                        previousAnimateTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                        spriteframe++;
                    }

                    // Player animation
                    if (spriteframe > list_offsets.FindAll(x => x.Name.StartsWith("die1_")).Count - 1)
                        spriteframe = list_offsets.FindAll(x => x.Name.StartsWith("die1_")).Count - 1;

                    if (prevspriteframe != spriteframe)
                    {
                        prevspriteframe = spriteframe;
                        spritename = "die1_" + spriteframe.ToString();
                        spriteOfset = getoffset();
                    }
                    break;
                #endregion
                #region spawn
                case EntityState.Spawn:

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Monster fadesin
                    if (transperancy < 1)
                        transperancy += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region agressive
                case EntityState.Agressive:

                    Speed = 0;
                    Direction = Vector2.Zero;

                    if (spriteEffect == SpriteEffects.FlipHorizontally)
                    {
                        // walk right
                        this.Direction.X = 1;
                        this.Speed = RUN_SPEED;
                    }
                    else
                    {
                        // walk left
                        this.Direction.X = -1;
                        this.Speed = RUN_SPEED;
                    }

                    // reduce timer
                    previousAnimateTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // set sprite frames
                    if (previousAnimateTimeSec < 0)
                    {
                        previousAnimateTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.05f; // faster animation
                        spriteframe++;
                    }

                    if (spriteframe > list_offsets.FindAll(x => x.Name.StartsWith("move_")).Count - 1)
                        spriteframe = 0;

                    // Player animation
                    if (prevspriteframe != spriteframe)
                    {
                        prevspriteframe = spriteframe;
                        spritename = "move_" + spriteframe.ToString();
                        spriteOfset = getoffset();
                    }

                    // Update the Position Monster
                    OldPosition = Position;

                    // Walk speed
                    Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region attacking
                case EntityState.Attacking:

                    // Move the Character
                    OldPosition = Position;

                    // reduce timer
                    previousAnimateTimeSec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // set sprite frames
                    if (previousAnimateTimeSec < 0)
                    {
                        previousAnimateTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.05f; // faster animation
                        spriteframe++;
                    }

                    if (spriteframe > list_offsets.FindAll(x => x.Name.StartsWith("move_")).Count - 1)
                        spriteframe = 0;

                    // Player animation
                    if (prevspriteframe != spriteframe)
                    {
                        prevspriteframe = spriteframe;
                        spritename = "move_" + spriteframe.ToString();
                        spriteOfset = getoffset();
                    }
                    break;
                #endregion
            }

            this.previousState = this.state;
        }
        #endregion

        #region draw
        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D drawsprite = null;
            Vector2 drawPosition = Vector2.Zero;

            if (Active)
            {
                DrawSpriteFrame(spriteBatch); // <-- debug option

                // load texture into sprite from Content Manager
                drawsprite = Content.Load<Texture2D>(spritepath + spritename);

                // Calculate position based on spriteEffect
                if (spriteEffect == SpriteEffects.None)
                    drawPosition.X = (int)Position.X + (int)getoffset().X + (int)(sprite.Width * 0.55f);
                else
                    drawPosition.X = (int)Position.X + (int)Math.Abs(getoffset().X) - drawsprite.Width + (int)(sprite.Width * 0.55f);

                // draw player sprite
                spriteBatch.Draw(drawsprite,
                    new Rectangle(
                        (int)drawPosition.X, //+ (int)sprCorrect.X,
                        (int)Position.Y + (int)getoffset().Y + (int)(sprite.Height * 0.85f),
                        drawsprite.Width,
                        drawsprite.Height),
                    new Rectangle(
                        0,
                        0,
                        drawsprite.Width,
                        drawsprite.Height),
                    Color.White * this.transperancy, 0f, Vector2.Zero, spriteEffect, 0f);
            }
        }

        private void DrawSpriteFrame(SpriteBatch spriteBatch)
        {
            if (this.debug)
            {
                Texture2D rect = new Texture2D(ResourceManager.GetInstance.gfxdevice, (int)Math.Abs(SpriteFrame.Width), (int)SpriteFrame.Height);

                Color[] data = new Color[(int)Math.Abs(SpriteFrame.Width) * (int)SpriteFrame.Height];
                for (int i = 0; i < data.Length; ++i) data[i] = Color.Blue;
                rect.SetData(data);

                spriteBatch.Draw(rect, SpriteBoundries, SpriteFrame, Color.White * 0.5f, 0, Vector2.Zero, spriteEffect, 0f);
            }
        }
        #endregion

        #region functions
        private struct Border
        {
            // Structure for monster walking bounds
            public float Min, Max;

            public Border(float min, float max)
            {
                Min = min;
                Max = max;
            }
        }

        private void ReadDrops(int ID)
        {
            PropertyInfo propertyMonster;
            int[] itemdrop = new int[] { 0, 1 };
            int index = 0;

            for (int a = 11; a < MonsterStore.Instance.getMonster(ID).GetType().GetProperties().Length; a++)
            {
                propertyMonster = MonsterStore.Instance.getMonster(ID).GetType().GetProperties()[a];

                if (propertyMonster.Name.StartsWith("drop") && propertyMonster.Name.EndsWith("Item"))
                {
                    var value = propertyMonster.GetValue(MonsterStore.Instance.getMonster(ID), null);
                    itemdrop[index] = Convert.ToInt32(value);
                    index++;
                }
                else if (propertyMonster.Name.StartsWith("drop") && propertyMonster.Name.EndsWith("Chance"))
                {
                    itemdrop[index] = Convert.ToInt32(propertyMonster.GetValue(MonsterStore.Instance.getMonster(ID), null));
                    ItemDrop.Add(new int[] { itemdrop[0], itemdrop[1] });
                    index = 0;
                }
            }
        }

        public Rectangle SpriteBoundries
        {
            get
            {
                return new Rectangle(
                            (int)(Position.X + SpriteFrame.Width * 0.20f + MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.X),
                            (int)(Position.Y + SpriteFrame.Height * 0.40f + MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.Y),
                            (int)Math.Abs(SpriteFrame.Width * 0.60f - MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.X),
                            (int)Math.Abs(SpriteFrame.Height * 0.60f - MonsterStore.Instance.monster_list.Find(x => x.monsterID == this.MonsterID).sizeMod.Y));
            }
        }

        public Vector2 getoffset()
        {
            if (this.list_offsets.Count == 0)
                loadoffsetfromXML(); // load from XML

            if (this.list_offsets.FindAll(x => x.Name == spritename + ".png").Count > 0)
            {
                return new Vector2(this.list_offsets.Find(x => x.Name == spritename.ToString() + ".png").X,
                                   this.list_offsets.Find(x => x.Name == spritename.ToString() + ".png").Y);
            }
            else
                return Vector2.Zero; // the sprite simply does not exist (e.g. hands for ladder and rope are disabled)

        }

        public void loadoffsetfromXML()
        {
            List<string> attribute = new List<string>();

            if (spritepath != null)
            {
                using (var reader = new StreamReader(Path.Combine(ResourceManager.GetInstance.Content.RootDirectory + "\\" + spritepath, "data.xml")))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(' ');

                        try
                        {
                            if (values[0] != "<i>")
                            {
                                for (int i = 0; i < values.Length; i++)
                                {
                                    if (values[i].StartsWith("image="))
                                    {
                                        char[] arrstart = new char[] { 'i', 'm', 'a', 'g', 'e', '=' };
                                        string result = values[i].TrimStart(arrstart);
                                        result = Regex.Replace(result, @"""", "");
                                        attribute.Add(result);
                                    }
                                    else if (values[i].StartsWith("x="))
                                    {
                                        char[] arrstart = new char[] { 'x', '=', '"' };
                                        char[] arrend = new char[] { '"' };
                                        string result = values[i].TrimStart(arrstart);
                                        result = result.TrimEnd(arrend);
                                        attribute.Add(result);
                                    }
                                    else if (values[i].StartsWith("y="))
                                    {
                                        char[] arrstart = new char[] { 'y', '=', '"' };
                                        char[] arrend = new char[] { '"', '\\', '"', '/', '>' };
                                        string result = values[i].TrimStart(arrstart);
                                        result = result.TrimEnd(arrend);
                                        attribute.Add(result);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string ee = ex.ToString();
                        }
                    }
                }

                // fill list with XML structures
                this.list_offsets.Clear();

                for (int i = 0; i < attribute.Count; i++)
                {
                    if (attribute[i].EndsWith(".png"))
                    {
                        this.list_offsets.Add(new spriteOffset(
                                0,
                                attribute[i].ToString(),
                                Convert.ToInt32(attribute[i + 1]),
                                Convert.ToInt32(attribute[i + 2])));
                    }
                }
            }
        }
        #endregion
    }
}
