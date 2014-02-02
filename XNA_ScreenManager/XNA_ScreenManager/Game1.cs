using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using XNA_ScreenManager.ScreenClasses;
using XNA_ScreenManager.ItemClasses;
using XNA_ScreenManager.MapClasses;

namespace XNA_ScreenManager
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // screens are drawble game objects
        StartScreen startScreen;
        HelpScreen helpScreen;         ActionScreen actionScreen;
        CreatePCScreen createPCScreen;
        InGameMainMenuScreen ingameMenuScreen;
        ItemMenuScreen itemMenuScreen;
        MessagePopup MessagePopupScreen;
        LoadingScreen loadingScreen;

        SpriteFont normalFont;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            ScreenManager.Instance.game = this;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            Services.AddService(typeof(ContentManager), Content);
            Services.AddService(typeof(GraphicsDeviceManager), graphics);
            Services.AddService(typeof(GraphicsDevice), GraphicsDevice);

            normalFont = Content.Load<SpriteFont>(@"font\normalfont");

            // create screens
            ingameMenuScreen = new InGameMainMenuScreen(this, normalFont, Content.Load<Texture2D>(@"gfx\screens\game_menu2"));
            ScreenManager.Instance.ingameMenuScreen = ingameMenuScreen;
            Components.Add(ingameMenuScreen);

            createPCScreen = new CreatePCScreen(this, normalFont, Content.Load<Texture2D>(@"gfx\screens\system_menu2"));
            ScreenManager.Instance.createPCScreen = createPCScreen;
            Components.Add(createPCScreen);

            startScreen = new StartScreen(this, normalFont, Content.Load<Texture2D>(@"gfx\screens\title_menu2"));
            ScreenManager.Instance.startScreen = startScreen;
            Components.Add(startScreen);

            helpScreen = new HelpScreen(this, Content.Load<Texture2D>(@"gfx\screens\system_menu2"));
            ScreenManager.Instance.helpScreen = helpScreen;
            Components.Add(helpScreen);

            actionScreen = new ActionScreen(this, normalFont);
            ScreenManager.Instance.actionScreen = actionScreen;
            Components.Add(actionScreen);

            itemMenuScreen = new ItemMenuScreen(this, normalFont, Content.Load<Texture2D>(@"gfx\screens\item_menu2"));
            ScreenManager.Instance.itemMenuScreen = itemMenuScreen;
            Components.Add(itemMenuScreen);

            loadingScreen = new LoadingScreen(this);
            ScreenManager.Instance.loadingScreen = loadingScreen;
            Components.Add(loadingScreen);

            MessagePopupScreen = new MessagePopup(this, normalFont);
            ScreenManager.Instance.MessagePopupScreen = MessagePopupScreen;
            Components.Add(MessagePopupScreen);

            // Create Screen Manager
            ScreenManager.Instance.StartManager();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ScreenManager.Instance.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null);

            base.Draw(gameTime);

            spriteBatch.End();
        }
    }
}
