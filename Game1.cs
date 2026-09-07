using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Toolkit;
using SideKicker.Native;
using SideKicker.PanelSystem;
using System;
using System.IO;
using System.Linq;

namespace SideKicker
{
    public class Game1 : Game
    {
        private static readonly TimeSpan OpenTickRate = TimeSpan.FromSeconds(1 / 60d);
        private static readonly TimeSpan ClosedTickRate = TimeSpan.FromSeconds(1 / 20d);
        
        private readonly GraphicsDeviceManager _graphics;

        private ContextPanelController panelController;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            TargetElapsedTime = ClosedTickRate;
        }

        protected override void Initialize()
        {
            PopUpUIManager.Initialize(this, _graphics);

            Window.IsBorderless = true;
            Window.Title = "";

            Window.Position = Point.Zero;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.ApplyChanges();

            GlobalHitRegionManager.Initialize(Window.Handle);

            GlobalDoubleTabWatcher.Initialize();
            GlobalDoubleTabWatcher.DoubleTapDetected += GlobalDoubleTabWatcher_DoubleTapDetected;

            HoverWindowOpener.OriginUpdated(new Point(1920, 920)); // replace when preference is open
            HoverWindowOpener.OpenControllerWindow += () => panelController.SetWindowState(isOpen: true, openedByHover: true);
            HoverWindowOpener.CloseControllerWindow += () => panelController.SetWindowState(isOpen: false);

            base.Initialize();
        }

        private void GlobalDoubleTabWatcher_DoubleTapDetected()
        {
            panelController.ToggleWindow();
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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            HoverWindowOpener.Update(gameTime.ElapsedGameTime.Milliseconds, panelController);
            GlobalDoubleTabWatcher.Update();

            if (panelController.IsOpen || panelController.IsMoving)
            {
                TargetElapsedTime = OpenTickRate;
                PopUpUIManager.Update(gameTime);
            }
            else
            {
                TargetElapsedTime = ClosedTickRate;
                SuppressDraw();
            }

            panelController.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // suppressed already - just check in case
            if (!panelController.IsOpen && !panelController.IsMoving)
            {
                return;
            }

            PopUpUIManager.RenderUpdate(gameTime);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);

            _spriteBatch.Begin();

            PopUpUIManager.BufferDraw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            panelController.OnExiting();
            GlobalDoubleTabWatcher.Dispose();
            base.OnExiting(sender, args);
        }
    }
}
