using Godot;
using System;

public class RtsCamera : Camera
{
    public override void _Process(float delta)
    {

    }

    public override void _UnhandledInput(InputEvent @event) {
        if (Input.IsActionPressed("zoom_in"))
            Size = Mathf.Max(3, Size * 0.9F);
        if (Input.IsActionPressed("zoom_out"))
            Size = Mathf.Min(10, Size * 1.1F);

        if (@event is InputEventMouseButton mouse)
            GD.Print(mouse.GetButtonIndex(), " ", mouse.AsText());
    }
}
