﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNA_ScreenManager.PlayerClasses;

namespace XNA_ScreenManager.ScreenClasses
{
    public class InGameMainMenuScreen : GameScreen
    {
        ContentManager Content;
        GraphicsDevice gfxdevice;

        PlayerInfo playerInfo = PlayerInfo.Instance;
        MenuComponent menu;

        string[] menuItems = {
            "Items", 
            "Skills", 
            "Equipment",
            "Status",
            "Save",
            "Load",
            "Title Screen"};

        SpriteFont spriteFont, infoFont;
        SpriteBatch spriteBatch; 
        Texture2D HudPicture;

        public InGameMainMenuScreen(Game game,SpriteFont spriteFont, Texture2D background)
            : base(game)
        {
            this.spriteFont = spriteFont;
            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            Content = (ContentManager)Game.Services.GetService(typeof(ContentManager));
            gfxdevice = (GraphicsDevice)Game.Services.GetService(typeof(GraphicsDevice));

            menu = new MenuComponent(game, spriteFont);
            menu.SetMenuItems(menuItems);

            Components.Add(new BackgroundComponent(game, background));
            Components.Add(menu);

            LoadAssets();
        }

        protected void LoadAssets()
        {
            base.LoadContent();
            infoFont = Content.Load<SpriteFont>(@"font\gamefont");
            HudPicture = Content.Load<Texture2D>(@"gfx\player\player_basic");
        }

        public int SelectedIndex
        {
            get { return menu.SelectedIndex; }
        }

        public override void Show()
        {
            menu.Position = new Vector2(50, 100);
            base.Show();
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw the base first
            base.Draw(gameTime);

            drawDescription(gameTime);
            drawPlayerInfo(gameTime);
        }

        private void drawDescription(GameTime gameTime)
        {
            string description = null;

            switch (SelectedIndex)
            {
                case 0:
                    description = "Go to Inventory";
                    break;
                case 1:
                    description = "Go to Skill Overview";
                    break;
                case 2:
                    description = "Go to Equipment";
                    break;
                case 3:
                    description = "Go to Status Overview";
                    break;
                case 4:
                    description = "Save Process";
                    break;
                case 5:
                    description = "Load a new Game";
                    break;
                case 6:
                    description = "Go back to TitleScreen";
                    break;
            }

            // item description
            spriteBatch.DrawString(spriteFont, description, new Vector2(50, 450), Color.Yellow);
        }

        private void drawPlayerInfo(GameTime gameTime)
        {
            spriteBatch.Draw(HudPicture, new Rectangle(300,75,60,60), 
                new Rectangle(20, 0, 60, 60), Color.White, 
                0f, new Vector2(0,0), SpriteEffects.FlipHorizontally, 1);

            spriteBatch.DrawString(infoFont, playerInfo.Name, new Vector2(400, 80), Color.White);
            spriteBatch.DrawString(infoFont, playerInfo.Jobclass, new Vector2(450, 80), Color.Yellow);

            spriteBatch.DrawString(infoFont, "Level", new Vector2(400, 95), Color.White);
            spriteBatch.DrawString(infoFont, playerInfo.Level.ToString(), new Vector2(450, 95), Color.Yellow);

            spriteBatch.DrawString(infoFont, "Next", new Vector2(400, 110), Color.White);
            spriteBatch.DrawString(infoFont, (playerInfo.NextLevelExp - playerInfo.Exp).ToString(), new Vector2(450, 110), Color.Yellow);

            spriteBatch.DrawString(infoFont, "HP", new Vector2(400, 125), Color.White);
            spriteBatch.DrawString(infoFont, playerInfo.Health.ToString() + "/" + playerInfo.MaxHealth.ToString(), new Vector2(450, 125), Color.Yellow);
        }
    }
}