using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pentacorn.Vision
{
    class Input
    {
        public static Input Global = new Input();

        public void Update(IntPtr hwnd)
        {
            Mouse.WindowHandle = hwnd;

            pm = cm;
            cm = Mouse.GetState();

            pk = ck;
            ck = Keyboard.GetState();
        }

        public Vector2 MousePosition { get { return new Vector2(cm.X, cm.Y); } }
        public Vector2 MousePositionDelta { get { return new Vector2(cm.X - pm.X, cm.Y - pm.Y); } }
        public float MouseScrollDelta { get { return (float)(cm.ScrollWheelValue - pm.ScrollWheelValue); } }

        public bool LeftButton { get { return cm.LeftButton == ButtonState.Pressed; } }
        public bool RightButton { get { return cm.RightButton == ButtonState.Pressed; } }
        public bool MiddleButton { get { return cm.MiddleButton == ButtonState.Pressed; } }
        public bool LeftButtonClicked { get { return cm.LeftButton == ButtonState.Pressed && pm.LeftButton == ButtonState.Released; } }
        public bool RightButtonClicked { get { return cm.RightButton == ButtonState.Pressed && pm.RightButton == ButtonState.Released; } }
        public bool MiddleButtonClicked { get { return cm.MiddleButton == ButtonState.Pressed && pm.MiddleButton == ButtonState.Released; } }

        private MouseState pm = Mouse.GetState();
        private MouseState cm = Mouse.GetState();

        private KeyboardState ck = Keyboard.GetState();
        private KeyboardState pk = Keyboard.GetState();

        public bool Pressed(Keys key)
        {
            return pk.IsKeyUp(key) && ck.IsKeyDown(key);
        }
    }
}