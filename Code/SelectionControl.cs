using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

public class SelectionControl : Spatial
{
    private DudeControl selectedDude = null;

    [Export]
    RID Some;

    [Export]
    NodePath Raycaster = "Raycaster";

    [Export]
    NodePath Camera = "/root/World/Camera";

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == 1)
        {
            SelectThroughCamera(eventMouseButton.Position);
        }
    }

    private async void SelectThroughCamera(Vector2 pos)
    {
        var camera = GetNode<Camera>(Camera);
        var raycaster = GetNode<Raycaster>(Raycaster);
        
        var from = camera.ProjectRayOrigin(pos);
        var direction = camera.ProjectRayNormal(pos);
        var node = await raycaster.Cast(from, direction);

        if (node is DudeControl dude)
            SelectDude(dude);
        else if (node is Spatial spatial && selectedDude != null)
        {
            if (!Input.IsKeyPressed((int)KeyList.Shift))
                selectedDude.Duties.Clear();

            selectedDude.Duties.Add(new MoveToTask
            {
                dest = spatial.Translation,
                dude = selectedDude,
            });

            if (!Input.IsKeyPressed((int)KeyList.Shift))
                DeselectDude();
        }
    }

    private void SelectDude(DudeControl dude)
    {
        selectedDude = dude;

        if (dude is Colorful colorful)
            colorful.SetColor(Colors.Gold);
    }

    private void DeselectDude() {
        if (selectedDude is Colorful colorful)
            colorful.ResetColor();

        selectedDude = null;
    }
}
