using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SystemInformation = System.Windows.Forms.SystemInformation;

namespace Pentacorn
{
    class Input
    {
        public bool LeftMousePressed { get { return Cms.LeftButton == ButtonState.Pressed;  } }
        public bool LeftMouseDown { get { return LeftMousePressed && Pms.LeftButton == ButtonState.Released; } }
        public bool LeftMouseUp { get { return !LeftMousePressed && Pms.LeftButton == ButtonState.Pressed; } }

        public float WheelDelta { get { return (float)(Cms.ScrollWheelValue - Pms.ScrollWheelValue) / SystemInformation.MouseWheelScrollDelta; } }
        public Vector2 MouseDelta { get { return new Vector2(Cms.X - Pms.X, Cms.Y - Pms.Y); } }
        public Vector2 MousePosition { get { return new Vector2(Cms.X, Cms.Y); } }

        public bool KeyPressed(Keys key) { return Cks.IsKeyDown(key); }
        public bool KeyDown(Keys key) { return Cks.IsKeyDown(key) && Pks.IsKeyUp(key); }
        public bool KeyUp(Keys key) { return Cks.IsKeyUp(key) && Pks.IsKeyDown(key); }

        public Input(MouseState cms, MouseState pms, KeyboardState cks, KeyboardState pks)
        {
            Pms = pms;
            Pks = pks;
            Cms = cms;
            Cks = cks;
        }

        public static void SetFocusOn(IntPtr hwnd)
        {
            Mouse.WindowHandle = hwnd;
        }

        internal static IObservable<Input> InitializeGlobal(IntPtr hwnd, IScheduler scheduler)
        {
            Mouse.WindowHandle = hwnd;
            
            PreviousMouseState = Mouse.GetState();
            PreviousKeyboardState = Keyboard.GetState();

            Subject = new Subject<Input>(scheduler);

            return Subject.AsObservable();
        }

        internal static void UpdateGlobal()
        {
            var cms = Mouse.GetState();
            var cks = Keyboard.GetState();

            Subject.OnNext(new Input(cms, PreviousMouseState, cks, PreviousKeyboardState));

            PreviousMouseState = cms;
            PreviousKeyboardState = cks;
        }

        private MouseState Pms, Cms;
        private KeyboardState Pks, Cks;

        private static MouseState PreviousMouseState;        
        private static KeyboardState PreviousKeyboardState;

        private static Subject<Input> Subject;
    }
}
