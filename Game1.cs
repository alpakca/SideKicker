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
            this.IsFixedTimeStep = false;

            Window.Position = Point.Zero;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.ApplyChanges();

            OverlayManager.Initialize(Window.Handle);
            OverlayManager.SetOpenCloseHotKey(0x0001 | 0x0002 | 0x4000, 0x4B); // install the VM_Keys and MOD_modifiers here
            // OverlayManager.HotKeyTriggered +=

            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OverlayManager.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ContentAssistant.LoadTexture2Ds(
                Directory.EnumerateFiles(Program.AssetsFolderPath, "*.png", SearchOption.AllDirectories)
                .Where(path => Path.GetExtension(path) != ".pui")
                .Select(path => (path, path))
                .ToArray()
            );

            ContentAssistant.LoadWithContentManager<SpriteFont>(
                Directory.EnumerateFiles(Path.Combine(Program.AssetsFolderPath, "fonts"))
                .Select(path =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    return (fileName, "Font_" + fileName);
                }).ToArray()
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
