using Godot;
using System;

public class RtsCamera : Camera
{
    public override void _Process(float delta)
    {
        var margin = 50;
        var viewport = GetViewport();
        var pos = viewport.GetMousePosition();
        
        if (pos.x < margin && pos.x > 0)
            HOffset -= delta * 3;
        if (pos.x > -margin + viewport.Size.x && pos.x < viewport.Size.x)
            HOffset += delta * 3;
        if (pos.y < margin && pos.y > 0)
            VOffset += delta * 3;
        if (pos.y > -margin + viewport.Size.y && pos.y < viewport.Size.y)
            VOffset -= delta * 3;
    }

    public override void _UnhandledInput(InputEvent @event) {
        if (Input.IsActionPressed("zoom_in"))
            Size = Mathf.Max(3, Size * 0.9F);
        if (Input.IsActionPressed("zoom_out"))
            Size = Mathf.Min(12, Size * 1.1F);
    }
}
