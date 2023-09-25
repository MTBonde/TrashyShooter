// Global Usings
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Diagnostics;

global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Audio;
global using Microsoft.Xna.Framework.Content;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
global using Microsoft.Xna.Framework.Media;

namespace MultiplayerEngine
{
    /// <summary>    ///
    /// Contains a collection of global accesibel fields, properties and methods.
    /// thor
    /// </summary>
    public static class Globals
    {
        // FIELDS
        #region Fields        

        public static Random Rnd = new Random();

        public static Vector2 ScreenSize = new Vector2(GameWorld.Instance.Graphics.PreferredBackBufferWidth,
                                                       GameWorld.Instance.Graphics.PreferredBackBufferHeight);
        public static Vector2 ScreenCenter = new Vector2(GameWorld.Instance.GraphicsDevice.Viewport.Width / 2,
                                                         GameWorld.Instance.GraphicsDevice.Viewport.Height / 2);
        public static Vector2 ScreenCenterWidth = new Vector2(GameWorld.Instance.GraphicsDevice.Viewport.Width / 2);
        public static Vector2 ScreenCenterHeight = new Vector2(GameWorld.Instance.GraphicsDevice.Viewport.Height / 2);


        #endregion Fields

        // PROPERTIES
        #region Properties
        public static float DeltaTime { get; set; }
        public static bool DebugModeToggled { get; set; } // Debug
        
        public static SpriteFont DebugFont { get; set; }
        public static GameTime GameTime { get; set; }
        #endregion Properties

        // METHODS
        #region Methods
        public static void Update(GameTime gameTime)
        {
            // DeltaTime Calculation
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            GameTime = gameTime;

          
        }
        #endregion Methods
    }
}
