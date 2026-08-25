using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Toolkit;
using SideKicker.PanelSystem;
using SideKicker.Theming;
using System.IO;
using System.Linq;

namespace SideKicker
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;

        private ContextPanelController panelController;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            PopUpUIManager.Initialize(this, _graphics);

            Window.IsBorderless = true;
            Window.Title = "";

            WindowTransparency.MakeBorderless(Window.Handle);
            WindowTransparency.EnableDwmPerPixelAlpha(Window.Handle);
            WindowTransparency.SetTopMost(Window.Handle);
            WindowTransparency.EnableRegionHitTesting(Window.Handle);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ContentAssistant.LoadTexture2Ds(
                Directory.EnumerateFiles(ThemeService.AssetsFolderPath, "*.png", SearchOption.AllDirectories)
                .Where(path => Path.GetExtension(path) != ".pui")
                .Select(path => (path, path))
                .ToArray()
            );

            panelController = new ContextPanelController();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            PopUpUIManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            PopUpUIManager.RenderUpdate(gameTime);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);

            _spriteBatch.Begin();

            PopUpUIManager.BufferDraw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
