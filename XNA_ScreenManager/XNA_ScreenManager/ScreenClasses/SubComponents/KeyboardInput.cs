﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace XNA_ScreenManager.ScreenClasses.SubComponents
{
    public class KeyboardInput : DrawableGameComponent
    {
        SpriteBatch spriteBatch = null;
        SpriteFont spriteFont;
        ContentManager Content;
        GraphicsDevice graphics;

        Texture2D nametag;

        Keys[] keys;
        bool[] IskeyUp;
        string[] SC = { ")", "!", "@", "#", "$", "%", "^", "&", "*", "(" };//special characters
        string result = "";
        public bool Active = false;
        float transperancy = 1,
              previousTimeSec = 0;

        public KeyboardInput(Game game, SpriteFont spriteFont)
            : base(game)
        {
            // base variables for gfx
            this.spriteFont = spriteFont;
            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            Content = (ContentManager)Game.Services.GetService(typeof(ContentManager));
            graphics = (GraphicsDevice)Game.Services.GetService(typeof(GraphicsDevice)); 

            nametag = Content.Load<Texture2D>(@"gfx\background\nametag");

            // new keyboard variables
            keys = new Keys[38];
            Keys[] tempkeys;
            tempkeys = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray<Keys>();
            int j = 0;
            for (int i = 0; i < tempkeys.Length; i++)
            {
                if ((i == 1 || i == 11) || (i > 26 && i < 63))//get the keys listed above as well as A-Z
                {
                    keys[j] = tempkeys[i];//fill our key array
                    j++;
                }
            }
            IskeyUp = new bool[keys.Length]; //boolean for each key to make the user have to release the key before adding to the string
            for (int i = 0; i < keys.Length; i++)
                IskeyUp[i] = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                KeyboardState state = Keyboard.GetState();
                int i = 0;
                foreach (Keys key in keys)
                {
                    if (state.IsKeyDown(key))
                    {
                        if (IskeyUp[i])
                        {
                            if (key == Keys.Back && result != "") result = result.Remove(result.Length - 1);
                            if (GetLengthPxt() <= 140)
                            {
                                if (key == Keys.Space) result += " ";
                                if (i > 1 && i < 12)
                                {
                                    if (state.IsKeyDown(Keys.RightShift) || state.IsKeyDown(Keys.LeftShift))
                                        result += SC[i - 2];//if shift is down, and a number is pressed, using the special key
                                    else result += key.ToString()[1];
                                }
                                if (i > 11 && i < 38)
                                {
                                    if (state.IsKeyDown(Keys.RightShift) || state.IsKeyDown(Keys.LeftShift))
                                        result += key.ToString();
                                    else result += key.ToString().ToLower(); //return the lowercase char is shift is up.
                                }
                            }
                        }
                        IskeyUp[i] = false; //make sure we know the key is pressed
                    }
                    else if (state.IsKeyUp(key)) IskeyUp[i] = true;
                    i++;
                }

                UpdatePointer(gameTime);
            }
        }

        private float GetLengthPxt()
        {
            float width = 0;

            foreach(char i in result)
                width += spriteFont.MeasureString(i.ToString()).X + 1f;

            width -= 1.5f; // small correction

            return width;
        }

        private void UpdatePointer(GameTime gameTime)
        {
            previousTimeSec -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (previousTimeSec <= 0)
            {
                previousTimeSec = (float)gameTime.ElapsedGameTime.TotalMilliseconds + 600;

                if (transperancy == 1)
                    transperancy = 0;
                else
                    transperancy = 1;
            }
        }

        public void Complete()
        {
            this.Active = false;
            PlayerClasses.PlayerInfo.Instance.Name = result;
        }

        public void Activate(string import)
        {
            this.Active = true;
            result = import;
        }

        public override void Draw(GameTime gameTime)
        {
            if (Active)
            {                 
                // Draw the NameTag
                Vector2 position = new Vector2(280, 30);
                spriteBatch.Draw(nametag, position, Color.White);

                // Draw result
                position = new Vector2(320, 135);
                spriteBatch.DrawString(spriteFont,
                    result,
                    position,
                    Color.DarkRed);

                // Draw Pointer
                position = new Vector2(320 + GetLengthPxt(), 135);

                spriteBatch.DrawString(spriteFont,
                    "|",
                    position,
                    Color.DarkRed * transperancy);
            }
        }

    }
}