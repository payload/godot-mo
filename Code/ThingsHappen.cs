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
        var mousePos = mouse.Position;
        var raycast = await RaycastThroughCamera(mousePos);
        var shift = Input.IsKeyPressed((int)KeyList.Shift);
        var ctrl = Input.IsKeyPressed((int)KeyList.Control);
        var collider = raycast.collider;
        var pos = raycast.position;
        var dude = collider as DudeControl;
        var factory = collider as FactorySimple;
        var item = collider as GameItem;
        var just_floor = dude == null && factory == null && item == null;
        var selected = Selection.Count > 0;

        if (leftClick && !shift && dude != null)
            SelectOnly(dude);
        if (leftClick && shift && dude != null)
            SelectDude(dude);
        if (leftClick && !shift && dude == null)
            DeselectAll();
        if (leftClick && factory != null)
            factory.Produce();
        if (rightClick && selected && !shift)
            SelectionClearDuties();
        if (rightClick && selected && just_floor)
            SelectionMoveTo(pos);
        if (rightClick && selected && item != null)
            SelectionMoveToAnd(pos, (d) => d.PickUp(item));
        if (rightClick && ctrl && selected && just_floor)
            SelectionMoveToAnd(pos, (d) => d.DropItem());
    }

    private void SelectionClearDuties() => 
        Selection.ForEach((dude) => dude.Duties.Clear());

    private void SelectionMoveTo(Vector3 pos) =>
        SelectionAddDuty((dude) => dude.MoveTo(pos) && dude.Stop());

    private void SelectionAddDuty(Func<DudeControl, bool> func) =>
        Selection.ForEach((dude) => dude.AddDuty(() => func(dude)));

    private void SelectionMoveToAnd(Vector3 pos, Func<DudeControl, bool> func) {
        SelectionMoveTo(pos);
        SelectionAddDuty(func);
    }

    Action[] SelectableActions;

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

    private List<DudeControl> Selection = new List<DudeControl>();

    private void SelectOnly(DudeControl dude)
    {
        DeselectAll();
        SelectDude(dude);
    }

    private void SelectDude(DudeControl dude)
    {
        if (dude is Colorful colorful)
            colorful.SetColor(Colors.Gold);

        Selection.Add(dude);
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
        var instance = scene.Instance() as T;
        instance.Transform = place;
        parent.AddChild(instance);
        return instance;
    }

    private Spatial InstantiateAt(Transform place, PackedScene scene, Node parent) =>
        InstantiateAt<Spatial>(place, scene, parent);
}
