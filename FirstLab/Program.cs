// Вариант 5. Построение кубической кривой Безье

// Постройте кубическую кривую Безье, используя четыре контрольные точки.
// Контрольные точки должны быть отображены на экране и иметь возможность перемещения.
// Кривая должна обновляться в реальном времени при изменении положения любой контрольной точки.
// Дополнительно: Реализуйте анимацию, где кривая плавно изменяет свою форму в зависимости от времени.

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace BezierCurveExample
{
    public class BezierCurveWindow : GameWindow
    {
        private Vector2[] controlPoints;
        private const int NumCurvePoints = 100;

        public BezierCurveWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            controlPoints = new Vector2[4]
            {
                new Vector2(-0.5f, -0.5f),
                new Vector2(-0.2f, 0.8f),
                new Vector2(0.2f, -0.8f),
                new Vector2(0.5f, 0.5f)
            };
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
                Close();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            DrawControlPoints();
            DrawBezierCurve();

            SwapBuffers();
        }

        private void DrawControlPoints()
        {
            GL.PointSize(10f);
            GL.Begin(PrimitiveType.Points);
            GL.Color3(1.0f, 0.0f, 1.0f);
            
            foreach (var point in controlPoints)
            {
                GL.Vertex2(point);
            }
            
            GL.End();
        }

        private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }

        private void DrawBezierCurve()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(0.0f, 1.0f, 0.0f);
            
            for (int i = 0; i < NumCurvePoints; i++)
            {
                float t = i / (float)(NumCurvePoints - 1);
                Vector2 point = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
                GL.Vertex2(point);
            }
            
            GL.End();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        public static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Cubic Bezier Curve"
            };

            using (var window = new BezierCurveWindow(gameWindowSettings, nativeWindowSettings))
            {
                window.Run();
            }
        }
    }
}

