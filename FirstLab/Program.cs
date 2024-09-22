// Вариант 5. Построение кубической кривой Безье

// Постройте кубическую кривую Безье, используя четыре контрольные точки.
// Контрольные точки должны быть отображены на экране и иметь возможность перемещения.
// Кривая должна обновляться в реальном времени при изменении положения любой контрольной точки.
// Дополнительно: Реализуйте анимацию, где кривая плавно изменяет свою форму в зависимости от времени.

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

public class BezierCurveWindow : GameWindow
{
    private int _program;
    private int _vao, _vbo, _ebo;

    public BezierCurveWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // Контрольные точки кривой Безье
        Vector3[] controlPoints = new Vector3[]
        {
            new Vector3(-0.8f, -0.8f, 0.0f),
            new Vector3(-0.4f, 0.8f, 0.0f),
            new Vector3(0.4f, -0.8f, 0.0f),
            new Vector3(0.8f, 0.8f, 0.0f)
        };

        // Создаем шейдерную программу
        _program = CreateProgram(vertexShaderSource, fragmentShaderSource);

        CheckProgramLink(_program);

        int controlPointsLocation = GL.GetUniformLocation(_program, "controlPoints");
        
        GL.UseProgram(_program);
        
        // Загружаем контрольные точки
        GL.Uniform3(controlPointsLocation, 4, ref controlPoints[0].X);

        // Подготовка VAO и буферов для значений t
        float[] tValues = new float[100];
        for (int i = 0; i < tValues.Length; i++)
        {
            tValues[i] = (float)i / (tValues.Length - 1);
        }

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, tValues.Length * sizeof(float), tValues, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_program);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.LineStrip, 0, 100);

        Context.SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteProgram(_program);
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
    }

    private int CreateProgram(string vertexShaderSource, string fragmentShaderSource)
    {
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);

        GL.DetachShader(program, vertexShader);
        GL.DetachShader(program, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return program;
    }

    private void CheckProgramLink(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(program);
            Console.WriteLine($"Ошибка линковки программы: {infoLog}");
        }
    }

    private static string vertexShaderSource = @"
        #version 330 core
        layout(location = 0) in float t;

        uniform vec3 controlPoints[4];
        void main()
        {
            float u = 1.0 - t;
            vec3 p = u * u * u * controlPoints[0]
                   + 3 * u * u * t * controlPoints[1]
                   + 3 * u * t * t * controlPoints[2]
                   + t * t * t * controlPoints[3];
            gl_Position = vec4(p, 1.0);
        }";

    private static string fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        void main()
        {
            FragColor = vec4(0.5, 0.5, 0.6, 1.0);
        }";

    public static void Main(string[] args)
    {
        var gameWindowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(500, 500),
            Title = "OpenTK Bezier Curve"
        };

        using (var window = new BezierCurveWindow(gameWindowSettings, nativeWindowSettings))
        {
            window.Run();
        }
    }
}
