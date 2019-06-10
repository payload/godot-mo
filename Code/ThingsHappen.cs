using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

public class ThingsHappen : Spatial
{
    [Export]
    NodePath Raycaster = "Raycaster";

    [Export]
    NodePath Camera = "../Camera";

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton)
            OnMouseClick(eventMouseButton);

        if (@event is InputEventKey key && key.IsPressed())
        {
            var number = key.Scancode - 48;
            if (Assignments != null && number >= 0 && number < Assignments.Count)
            {
                Assignments[number].Assign();
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
        var block = collider as Block;
        var just_floor = dude == null && factory == null && item == null && block == null;
        var selected = Selection.Count > 0;

        if (leftClick && !shift && dude != null)
            SelectOnly(dude);
        if (leftClick && shift && dude != null)
            SelectDude(dude);
        if (leftClick && !shift && dude == null)
            DeselectAll();
        if (leftDblClick && dude != null)
            SelectAllDudesHere();
        if (leftClick && factory != null)
            factory.Produce();
        if (rightClick && selected && !shift)
            SelectionClearDuties();
        // if (rightClick && selected && just_floor)
        //    SelectionMoveTo(pos);
        if (rightClick && selected && item != null)
            SelectionMoveToAnd(pos, "Pick up", (d) => d.PickUp(item));
        if (rightClick && ctrl && selected && just_floor)
            SelectionMoveToAnd(pos, "Drop item", (d) => d.DropItem());
        if (rightClick && Selection.Count == 1)
            ShowPossibleAssignements(mousePos, raycast);
    }

    private void SelectAllDudesHere()
    {
        var dudes = from node in GetTree().GetNodesInGroup("Dudes")
                    let dude = node as DudeControl
                    let visi = node as HasVisibilityNotifier
                    where visi != null && visi.VisibilityNotifier.IsOnScreen()
                    select dude;

        Selection.Clear();
        foreach (var dude in dudes)
            SelectDude(dude);
    }

    private void SelectionClearDuties() =>
        Selection.ForEach((dude) => dude.Duties.Clear());

    private void SelectionMoveTo(Vector3 pos) =>
        SelectionAddDuty("Move", (dude) => dude.MoveTo(pos) && dude.Stop());

    private void SelectionAddDuty(string name, Func<DudeControl, bool> func) =>
        Selection.ForEach((dude) => dude.AddDuty(name, () => func(dude)));

    private void SelectionMoveToAnd(Vector3 pos, string name, Func<DudeControl, bool> func)
    {
        SelectionMoveTo(pos);
        SelectionAddDuty(name, func);
    }

    //

    IList<Assignment> Assignments;

    private void ShowPossibleAssignements(Vector2 mousePos, RaycastResponse raycast)
    {
        var dude = Selection[0];
        Assignments = dude.GetAssignmentsWith(raycast);
        ShowSelectableActions(mousePos);
    }

    private void ShowSelectableActions(Vector2 mousePos)
    {
        var list = GetNodeOrNull<ItemList>("/root/World/SelectableActions");

        list.Clear();
        foreach (var assignment in Assignments)
            list.AddItem(assignment.Name);

        list.RectPosition = mousePos;
        list.Visible = true;
    }

    private void _on_SelectableActions_item_selected(int index)
    {
        var list = GetNodeOrNull<ItemList>("/root/World/SelectableActions");

        list.Visible = false;

        GD.Print("Assignment ", Assignments[index].Name);

        Assignments[index].Assign();
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

    public static T InstantiateAt<T>(Transform place, Resource resource, Node parent) where T : Spatial
    {
        var scene = GD.Load<PackedScene>(resource.GetPath());
        var instance = scene.Instance() as T;
        instance.Transform = place;
        parent.AddChild(instance);
        return instance;
    }

    public static Spatial InstantiateAt(Transform place, Resource resource, Node parent) =>
        InstantiateAt<Spatial>(place, resource, parent);
}

class SomeAssignment : Assignment
{
    Action Action;
    public String Name { get; set; }

    public void Assign() => Action();

    public SomeAssignment(string name, Action action)
    {
        Name = name;
        Action = action;
    }
}