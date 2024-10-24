// Вариант 5. Построение кубической кривой Безье
// Постройте кубическую кривую Безье, используя четыре контрольные точки.
// Контрольные точки должны быть отображены на экране и иметь возможность перемещения.
// Кривая должна обновляться в реальном времени при изменении положения любой контрольной точки.
// Дополнительно: Реализуйте анимацию, где кривая плавно изменяет свою форму в зависимости от времени.

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using FirstLab;

public class BezierWindow : GameWindow
{
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private Shader _bezierShader;
    private Shader _pointShader;

    private readonly string _vertexShaderSource = @"
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

    private readonly string _fragmentShaderSource = @"
    #version 330 core
    out vec4 FragColor;

    void main()
    {
        FragColor = vec4(2.0, 1.0, 1.0, 1.0); // Белый цвет
    }";

    private readonly string _vertexShaderSource1 = @"
    #version 330 core
    layout(location = 0) in vec2 aPosition;
    void main()
    {
        gl_Position = vec4(aPosition, 0.0, 1.0);
    }
    ";

    private readonly string _fragmentShaderSource1 = @"
    #version 330 core
    out vec4 FragColor;
    void main()
    {
        FragColor = vec4(0.0, 1.0, 0.0, 1.0);
    }
    ";

    private Vector2[] _controlPoints;
    private bool _isDragging = false;
    private bool _isAnimating = false;
    private float _time = 0.0f;
    private Vector2 _mousePosition;
    private int _selectedPoint = -1;
    private Vector2 _lastMousePos;

    private int _pointVertexArrayObject;
    private int _pointVertexBufferObject;

    private BezierWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        KeyDown += Keyboard_KeyDown;

        _controlPoints = new Vector2[]
        {
            new Vector2(-0.75f, -0.75f),
            new Vector2(-0.25f, 0.75f),
            new Vector2(0.25f, -0.75f),
            new Vector2(0.75f, 0.75f),
        };

        UpdateVertexBuffer();

        _bezierShader = new Shader(_vertexShaderSource, _fragmentShaderSource);
        _pointShader = new Shader(_vertexShaderSource1, _fragmentShaderSource1);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, 100 * 2 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        _pointVertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_pointVertexArrayObject);

        _pointVertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _pointVertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _controlPoints.Length * Vector2.SizeInBytes, _controlPoints,
            BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);
        GL.EnableVertexAttribArray(0);
    }
    
    private void Keyboard_KeyDown(KeyboardKeyEventArgs e)
    {
        if (e.Key == Keys.Space)
        {
            _isAnimating = !_isAnimating;
        }
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (_isAnimating)
        {
            _time += (float)args.Time;

            float speed = 0.001f; 

            for (int i = 0; i < _controlPoints.Length; i++)
            {
                _controlPoints[i].X += speed * (float)Math.Sin(_time + i);
                _controlPoints[i].Y += speed * (float)Math.Cos(_time + i);

                _controlPoints[i].X = Math.Clamp(_controlPoints[i].X, -0.9f, 0.9f);
                _controlPoints[i].Y = Math.Clamp(_controlPoints[i].Y, -0.9f, 0.9f);
            }
    
            UpdateVertexBuffer();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _pointVertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _controlPoints.Length * Vector2.SizeInBytes, _controlPoints, BufferUsageHint.DynamicDraw);

        _bezierShader.Use();
        GL.LineWidth(2.0f);
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.LineStrip, 0, 100);

        _pointShader.Use();
        GL.PointSize(10.0f);
        GL.BindVertexArray(_pointVertexArrayObject);
        GL.DrawArrays(PrimitiveType.Points, 0, _controlPoints.Length);

        SwapBuffers();
    }

    private void UpdateVertexBuffer()
    {
        const int segments = 100;
        var vertices = new float[segments * 2];

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector2 point = CalculateBezierPoint(t, _controlPoints);
            vertices[i * 2] = point.X;
            vertices[i * 2 + 1] = point.Y;
        }

        GL.BindVertexArray(_vertexArrayObject);
        GL.BindVertexArray(_vertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
    }

    private Vector2 CalculateBezierPoint(float t, Vector2[] controlPoints)
    {
        float u = 1.0f - t;
        return u * u * u * controlPoints[0]
               + 3 * u * u * t * controlPoints[1]
               + 3 * u * t * t * controlPoints[2]
               + t * t * t * controlPoints[3];
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        _mousePosition = new Vector2(
            MousePosition.X / Size.X * 2.0f - 1.0f,
            -(MousePosition.Y / Size.Y * 2.0f - 1.0f)
        );

        float clickThreshold = 0.1f;

        for (int i = 0; i < _controlPoints.Length; i++)
        {
            if (Vector2.Distance(_controlPoints[i], _mousePosition) < clickThreshold)
            {
                _selectedPoint = i;
                _controlPoints[_selectedPoint] = _mousePosition;
                _isDragging = true;
                UpdateVertexBuffer();
                break;
            }
        }
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        _mousePosition = new Vector2(
            MousePosition.X / Size.X * 2.0f - 1.0f,
            -(MousePosition.Y / Size.Y * 2.0f - 1.0f)
        );
        
        if (_isDragging && _selectedPoint >= 0 && _selectedPoint < _controlPoints.Length)
        {
            _controlPoints[_selectedPoint] = _mousePosition;
            UpdateVertexBuffer();
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_isDragging && e.Button == MouseButton.Left)
        {
            _isDragging = false;
            _selectedPoint = -1;
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffer(_vertexBufferObject);

        _bezierShader.Dispose();
        _pointShader.Dispose();
    }

    public static void Main(string[] args)
    {
        var gameWindowSettings = GameWindowSettings.Default;
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600), Title = "Bezier Curve"
        };

        using (var window = new BezierWindow(gameWindowSettings, nativeWindowSettings))
        {
            window.Run();
        }
    }
}
