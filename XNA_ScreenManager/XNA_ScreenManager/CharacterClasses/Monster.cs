﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using XNA_ScreenManager.CharacterClasses;
using XNA_ScreenManager.MapClasses;
using XNA_ScreenManager.PlayerClasses;
using System.Text;

namespace XNA_ScreenManager.CharacterClasses
{
    public class Monster : Entity
    {
        #region properties

        // static randomizer
        randomizer Randomizer = randomizer.Instance;                                                // generate unique random ID
        PlayerInfo PlayerInfo = PlayerInfo.Instance;                                                // get battle information of player
        GameWorld world;

        // Drawing properties
        private int spriteWidth = 90;
        private int spriteHeight = 90;
        private Vector2 spriteOfset = new Vector2(90, 0);
        private SpriteEffects spriteEffect = SpriteEffects.None;
        private int damage = 0;

        // Sprite Animation Properties
        Color color = Color.White;                                                                  // Sprite color
        private Vector2 Direction = Vector2.Zero;                                                   // Sprite Move direction
        private float Speed;                                                                        // Speed used in functions
        private bool ani_forward = true;                                                            // if we play for or backward
        private bool frozen = false;                                                                // frozen switch during hit

        // Movement properties
        const int WALK_SPEED = 100;                                                                 // The actual speed of the entity
        const int ANIMATION_SPEED = 120;                                                            // Animation speed, 120 = default
        const int IDLE_TIME = 10;                                                                   // idle time until next movement
        Border Borders = new Border(0, 0);                                                          // max tiles to walk from center (avoid falling)

        // Clocks and Timers
        int previousAnimateTimeMsec,                                                                // Animation in Miliseconds
            previousAnimateTimeSec;                                                                 // Animation in Seconds
        int previousWalkTimeSec,                                                                    // WalkTime in Seconds
            previousWalkTimeMin;                                                                    // WalkTime in Minutes
        int previousIdleTimeSec,                                                                    // IdleTime in Seconds
            previousIdleTimeMin;                                                                    // IdleTime in Minutes
        int previousHitTimeMSec,                                                                    // IdleTime in Miliseconds
            previousHitTimeSec;                                                                     // IdleTime in Seconds

        #endregion

        public Monster(Texture2D texture, Vector2 position, Vector2 borders)
            : base()
        {
            // Derived properties
            Active = true;
            sprite = texture;
            SpriteFrame = new Rectangle((int)spriteOfset.X, (int)spriteOfset.Y, spriteWidth, spriteHeight);
            Position = position;
            OldPosition = position;
            entityType = EntityType.Monster;

            // temporary parameters these should eventually be imported from the Monster Database
            HP = 10; MP = 0; ATK = 60; DEF = 30; LVL = 1; HIT = 10; FLEE = 5;

            // Local properties
            Direction = new Vector2();                                                              // Move direction
            state = EntityState.Stand;                                                              // Player state
            Borders = new Border(borders.X, borders.Y);                                             // Max Tiles from center
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                update_movement(gameTime);
                update_animation(gameTime);
            }
        }

        private void update_movement(GameTime gameTime)
        {
            switch (state)
            {
                case EntityState.Stand:

                    if (previousIdleTimeSec <= (int)gameTime.TotalGameTime.Seconds
                        || previousIdleTimeMin != (int)gameTime.TotalGameTime.Minutes)
                    {
                        previousWalkTimeSec = (int)gameTime.TotalGameTime.Seconds + Randomizer.generateRandom(6, 12);
                        previousWalkTimeMin = (int)gameTime.TotalGameTime.Minutes;

                        // temporary random generator
                        if (Randomizer.generateRandom(0, 2) == 1)
                            spriteEffect = SpriteEffects.None;
                        else
                            spriteEffect = SpriteEffects.FlipHorizontally;

                        // reset sprite frame and change state
                        spriteFrame.X = 0;
                        state = EntityState.Walk;
                    }

                    break;

                case EntityState.Walk:

                    if (previousWalkTimeSec <= (int)gameTime.TotalGameTime.Seconds
                        || previousWalkTimeMin != (int)gameTime.TotalGameTime.Minutes)
                    {
                        previousIdleTimeSec = (int)gameTime.TotalGameTime.Seconds + Randomizer.generateRandom(6, 12);
                        previousIdleTimeMin = (int)gameTime.TotalGameTime.Minutes;

                        // reset sprite frame and change state
                        spriteFrame.X = 0;
                        state = EntityState.Stand;
                    }

                    break;
            }
        }

        private void update_animation(GameTime gameTime)
        {
            switch (state)
            {
                #region state stand
                case EntityState.Stand:

                    Speed = 0;
                    Direction = Vector2.Zero;

                    // Check if Monster is steady standing
                    if (Position.Y > OldPosition.Y)
                        state = EntityState.Falling;

                    // Move the Monster
                    OldPosition = Position;

                    // monster animation
                    spriteOfset = new Vector2(0, 0);
                    spriteFrame.Y = Convert.ToInt32(spriteOfset.Y);

                    if (previousAnimateTimeMsec <= (int)gameTime.TotalGameTime.Milliseconds
                        || previousAnimateTimeSec != (int)gameTime.TotalGameTime.Seconds)
                    {
                        previousAnimateTimeMsec = (int)gameTime.TotalGameTime.Milliseconds + ANIMATION_SPEED;
                        previousAnimateTimeSec = (int)gameTime.TotalGameTime.Seconds;

                        if (ani_forward)
                        {
                            spriteFrame.X += spriteWidth;
                            if (spriteFrame.X > spriteOfset.X + (spriteWidth * 2))
                            {
                                spriteFrame.X = (int)spriteOfset.X + (spriteWidth * 2);
                                ani_forward = false;
                            }
                        }
                        else
                        {
                            spriteFrame.X -= spriteWidth;
                            if (spriteFrame.X < spriteOfset.X)
                            {
                                spriteFrame.X = (int)spriteOfset.X;
                                ani_forward = true;
                            }
                        }
                        
                    }

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 200 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    break;
                #endregion
                #region state walk
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

                    // monster animation
                    spriteOfset = new Vector2(0, 90);
                    spriteFrame.Y = Convert.ToInt32(spriteOfset.Y);

                    if (previousAnimateTimeMsec <= (int)gameTime.TotalGameTime.Milliseconds
                        || previousAnimateTimeSec != (int)gameTime.TotalGameTime.Seconds)
                    {
                        previousAnimateTimeMsec = (int)gameTime.TotalGameTime.Milliseconds + ANIMATION_SPEED;
                        previousAnimateTimeSec = (int)gameTime.TotalGameTime.Seconds;

                        if (ani_forward)
                        {
                            spriteFrame.X += spriteWidth;
                            if (spriteFrame.X > spriteOfset.X + (spriteWidth * 3))
                            {
                                spriteFrame.X = (int)spriteOfset.X + (spriteWidth * 3);
                                ani_forward = false;
                            }
                        }
                        else
                        {
                            spriteFrame.X -= spriteWidth;
                            if (spriteFrame.X < spriteOfset.X)
                            {
                                spriteFrame.X = (int)spriteOfset.X;
                                ani_forward = true;
                            }
                        }
                    }

                    // Check if monster is steady standing
                    if (Position.Y > OldPosition.Y && collideSlope == false)
                        state = EntityState.Falling;

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
                        spriteEffect = SpriteEffects.FlipHorizontally;
                    }
                    else if(Position.X >= Borders.Max)
                    {
                        Position = OldPosition;
                        spriteEffect = SpriteEffects.None;
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
                    
                    // Start freeze timer 
                    previousHitTimeMSec = (int)gameTime.TotalGameTime.Milliseconds + 700;
                    previousHitTimeSec = (int)gameTime.TotalGameTime.Seconds;

                    // Check of world instance is created
                    if (world == null)
                        world = GameWorld.GetInstance;

                    // Start damage controll
                    damage = Battle.battle_calc_damage(PlayerInfo, this);

                    // create a damage baloon
                    world.createEffects(damage, new Vector2((this.position.X + this.SpriteFrame.Width * 0.45f) - damage.ToString().Length * 5,
                                                             this.position.Y + this.SpriteFrame.Height * 0.20f));

                    state = EntityState.Frozen;

                    break;
                #endregion
                #region frozen
                case EntityState.Frozen:

                    // Apply Gravity 
                    Position += new Vector2(0, 1) * 250 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // monster animation
                    spriteOfset = new Vector2(0, 180);
                    spriteFrame.X = Convert.ToInt32(spriteOfset.X);
                    spriteFrame.Y = Convert.ToInt32(spriteOfset.Y);

                    if (previousHitTimeMSec <= (int)gameTime.TotalGameTime.Milliseconds
                        || previousHitTimeSec != (int)gameTime.TotalGameTime.Seconds)
                    {
                        // reset sprite frame
                        spriteFrame.X = 0;
                        state = EntityState.Stand;
                    }
                    break;
                #endregion
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.Draw(sprite, new Rectangle((int)Position.X, (int)Position.Y, SpriteFrame.Width, SpriteFrame.Height),
                    SpriteFrame, Color.White, 0f, Vector2.Zero, spriteEffect, 0f);
        }

        private struct Border
        {
            // Structure for monster walking bounds
            public float Min, Max;

            public Border(float min, float max)
            {
                Min = min * 32;
                Max = max * 32;
            }
        }
    }

    // Singleton randomizer to provide unique values
    public class randomizer
    {
        private static randomizer instance;
        private Random rand;

        private randomizer()
        {
            rand = new Random();
        }

        public int generateRandom(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static randomizer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new randomizer();
                }
                return instance;
            }
        }
    }
}