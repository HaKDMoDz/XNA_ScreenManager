﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA_ScreenManager.ScreenClasses.SubComponents;
using Microsoft.Xna.Framework.Content;
using XNA_ScreenManager.MapClasses;
using XNA_ScreenManager.ScreenClasses.MainClasses;

namespace XNA_ScreenManager.ScreenClasses.Menus
{
    public class CharacterSelectionScreen : GameScreen
    {
        SpriteBatch spriteBatch;
        ContentManager Content;

        PlayerClasses.PlayerInfo playerinfo = PlayerClasses.PlayerInfo.Instance;

        KeyboardInput keyboardiput;
        BackgroundComponent bgcomp1, bgcomp2, bgcomp3;
        public MenuComponent menu;
        SpriteFont playerNameFont;

        Texture2D[] slot = new Texture2D[6];
        Texture2D option_board, screen_board;
        PlayerSprite playersprite;

        string[] menuItems = {
            "Select", 
            "Create", 
            "Delete",
            "Continue",
            "Back"};

        public CharacterSelectionScreen(Game game)
            : base(game)
        {
            // select Resource services
            spriteBatch = ResourceManager.GetInstance.spriteBatch;
            Content = ResourceManager.GetInstance.Content;

            // select spritefont
            playerNameFont = Content.Load<SpriteFont>(@"font\Arial_12px");

            // define components
            keyboardiput = new KeyboardInput(game, Content.Load<SpriteFont>(@"font\Arial_12px"));
            bgcomp1 = new BackgroundComponent(game, Content.Load<Texture2D>(@"gfx\background\background03"));
            bgcomp2 = new BackgroundComponent(game, Content.Load<Texture2D>(@"gfx\background\character_selection"));
            bgcomp3 = new BackgroundComponent(game, Content.Load<Texture2D>(@"gfx\background\frame2"));
            menu = new MenuComponent(game, Content.Load<SpriteFont>(@"font\Comic_Sans_18px"));

            // add components
            Components.Add(bgcomp1);
            Components.Add(bgcomp2);
            Components.Add(bgcomp3);
            Components.Add(keyboardiput);

            // menu options
            menu.SetMenuItems(menuItems);
            menu.StartIndex = 0;
            menu.Position = new Vector2(665, 230);
            menu.NormalColor = Color.White;
            menu.HiliteColor = Color.Red;
            menu.MenuItemSpace = 25;
            menu.Rotation[2] = 0.25f;
            menu.Offset[2] = new Vector2(0, -5);
            menu.Offset[3] = new Vector2(-8, 23);
            menu.Offset[4] = new Vector2(-526, -22);
            menu.EndIndex = 3;

            // player sprite
            playersprite = new PlayerSprite(100, 123, new Vector2(32, 32));

            // options
            option_board = Content.Load<Texture2D>(@"gfx\screens\screenobjects\option_board");
            screen_board = Content.Load<Texture2D>(@"gfx\screens\screenobjects\next_board");

            // slots
            for (int i = 0; i < 6; i++)
                slot[i] = Content.Load<Texture2D>(@"gfx\screens\screenobjects\empty_character_slot");
        }
        
        public int SelectedIndex
        {
            get { return menu.SelectedIndex; }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            menu.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 Position = Vector2.Zero;

            // Draw Backgrounds
            base.Draw(gameTime);

            // Draw Player
            playersprite.Draw(spriteBatch);

            Texture2D rect = new Texture2D(ResourceManager.GetInstance.gfxdevice,
                (int)(playerNameFont.MeasureString(playerinfo.Name).X),
                (int)(playerNameFont.MeasureString(playerinfo.Name).Y));

            Color[] data = new Color[(int)(playerNameFont.MeasureString(playerinfo.Name).X) * (int)(playerNameFont.MeasureString(playerinfo.Name).Y)];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Black;
            rect.SetData(data);

            // Draw Player Name
            spriteBatch.Draw(rect, new Vector2(
                playersprite.Position.X + (playersprite.SpriteFrame.Width * 0.5f) - (playerNameFont.MeasureString(playerinfo.Name).X * 0.5f),
                            playersprite.Position.Y + (playersprite.SpriteFrame.Height) + 5), 
                Color.White * 0.5f);

            spriteBatch.DrawString(playerNameFont, playerinfo.Name,
                new Vector2(playersprite.Position.X + (playersprite.SpriteFrame.Width * 0.5f) - (playerNameFont.MeasureString(playerinfo.Name).X * 0.5f),
                            playersprite.Position.Y + (playersprite.SpriteFrame.Height) + 5), 
                Color.White);

            // Draw Slots
            Position = new Vector2(250, 132); // <<-- change this to 100 when more players into game
            
            for (int i = 0; i < 6; i++)
            {
                spriteBatch.Draw(slot[i], Position, Color.White);
                Position.X += 150;

                if (Position.X > 400)
                {
                    Position.X = 100;
                    Position.Y = 290;
                }
            }

            // Draw option board
            spriteBatch.Draw(option_board, new Vector2(620, 205), Color.White);

            // Draw screen boards
            spriteBatch.Draw(screen_board, new Rectangle(100, 372, screen_board.Width, screen_board.Height), 
                new Rectangle(0, 0, screen_board.Width, screen_board.Height), Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(screen_board, new Vector2(650, 372), Color.White);

            // Draw menu items
            menu.Draw(gameTime);

        }
    }
}