using Godot;

public partial class Camera3d : Camera3D
{
    [Export] public float MoveSpeed { get; set; } = 10.0f;
    [Export] public float MouseSensitivity { get; set; } = 0.002f;

    private float _yaw;
    private float _pitch;

    public override void _Ready()
    {
        _yaw = Rotation.Y;
        _pitch = Rotation.X;

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key &&
            key.Pressed && key.Keycode == Key.Escape)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        if (@event is InputEventMouseButton mouse &&
            mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        if (@event is InputEventMouseMotion motion &&
            Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _yaw -= motion.Relative.X * MouseSensitivity;
            _pitch -= motion.Relative.Y * MouseSensitivity;

            _pitch = Mathf.Clamp(
                _pitch,
                Mathf.DegToRad(-89),
                Mathf.DegToRad(89)
            );

            Rotation = new Vector3(_pitch, _yaw, 0);
        }
    }

    public override void _Process(double delta)
    {
        Vector2 input = Vector2.Zero;

        if (Input.IsKeyPressed(Key.W)) input.Y -= 1;
        if (Input.IsKeyPressed(Key.S)) input.Y += 1;
        if (Input.IsKeyPressed(Key.A)) input.X -= 1;
        if (Input.IsKeyPressed(Key.D)) input.X += 1;

        Vector3 forward = new(-Mathf.Sin(_yaw), 0, -Mathf.Cos(_yaw));
        Vector3 right = new(Mathf.Cos(_yaw), 0, -Mathf.Sin(_yaw));

        Vector3 direction = right * input.X - forward * input.Y;

        if (Input.IsKeyPressed(Key.Space))
            direction.Y += 1;

        if (Input.IsKeyPressed(Key.C))
            direction.Y -= 1;

        if (direction.LengthSquared() > 0)
            direction = direction.Normalized();

        GlobalPosition += direction * MoveSpeed * (float)delta;
    }
}