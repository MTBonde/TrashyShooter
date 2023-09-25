using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerEngine
{
   
    public delegate void ClickEvent();

    /// <summary>
    /// a UI button that can call event by being clicked
    /// Niels
    /// </summary>
    public class UIButton : Component
    {

        public event ClickEvent OnClick, OnHover;
        private Texture2D _texture;

        private Vector2 scale = new Vector2(0.25f, 0.25f);

        public Vector2 Scale 
        { 
            get 
            { 
                return scale; 
            } 
            set 
            { 
                scale = value;
                origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
                buttonSize = origin * scale;
            } 
        }
        private Vector2 origin;
        private Vector2 buttonSize;
        bool hover;
        Color color = Color.White;

        UIButton()
        {
            SetTexture("button");
            InputManager.buttons.Add(this);
        }

        public void SetTexture(string textureName)
        {
            _texture = GameWorld.Instance.Content.Load<Texture2D>(textureName);
            origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
            buttonSize = origin * scale;
        }

        public bool CheckMouse(bool press)
        {
            if (enabled)
            {
                if(transform.Position.X + buttonSize.X > Mouse.GetState().X &&
                    transform.Position.X - buttonSize.X < Mouse.GetState().X &&
                    transform.Position.Y + buttonSize.Y > Mouse.GetState().Y &&
                    transform.Position.Y - buttonSize.Y < Mouse.GetState().Y)
                {
                    if (!hover)
                    {
                        hover = true;
                        color = Color.Gray;
                    }
                    if(OnHover != null)
                        OnHover.Invoke();
                    if(press)
                    {
                        if (OnClick != null)
                            OnClick.Invoke();
                        return true;
                    }
                }
                else if(hover)
                {
                    hover = false;
                    color = Color.White;
                }
            }
            return false;
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, transform.Position,null, color,0,origin,scale,SpriteEffects.None,0.9f);
        }
    }
}
