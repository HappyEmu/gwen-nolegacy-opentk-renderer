using Gwen.Control;
using OpenToolkit.Graphics.ES20;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Gwen.Sample.OpenTK
{
    /// <summary>
    /// Demonstrates the GameWindow class.
    /// </summary>
    public class SimpleWindow : GameWindow
    {
        private Gwen.Input.OpenTK input;
        private Gwen.Renderer.OpenTK renderer;
        private Gwen.Skin.Base skin;
        private Gwen.Control.Canvas canvas;
        private UnitTest.UnitTest test;
        private const int fps_frames = 50;
        private readonly List<long> ftime;
        private readonly Stopwatch stopwatch;
        private long lastTime;
        private bool altDown = false;

        public SimpleWindow()
            : base(new GameWindowSettings(), new NativeWindowSettings() { Size = new Vector2i(1024, 768), Title = "gwen OpenTK Renderer" })
        {
            ftime = new List<long>(fps_frames);
            stopwatch = new Stopwatch();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenToolkit.Windowing.Common.Input.Key.Escape)
            {
                Close();
            }
            else if (e.Key == OpenToolkit.Windowing.Common.Input.Key.AltLeft)
            {
                altDown = true;
            }
            else if (altDown && e.Key == OpenToolkit.Windowing.Common.Input.Key.Enter)
            {
                if (WindowState == WindowState.Fullscreen)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowState = WindowState.Fullscreen;
                }
            }

            input.ProcessKeyDown(e);

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            altDown = false;

            input.ProcessKeyUp(e);

            base.OnKeyUp(e);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            input.KeyPress(e);
            base.OnTextInput(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            input.ProcessMouseMessage(e);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            input.ProcessMouseMessage(e);
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            input.ProcessMouseMessage(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            input.ProcessMouseMessage(e);
            base.OnMouseWheel(e);
        }

        protected override void OnLoad()
        {
            GL.ClearColor(Color.MidnightBlue);

            renderer = new Gwen.Renderer.OpenTK(this);
            skin = new Gwen.Skin.TexturedBase(renderer, "DefaultSkin.png")
            {
                DefaultFont = new Font(renderer, "Arial", 10)
            };
            canvas = new Canvas(skin);

            input = new Input.OpenTK();
            input.Initialize(canvas);

            canvas.SetSize(1024, 768);
            canvas.ShouldDrawBackground = true;
            canvas.BackgroundColor = Color.FromArgb(255, 150, 170, 170);
            //canvas.KeyboardInputEnabled = true;

            test = new UnitTest.UnitTest(canvas);

            stopwatch.Restart();
            lastTime = 0;
        }

        protected override void Dispose(bool disposing)
        {
            canvas.Dispose();
            skin.Dispose();
            renderer.Dispose();

            base.Dispose(disposing);
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            renderer.Resize(e.Width, e.Height);
            canvas.SetSize(e.Width, e.Height);

            base.OnResize(e);
        }

        /// <summary>
        /// Add your game logic here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            totalTime += (float)e.Time;
            if (ftime.Count == fps_frames)
            {
                ftime.RemoveAt(0);
            }

            ftime.Add(stopwatch.ElapsedMilliseconds - lastTime);
            lastTime = stopwatch.ElapsedMilliseconds;


            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                //Debug.WriteLine (String.Format ("String Cache size: {0} Draw Calls: {1} Vertex Count: {2}", renderer.TextCacheSize, renderer.DrawCallCount, renderer.VertexCount));
                test.Note = string.Format("String Cache size: {0} Draw Calls: {1} Vertex Count: {2}", renderer.TextCacheSize, renderer.DrawCallCount, renderer.VertexCount);
                test.Fps = 1000f * ftime.Count / ftime.Sum();

                float ft = 1000 * (float)e.Time;

                stopwatch.Restart();

                if (renderer.TextCacheSize > 1000) // each cached string is an allocated texture, flush the cache once in a while in your real project
                {
                    renderer.FlushTextCache();
                }
            }
        }

        /// <summary>
        /// Add your game rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        private float totalTime = 0f;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            canvas.RenderCanvas();

            SwapBuffers();
        }

        /// <summary>
        /// Entry point of this example.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using var example = new SimpleWindow
            {
                Title = "Gwen-DotNet OpenTK test",
                RenderFrequency = 0.0,
                VSync = VSyncMode.Off // to measure performance
            };
            example.Run();
        }
    }
}
