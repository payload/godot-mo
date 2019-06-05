using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

public class ThingsHappen : Spatial
{
    [Export]
    NodePath Raycaster = "Raycaster";

    [Export]
    NodePath Camera = "../Camera";

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            if (eventMouseButton.ButtonIndex == 1)
                PrimaryActionThroughCamera(eventMouseButton.Position);
            else if (eventMouseButton.ButtonIndex == 2)
                SecondaryActionThroughCamera(eventMouseButton.Position);
        }
    }

    private async void PrimaryActionThroughCamera(Vector2 pos)
    {
        var node = await RaycastThroughCamera(pos);
        DoPrimaryAction(node);
    }

    private async void SecondaryActionThroughCamera(Vector2 pos)
    {
        var node = await RaycastThroughCamera(pos);
        DoSecondaryAction(node);
    }

    private bool IsModifier()
    {
        return Input.IsKeyPressed((int)KeyList.Shift);
    }


    //

    private List<DudeControl> Selection = new List<DudeControl>();

    private void DoPrimaryAction(Node node)
    {
        if (node is DudeControl dude)
        {
            if (!IsModifier()) DeselectAll();

            SelectDude(dude);
            Selection.Add(dude);
        }
        else
        {
            if (!IsModifier()) DeselectAll();
        }

        if (node is Factory factory)
        {
            factory.Produce();
        }
    }

    private void DoSecondaryAction(Node node)
    {
        if (Selection.Count > 0)
        {
            if (!IsModifier())
            {
                Selection.ForEach((dude) => dude.Duties.Clear());
            }

            if (node is Spatial spatial)
            {
                Selection.ForEach((dude) => dude.AddDuty(() =>
                    dude.MoveTo(spatial.Translation) && dude.Stop()
                ));
            }
        }
    }

    //

    private async Task<Node> RaycastThroughCamera(Vector2 pos)
    {
        var camera = GetNode<Camera>(Camera);
        var raycaster = GetNode<Raycaster>(Raycaster);

        var from = camera.ProjectRayOrigin(pos);
        var direction = camera.ProjectRayNormal(pos);
        return await raycaster.Cast(from, direction);
    }

    private void SelectDude(DudeControl dude)
    {
        if (dude is Colorful colorful)
            colorful.SetColor(Colors.Gold);
    }

    private void DeselectDude(DudeControl dude)
    {
        if (dude is Colorful colorful)
            colorful.ResetColor();
    }

    private void DeselectAll()
    {
        Selection.ForEach((dude) => DeselectDude(dude));
        Selection.Clear();
    }
}
