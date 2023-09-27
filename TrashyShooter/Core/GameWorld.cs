using MessagePack;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharedData;
using System;
using System.Net.Sockets;

namespace MultiplayerEngine
{
    public class GameWorld : Game
    {

        // PROPERTIES
        public GraphicsDeviceManager Graphics { get; private set; }

        // GameWorld Singleton
        private static GameWorld _instance;
        public static GameWorld Instance => _instance ??= new();

        public SpriteBatch SpriteBatch { get; set; }

        public NetworkManager.UDPGameClient gameClient;

        private GameWorld()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();
            this.IsFixedTimeStep = false;//false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 120d); //60);

            Content.RootDirectory = "Content";
        }

        #region Methods
        protected override void Initialize()
        {
            base.Initialize();
            gameClient = new NetworkManager.UDPGameClient();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            
            CameraManager.Setup();
            AudioManager.LoadMusic();
            SceneManager.SetupScene();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            
            SceneManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            
            SceneManager.Draw(SpriteBatch);

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            //Disconnects to server if in a game
            if(SceneManager.active_scene.GetType() == typeof(TestScene))
            {
                ClientHasLeft leaveMessage = new ClientHasLeft();
                gameClient.SendDataToServer(leaveMessage);
            }
            base.OnExiting(sender, args);
        }
        #endregion
    }
}