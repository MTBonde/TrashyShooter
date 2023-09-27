using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerEngine
{
    public enum TextAlignment
    {
        Left,
        Rigt,
        Center
    }

    /// <summary>
    /// used to render text in ui space
    /// Niels
    /// </summary>
    public class TextRenderer : Component
    {

        public SpriteFont font;
        public string text = "Text";
        public Color color = Color.Black;
        public float scale = 1;
        Vector2 origin;
        public enum TextPivots
        {
            TopCenter,
            MidCenter,
            ButtomCenter,
            TopLeft
        }
        TextPivots textPivot = TextPivots.MidCenter;
        public TextPivots TextPivot 
        { 
            get 
            { 
                return textPivot; 
            } 
            set 
            { 
                textPivot = value;
                FindOrigin();
            } 
        }


        TextRenderer()
        {
            SetFont("UIFont");
        }

        public void SetText(string text)
        {
            this.text = text;
            FindOrigin();
        }        

        public void SetFont(string fontName)
        {
            font = GameWorld.Instance.Content.Load<SpriteFont>(fontName);
        }

        // Metode til at ændre tekststørrelse
        public void SetTextSize(float newSize)
        {
            // Ændrer skalefaktoren for tekst
            scale = newSize;
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font,text,transform.Position,color,0,origin,scale/10, SpriteEffects.None,1);
        }

        private void FindOrigin()
        {
            switch (textPivot)
            {
                case TextPivots.TopCenter:
                    origin.X = font.MeasureString(text).X / 2;
                    break;
                case TextPivots.MidCenter:
                    origin = font.MeasureString(text) / 2;
                    break;
                case TextPivots.ButtomCenter:
                    origin.X = font.MeasureString(text).X / 2;
                    origin.Y = font.MeasureString(text).Y;
                    break;
                case TextPivots.TopLeft:

                    break;
            }
        }
    }
}