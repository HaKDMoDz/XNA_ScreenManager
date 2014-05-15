﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNA_ScreenManager.CharacterClasses;
using XNA_ScreenManager.ItemClasses;
using XNA_ScreenManager.ScriptClasses;
using XNA_ScreenManager.MapClasses;
using XNA_ScreenManager.PlayerClasses;
using Microsoft.Xna.Framework.Content;
using XNA_ScreenManager.ScreenClasses.MainClasses;
using System.Collections.Generic;
using XNA_ScreenManager.GameWorldClasses.Entities;
using System.Xml;
using System.IO;

namespace XNA_ScreenManager
{
    public class PlayerSprite2 : Entity
    {
        #region properties

        // The Gameworld
        protected GameWorld world;
        protected ResourceManager resourcemanager = ResourceManager.GetInstance;

        // Keyboard- and Mousestate
        protected KeyboardState keyboardStateCurrent, keyboardStatePrevious;

        // Player inventory
        protected Inventory inventory = Inventory.Instance;
        protected ItemStore itemStore = ItemStore.Instance;
        protected Equipment equipment = Equipment.Instance;
        protected ScriptInterpreter scriptManager = ScriptInterpreter.Instance;
        protected PlayerStore playerinfo = PlayerStore.Instance;

        // link to world content manager
        protected ContentManager Content;

        // Player properties
        protected SpriteEffects spriteEffect = SpriteEffects.None;
        private float transperancy = 1;

        // Sprite Animation Properties
        public int effectCounter = 0;                                                               // for the warp effect
        Color color = Color.White;                                                                  // sprite color
        public Vector2 Direction = Vector2.Zero;                                                    // Sprite Move direction
        public float Speed;                                                                         // Speed used in functions
        public Vector2 Velocity = new Vector2(0,1);                                                 // speed used in jump
        private const int PLAYER_SPEED = 200;                                                       // The actual speed of the player
        const int ANIMATION_SPEED = 120;                                                            // Animation speed, 120 = default 
        const int MOVE_UP = -1;                                                                     // player moving directions
        const int MOVE_DOWN = 1;                                                                    // player moving directions
        const int MOVE_LEFT = -1;                                                                   // player moving directions
        const int MOVE_RIGHT = 1;                                                                   // player moving directions
        float previousGameTimeMsec;                                                                 // GameTime in Miliseconds
        private bool landed;                                                                        // land switch, arrow switch

        // new Texture properties
        protected string spritename;
        protected string[] spritepath = new string[] 
        { 
            @"gfx\player\body\head\",
            @"gfx\player\body\torso\", 
            @"gfx\player\faceset\face1\",
            @"gfx\player\hairset\hair1\",
            "",
            "", 
        };
        protected Vector2[] spriteOfset = new Vector2[6];
        protected int spriteframe = 0, prevspriteframe = 0;

        #endregion

        public PlayerSprite2(int _X, int _Y, Vector2 _tileSize)
            : base()
        {
            // Derived properties
            Active = true;
            SpriteSize = new Rectangle(0, 0, 50, 70);
            Position = new Vector2(_X, _Y);
            OldPosition = new Vector2(_X, _Y);
            
            // Local properties
            Direction = new Vector2();                                                              // Move direction
            state = EntityState.Stand;                                                              // Player state

            Content = resourcemanager.Content;
            spriteEffect = SpriteEffects.FlipHorizontally;
        }

        public EntityState SetState { get; set; }

        public override void Update(GameTime gameTime)
        {
            keyboardStateCurrent = Keyboard.GetState();

            // reset effect state
            if (!collideWarp)
            {
                this.color = Color.White;
                this.effectCounter = 0;
            }

            if (Active)
            {
                switch (state)
                {
                    #region state skillactive
                    case EntityState.Skill:

                            // Move the Character
                            OldPosition = Position;
                            // Walk speed
                            Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;

                    #endregion
                    #region state cooldown
                    case EntityState.Cooldown:

                        // reduce timer
                        previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (previousGameTimeMsec < 0)
                        {
                            previousGameTimeMsec = 0;

                            spriteFrame.X = 0;
                            state = EntityState.Stand;
                        }

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;

                    #endregion
                    #region state swinging
                    case EntityState.Swing:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;

                        // Move the Character
                        OldPosition = Position;

                        // reduce timer
                        previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "swingO1_" + spriteframe.ToString();
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }

                        if (previousGameTimeMsec < 0)
                        {
                            previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                            // set sprite frames
                            spriteframe++;

                            if (spriteframe > 2)
                            {
                                spriteframe = 2;
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                                // make sure the world is connected
                                if (world == null)
                                    world = GameWorld.GetInstance;

                                // create swing effect
                                if (spriteEffect == SpriteEffects.FlipHorizontally)
                                    world.newEffect.Add(new WeaponSwing(
                                        new Vector2(this.Position.X + this.SpriteFrame.Width * 1.4f, this.Position.Y + this.SpriteFrame.Height * 0.7f),
                                        WeaponSwingType.Swing01,
                                        spriteEffect));
                                else
                                    world.newEffect.Add(new WeaponSwing(
                                        new Vector2(this.Position.X - this.SpriteFrame.Width * 0.6f, this.Position.Y + this.SpriteFrame.Height * 0.7f),
                                        WeaponSwingType.Swing01,
                                        spriteEffect));

                                // start cooldown
                                state = EntityState.Cooldown;
                            }
                        }

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state stabbing
                    case EntityState.Stab:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;

                        // Move the Character
                        OldPosition = Position;

                        // reduce timer
                        previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "stabO1_" + spriteframe.ToString();
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }

                        if (previousGameTimeMsec < 0)
                        {
                            previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                            // set sprite frames
                            spriteframe++;

                            if (spriteframe > 1)
                            {
                                spriteframe = 1;
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                                // make sure the world is connected
                                if (world == null)
                                    world = GameWorld.GetInstance;

                                // create stab effect
                                if (spriteEffect == SpriteEffects.FlipHorizontally)
                                    world.newEffect.Add(new WeaponSwing(
                                        new Vector2(this.Position.X + this.SpriteFrame.Width * 0.3f, this.Position.Y + this.SpriteFrame.Height * 0.7f),
                                        WeaponSwingType.Stab01,
                                        spriteEffect));
                                else
                                    world.newEffect.Add(new WeaponSwing(
                                        new Vector2(this.Position.X - this.SpriteFrame.Width * 0.7f, this.Position.Y + this.SpriteFrame.Height * 0.7f),
                                        WeaponSwingType.Stab01,
                                        spriteEffect));

                                // reset sprite frame and change state
                                state = EntityState.Cooldown;
                            }
                        }

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state shooting
                    case EntityState.Shoot:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;

                        // Move the Character
                        OldPosition = Position;

                        // reduce timer
                        previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        
                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "shoot1_" + spriteframe.ToString();
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }

                        if (previousGameTimeMsec < 0)
                        {
                            spriteframe++;

                            if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt)
                                || keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D1))
                            {
                                // Later = charge arrow skill
                                if (spriteframe > 1)
                                    spriteframe = 1;
                            }
                            else
                            {
                                if (spriteframe > 2)
                                {
                                    // make sure the world is connected
                                    if (world == null)
                                        world = GameWorld.GetInstance;

                                    // create and release an arrow
                                    if (spriteEffect == SpriteEffects.FlipHorizontally)
                                        world.newEffect.Add(new Arrow(Content.Load<Texture2D>(@"gfx\gameobjects\arrow"),
                                            new Vector2(this.Position.X, this.Position.Y + this.SpriteFrame.Height * 0.6f),
                                            800, new Vector2(1, 0), Vector2.Zero));
                                    else
                                        world.newEffect.Add(new Arrow(Content.Load<Texture2D>(@"gfx\gameobjects\arrow"),
                                            new Vector2(this.Position.X, this.Position.Y + this.SpriteFrame.Height * 0.6f),
                                            800, new Vector2(-1, 0), Vector2.Zero));

                                    // Set the timer for cooldown
                                    previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;

                                    // reset sprite frame and change state
                                    // start cooldown
                                    spriteFrame.X = 0;
                                    state = EntityState.Cooldown;
                                }
                            }
                        }

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state sit
                    case EntityState.Sit:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;
                        
                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
                        {
                            state = EntityState.Stand;
                        }
                        else if (keyboardStateCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Up) &&
                                 keyboardStatePrevious.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                        {
                            state = EntityState.Stand;
                        }
                        else if (keyboardStateCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Insert) &&
                                 keyboardStatePrevious.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Insert))
                        {
                            state = EntityState.Stand;
                        }

                        // Move the Character
                        OldPosition = Position;

                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "sit_0";
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }                        

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state Rope
                    case EntityState.Rope:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;
                        spriteEffect = SpriteEffects.None;

                        // double check collision
                        if (this.collideRope == false)
                            this.state = EntityState.Falling;

                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.Y = MOVE_DOWN;
                            this.Speed = PLAYER_SPEED * 0.75f;

                            // reduce timer
                            previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (previousGameTimeMsec < 0)
                            {
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                                spriteframe++;
                            }

                            // double check frame if previous state has higher X
                            if (spriteframe > 1)
                                spriteframe = 0;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.Y = MOVE_UP;
                            this.Speed = PLAYER_SPEED * 0.75f;

                            // reduce timer
                            previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (previousGameTimeMsec < 0)
                            {
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                                spriteframe++;
                            }

                            // double check frame if previous state has higher X
                            if (spriteframe > 1)
                                spriteframe = 0;
                        }

                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "stand1_" + spriteframe.ToString();
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }

                        // Move the Character
                        OldPosition = Position;

                        // Climb speed
                        Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state Ladder
                    case EntityState.Ladder:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;
                        spriteEffect = SpriteEffects.None;

                        // double check collision
                        if (this.collideLadder == false)
                            this.state = EntityState.Falling;

                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.Y = MOVE_DOWN;
                            this.Speed = PLAYER_SPEED * 0.75f;

                            // reduce timer
                            previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (previousGameTimeMsec < 0)
                            {
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                                spriteframe++;
                            }

                            // double check frame if previous state has higher X
                            if (spriteframe > 1)
                                spriteframe = 0;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.Y = MOVE_UP;
                            this.Speed = PLAYER_SPEED * 0.75f;

                            // reduce timer
                            previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (previousGameTimeMsec < 0)
                            {
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                                spriteframe++;
                            }

                            // double check frame if previous state has higher X
                            if (spriteframe > 1)
                                spriteframe = 0;
                        }

                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "ladder_" + spriteframe.ToString();
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }

                        // Move the Character
                        OldPosition = Position;

                        // Climb speed
                        Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state stand
                    case EntityState.Stand:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;

                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                        {
                            spriteframe = 0;
                            state = EntityState.Walk;
                            spriteEffect = SpriteEffects.FlipHorizontally;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                        {
                            spriteframe = 0;
                            state = EntityState.Walk;
                            spriteEffect = SpriteEffects.None;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
                        {
                            if (!collideNPC)
                            {
                                spriteframe = 0;
                                Velocity += new Vector2(0, -1.6f); // Add an upward impulse
                                state = EntityState.Jump;
                            }
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Insert))
                        {
                            spriteframe = 0;
                            state = EntityState.Sit;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                        {
                            spriteframe = 0;
                            if (this.collideLadder)
                                state = EntityState.Ladder;
                            else if (this.collideRope)
                                state = EntityState.Rope;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
                        {
                            // check if weapon is equiped
                            if (equipment.item_list.FindAll(delegate(Item item) { return item.Type == ItemType.Weapon; }).Count > 0)
                            {
                                WeaponType weapontype = equipment.item_list.Find(delegate(Item item) { return item.Type == ItemType.Weapon; }).WeaponType;

                                // check the weapon type
                                if (weapontype == WeaponType.Bow)
                                {
                                    previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + (float)((350 - playerinfo.activePlayer.ASPD * 12) * 0.0006f) + 0.05f;

                                    spriteframe = 0;
                                    state = EntityState.Shoot;
                                }
                                else if (weapontype == WeaponType.Dagger || weapontype == WeaponType.One_handed_Sword)
                                {
                                    previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + (float)((350 - playerinfo.activePlayer.ASPD * 12) * 0.0006f) + 0.05f;

                                    spriteframe = 0;

                                    if(randomizer.Instance.generateRandom(0,2) == 1)
                                        state = EntityState.Stab;
                                    else
                                        state = EntityState.Swing;
                                }
                            }
                        }

                        // Check if player is steady standing
                        if (Position.Y > OldPosition.Y)
                            state = EntityState.Falling;

                        // Move the Character
                        OldPosition = Position;

                        // reduce timer
                        previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (previousGameTimeMsec < 0)
                        {
                            previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                            spriteframe++;
                            if (spriteframe > 4)
                                spriteframe = 0;
                        }

                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "stand1_" + spriteframe.ToString();
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }

                        // Apply Gravity 
                        Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state walk
                    case EntityState.Walk:

                        Speed = 0;
                        Direction = Vector2.Zero;
                        Velocity = Vector2.Zero;

                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.X = MOVE_RIGHT;
                            this.Speed = PLAYER_SPEED;
                            spriteEffect = SpriteEffects.FlipHorizontally;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.X = MOVE_LEFT;
                            this.Speed = PLAYER_SPEED;
                            spriteEffect = SpriteEffects.None;
                        }
                        else
                        {
                            state = EntityState.Stand;
                        }
                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
                        {
                            if (!collideNPC)
                            {
                                Velocity += new Vector2(0, -1.6f); // Add an upward impulse
                                state = EntityState.Jump;
                            }
                        }

                        // reduce timer
                        previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // set sprite frames
                        if (previousGameTimeMsec < 0)
                        {
                            previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.10f;
                            spriteframe ++;
                        }
                        if (spriteframe > 3)
                            spriteframe = 0;

                        // Player animation
                        if (prevspriteframe != spriteframe)
                        {
                            prevspriteframe = spriteframe;
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "walk1_" + spriteframe.ToString();
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
                        }

                        // Check if player is steady standing
                        if (Position.Y > OldPosition.Y && collideSlope == false)
                            state = EntityState.Falling;
                                                
                        // Move the Character
                        OldPosition = Position;

                        // Walk speed
                        Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        // Apply Gravity 
                        Position += new Vector2(0,1) * 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        break;
                    #endregion
                    #region state jump
                    case EntityState.Jump:

                        Velocity.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        
                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.X += MOVE_LEFT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                            this.Speed = PLAYER_SPEED;

                            if (this.Direction.X < -1)
                                this.Direction.X = -1;
                            else if (this.Direction.X < 0)
                                this.Direction.X = 0;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.X += MOVE_RIGHT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                            this.Speed = PLAYER_SPEED;

                            if (this.Direction.X > 1)
                                this.Direction.X = 1;
                            else if (this.Direction.X > 0)
                                this.Direction.X = 0;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                        {
                            if (this.collideLadder)
                                state = EntityState.Ladder;
                            else if (this.collideRope)
                                state = EntityState.Rope;
                            else
                                state = EntityState.Sit;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                        {
                            if (this.collideLadder)
                                state = EntityState.Ladder;
                            else if (this.collideRope)
                                state = EntityState.Rope;
                        }

                            // Move the Character
                            OldPosition = Position;

                            // Player animation
                            for (int i = 0; i < spritepath.Length; i++)
                            {
                                spritename = "jump_0";
                                spriteOfset[i] = getoffsetfromXML(i);
                            }
        
                            // Apply Gravity + jumping
                            if (Velocity.Y < -1.2f)
                            {
                                // Apply jumping
                                Position += Velocity * 350 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                                // Apply Gravity 
                                Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                                // Walk / Jump speed
                                Position += Direction * (Speed / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                            else
                            {
                                landed = false;
                                state = EntityState.Falling;
                            }

                        break;
                    #endregion
                    #region state falling
                    case EntityState.Falling:

                        if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.X += MOVE_LEFT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                            this.Speed = PLAYER_SPEED;

                            if (this.Direction.X < -1)
                                this.Direction.X = -1;
                            else if (this.Direction.X < 0)
                                this.Direction.X = 0;
                        }
                        else if (keyboardStateCurrent.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                        {
                            // move player location (make ActiveMap tile check here in the future)
                            this.Direction.X += MOVE_RIGHT * 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
                            this.Speed = PLAYER_SPEED;

                            if (this.Direction.X > 1)
                                this.Direction.X = 1;
                            else if (this.Direction.X > 0)
                                this.Direction.X = 0;
                        }

                        if (OldPosition.Y < position.Y)
                        {
                            // Move the Character
                            OldPosition = Position;

                            Velocity.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            // Apply Gravity 
                            Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                            
                            // Walk / Jump speed
                            Position += Direction * (Speed / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        else
                        {
                            // reduce timer
                            previousGameTimeMsec -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (previousGameTimeMsec < 0)
                            {
                                previousGameTimeMsec = (float)gameTime.ElapsedGameTime.TotalSeconds + 0.08f;

                                if (landed == true)
                                    state = EntityState.Stand;
                                else
                                    landed = true;
                            }

                            // Move the Character
                            OldPosition = Position;

                            // Apply Gravity 
                            Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                            // Walk / Jump speed
                            Position += Direction * (Speed / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }

                        // Player animation
                        for (int i = 0; i < spritepath.Length; i++)
                        {
                            spritename = "fly_0";
                            spriteOfset[i] = getoffsetfromXML(i);
                        }

                        break;
                    #endregion
                    #region state hit
                    case EntityState.Hit:

                        // Add an upward impulse
                        Velocity = new Vector2(0, -1.5f);

                        // Add an sideward pulse
                        if (spriteEffect == SpriteEffects.None)
                            Direction = new Vector2(1.6f, 0);
                        else
                            Direction = new Vector2(-1.6f, 0);

                        // Damage controll and balloon is triggered in monster sprite

                        // Move the Character
                        OldPosition = Position;

                        // Player animation
                        for (int i = 0; i < spritepath.Length; i++)
                        {
                            spritename = "fly_0" + spriteframe.ToString();
                            spriteOfset[i] = getoffsetfromXML(i);
                        }

                        // Set new state
                        state = EntityState.Frozen;

                        break;
                    #endregion
                    #region state recover hit
                    case EntityState.Frozen:

                        // Upward Position
                        Velocity.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * 2;

                        // Make player transperant
                        if (transperancy >= 0 )
                            this.transperancy -= (float)gameTime.ElapsedGameTime.TotalSeconds * 10;

                        // turn red
                        this.color = Color.Red;

                        // Move the Character
                        OldPosition = Position;

                        // Player animation
                        for (int i = 0; i < spritepath.Length; i++)
                        {
                            spritename = "fly_0" + spriteframe.ToString();
                            spriteOfset[i] = getoffsetfromXML(i);
                        }

                        // Apply Gravity + jumping
                        if (Velocity.Y < -1.2f)
                        {
                            // Apply jumping
                            Position += Velocity * 350 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                            // Apply Gravity 
                            Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                            // Walk / Jump speed
                            Position += Direction * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        else
                        {
                            landed = false;
                            state = EntityState.Falling;
                            Direction = Vector2.Zero;
                            Velocity = Vector2.Zero;
                            //this.color = Color.White;
                            this.transperancy = 1;
                        }

                        break;
                    #endregion
                }
            }

            #region temporary quickbuttons
            // temporary global function buttons 
            // should be handles by singleton class keyboard manager
            if (keyboardStateCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F1) == true &&
                     keyboardStatePrevious.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F1) == true)
            {
                inventory.addItem(itemStore.getItem(new Random().Next(1100, 1114)));
            } 
            else if (keyboardStateCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F2) == true &&
                      keyboardStatePrevious.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2) == true)
            {
                inventory.addItem(itemStore.getItem(randomizer.Instance.generateRandom(2300,2304)));
            }
            else if (keyboardStateCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F3) == true &&
                      keyboardStatePrevious.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F3) == true)
            {
                inventory.addItem(itemStore.getItem(1300));
            }
            else if (keyboardStateCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F4) == true &&
                     keyboardStatePrevious.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F4) == true)
            {
                inventory.saveItem("inventory.bin");
            }
            else if (keyboardStateCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F5) == true &&
                     keyboardStatePrevious.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F5) == true)
            {
                inventory.loadItems("inventory.bin");
            }
            // temporary
            #endregion

            keyboardStatePrevious = keyboardStateCurrent;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                for (int i = 0; i < spritepath.Length; i++)
                {
                    Color drawcolor = Color.White;
                    Texture2D drawsprite = null;
                    int calculatedPosition = 0;

                    try
                    {
                        if (getspritepath(i) != null)
                        {
                            drawsprite = Content.Load<Texture2D>(getspritepath(i) + spritename);

                            if (spriteEffect == SpriteEffects.None)
                                calculatedPosition = (int)Position.X + (int)spriteOfset[i].X + 35;
                            else
                                calculatedPosition = (int)Position.X + (int)Math.Abs(spriteOfset[i].X) - drawsprite.Width + 25;

                            if (i == 0 || i == 1)
                            {
                                if (this.Player == null)
                                    drawcolor = PlayerStore.Instance.activePlayer.skin_color;
                                else
                                    drawcolor = this.Player.skin_color;
                            }

                            spriteBatch.Draw(drawsprite,
                                new Rectangle(calculatedPosition, (int)Position.Y + (int)spriteOfset[i].Y + 78,
                                    drawsprite.Width, drawsprite.Height),
                                new Rectangle(0, 0, drawsprite.Width, drawsprite.Height),
                                drawcolor * this.transperancy, 0f, Vector2.Zero, spriteEffect, 0f);
                        }
                    }
                    catch
                    {
                        // no texture found
                    }
                }
            }
        }

        public override Rectangle SpriteFrame
        {
            get 
            {
                return new Rectangle((int)Position.X, (int)Position.Y, 60, 80);
            }
        }

        public bool Effect(GameTime gameTime)
        {
            // press up will instant warp
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                this.effectCounter = 0;
                this.color = Color.White;
                return true;
            }
            return false;
        }

        public PlayerInfo Player { get; set; }
                
        public bool CheckKey(Microsoft.Xna.Framework.Input.Keys theKey)
        {
            KeyboardState keyboardStateCurrent = Keyboard.GetState();
            return keyboardStatePrevious.IsKeyDown(theKey) && keyboardStateCurrent.IsKeyUp(theKey);
        }

        public Vector2 getoffsetfromXML(int spriteID)
        {
            List<string> attribute = new List<string>();

            if (getspritepath(spriteID) != null)
            {
                using (var reader = new StreamReader(Path.Combine(Content.RootDirectory + "\\" + getspritepath(spriteID), "data.xml")))
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
                                        char[] arrstart = new char[] { 'i', 'm', 'a', 'g', 'e', '=', '"' };
                                        char[] arrend = new char[] { '"' };
                                        string result = values[i].TrimStart(arrstart);
                                        result = result.TrimEnd(arrend);
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

                for (int i = 0; i < attribute.Count; i++)
                {
                    if (attribute[i].ToString() == spritename.ToString() + ".png")
                        return new Vector2(Convert.ToInt32(attribute[i + 1]),
                                           Convert.ToInt32(attribute[i + 2]));
                }
            }

            return Vector2.Zero;
        }

        public string getspritepath(int spriteID)
        {
            PlayerInfo player = null;

            if (this.Player == null)
                player = PlayerStore.Instance.activePlayer;
            else
                player = this.Player;

            switch (spriteID)
            {
                case 0:
                    return player.head_sprite;
                case 1:
                    return player.body_sprite;
                case 2:
                    return player.faceset_sprite;
                case 3:
                    return player.hair_sprite;
                case 4:
                    return player.costume_sprite;
                case 5:
                    return player.weapon_sprite;
            }
            return null;
        }
    }
}
