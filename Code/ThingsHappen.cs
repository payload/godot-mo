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
        if (@event is InputEventMouseButton eventMouseButton)
            OnMouseClick(eventMouseButton);

        if (@event is InputEventKey key && key.IsPressed())
        {
            var number = key.Scancode - 48;
            if (SelectableActions != null && number >= 0 && number < SelectableActions.Length)
            {
                SelectableActions[number]();
            }
        }
    }

    private async void OnMouseClick(InputEventMouseButton mouse)
    {
        var leftClick = mouse.IsPressed() && mouse.ButtonIndex == 1 && !mouse.Doubleclick;
        var leftDblClick = mouse.IsPressed() && mouse.ButtonIndex == 1 && mouse.Doubleclick;
        var rightClick = mouse.IsPressed() && mouse.ButtonIndex == 2 && !mouse.Doubleclick;
        var rightDblClick = mouse.IsPressed() && mouse.ButtonIndex == 2 && mouse.Doubleclick;
        var pos = mouse.Position;
        var raycast = await RaycastThroughCamera(pos);

        if (leftClick) DoPrimaryAction(raycast);
        if (rightClick) DoSecondaryAction(pos, raycast);
    }


    private bool IsModifier1() => Input.IsKeyPressed((int)KeyList.Shift);
    private bool IsModifier2() => Input.IsKeyPressed((int)KeyList.Control);

    //

    private List<DudeControl> Selection = new List<DudeControl>();

    private void DoPrimaryAction(RaycastResponse raycast)
    {
        var node = raycast.collider;

        if (node is DudeControl dude)
        {
            if (!IsModifier1()) DeselectAll();

            SelectDude(dude);
            Selection.Add(dude);
        }
        else
        {
            if (!IsModifier1()) DeselectAll();
        }

        if (node is Factory factory)
        {
            factory.Produce();
        }
    }

    //

    Action[] SelectableActions;

    private void DoSecondaryAction(Vector2 mousePos, RaycastResponse raycast)
    {
        var node = raycast.collider;

        var assignment = WantToConstructOnBlock(raycast);
        if (assignment != null)
        {
            if (!IsModifier1())
                assignment.Dude.Duties.Clear();
            assignment.Dude.AddDuty(assignment.Duty);
            GD.Print("Jaja");
        }
        else if (Selection.Count > 0)
        {
            if (!IsModifier1())
            {
                Selection.ForEach((dude) => dude.Duties.Clear());
            }

            switch (node)
            {
                case DudeControl dude:
                    break;
                case Factory factory:
                    break;
                case GameItem item:
                    GD.Print("PickUp!!");
                    Selection.ForEach((dude) => dude.AddDuty(() =>
                        dude.MoveTo(item.Spatial.Translation) && dude.Stop() && dude.PickUp(item)
                    ));
                    break;
                case Spatial spatial:
                    if (IsModifier2())
                    {
                        GD.Print("DropItem!!");
                        Selection.ForEach((dude) => dude.AddDuty(() =>
                            dude.MoveTo(raycast.position) && dude.Stop() && dude.DropItem()
                        ));
                    }
                    else
                    {
                        GD.Print("MoveTo!!");
                        Selection.ForEach((dude) => dude.AddDuty(() =>
                            dude.MoveTo(raycast.position) && dude.Stop()
                        ));
                    }
                    break;
            }
        }
        else
        {
            switch (node)
            {
                case Actionable actionable:
                    SelectableActions = actionable.GetActions();
                    ShowSelectableActions(mousePos);
                    break;
            }
        }
    }

    //

    private void ShowSelectableActions(Vector2 mousePos)
    {
        var list = GetNodeOrNull<ItemList>("/root/World/SelectableActions");

        list.Clear();
        foreach (var action in SelectableActions)
            list.AddItem(action.Method.Name);

        list.RectPosition = mousePos;
        list.Visible = true;
    }

    private void _on_SelectableActions_item_selected(int index)
    {
        var list = GetNodeOrNull<ItemList>("/root/World/SelectableActions");

        list.Visible = false;

        SelectableActions[index]();
    }

    private async Task<RaycastResponse> RaycastThroughCamera(Vector2 pos)
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

    private Assignment WantToConstructOnBlock(RaycastResponse response)
    {
        var builder = Selection.Find(IsBuilder);
        if (builder != null &&
            response.collider is Block block &&
            block.Kind == BlockKind.Coal)
        {
            return new Assignment
            {
                Name = "ConstructDrill",
                Dude = builder,
                Duty = new SomeDuty(() => ConstructOnBlock(builder, block, new Drill()))
            };
        }
        return null;
    }

    class Assignment
    {
        public string Name;
        public DudeControl Dude;
        public Duty Duty;
    }

    class Drill : Construction
    {
        public PackedScene Scene => GD.Load<PackedScene>("res://Scenes/Dude.tscn");
    }

    private bool IsBuilder(DudeControl dude) => true;

    private bool ConstructOnBlock(DudeControl dude, Block block, Construction construction)
    {
        var dynamic = GetNode("/root/World/Dynamic");
        return
            dude.MoveTo(block.ConstructionPoint.origin) &&
            dude.Stop() &&
            InstantiateAt(block.ConstructionPoint, construction.Scene, dynamic) != null;
    }

    private T InstantiateAt<T>(Transform place, PackedScene scene, Node parent) where T : Spatial
    {
        GD.Print("Inst");
        var instance = scene.Instance() as T;
        instance.Transform = place;
        parent.AddChild(instance);
        return instance;
    }

    private Spatial InstantiateAt(Transform place, PackedScene scene, Node parent) =>
        InstantiateAt<Spatial>(place, scene, parent);
}
