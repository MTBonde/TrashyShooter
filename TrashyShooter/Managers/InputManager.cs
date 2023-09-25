namespace MultiplayerEngine
{
    public static class InputManager
    {

        public static List<UIButton> buttons = new List<UIButton>();
        static bool mouseDown;

        public static void Update()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (Mouse.GetState().LeftButton.HasFlag(ButtonState.Pressed) && !mouseDown)
                {
                    if (buttons[i].CheckMouse(true))
                    {
                        mouseDown = true;
                    }
                }
                else
                {
                    buttons[i].CheckMouse(false);
                }
            }
            if (Mouse.GetState().LeftButton.HasFlag(ButtonState.Pressed) && !mouseDown)
            {
                mouseDown = true;
            }
            else if (!Mouse.GetState().LeftButton.HasFlag(ButtonState.Pressed) && mouseDown)
            {
                mouseDown = false;
            }
        }

        public static void Reset()
        {
            buttons.Clear();
        }
    }
}
