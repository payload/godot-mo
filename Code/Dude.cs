using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;


class MoveToTask : AbstractDuty
{
    public Vector3 dest;
    public DudeControl dude;

    override protected IEnumerator<object> Enumerator()
    {
        while (!(dude.MoveTo(dest) && dude.Stop())) yield return null;
    }
}

class SomeDuty : AbstractDuty
{
    private Func<bool> Func;

    public SomeDuty(Func<bool> func)
    {
        Func = func;
    }

    override protected IEnumerator<object> Enumerator()
    {
        while (!Func()) yield return null;
    }
}

public class Dude : RigidBody, Colorful, DudeControl, HasVisibilityNotifier
{
    float Delta;

    public override void _Process(float delta)
    {
        Delta = delta;
        ProcessTask();

        if (LinearVelocity.LengthSquared() > 0.01)
        {
            var visual = GetNode<Spatial>("Visual");
            var angle = Mathf.Atan2(LinearVelocity.x, LinearVelocity.z);
            visual.Rotation = new Vector3(0, angle, 0);
        }
    }

    void ProcessTask()
    {
        if (Duties.Count > 0 && !Duties[0].Tick())
            Duties.RemoveAt(0);
    }

    // DudeControl

    public List<Duty> Duties { get => duties; }
    List<Duty> duties = new List<Duty>();

    public bool Stop()
    {
        LinearDamp = 0.98F;
        return LinearVelocity.LengthSquared() < 0.1;
    }

    public bool MoveTo(Vector3 dest)
    {
        var diff = dest - Translation;
        var angle = LinearVelocity.AngleTo(diff);
        var done = diff.LengthSquared() < 0.1;

        LinearDamp = Mathf.Abs(angle) < 0.5 ? -1F : 0.98F;
        if (!done)
        {
            ApplyCentralImpulse(Vec.Clamp(dest - Translation, Delta));
        }
        return done;
    }

    List<GameItem> inventory = new List<GameItem>();

    public bool PickUp(GameItem item)
    {
        var spatial = item.Spatial;
        if (Translation.DistanceSquaredTo(spatial.Translation) < 0.2)
        {
            var hands = GetNode<Spatial>("Visual/Hands");
            var owner = spatial.Owner;

            spatial.GetParent().RemoveChild(spatial);
            hands.AddChild(spatial);

            spatial.Owner = owner;
            spatial.Transform = Transform.Identity;

            spatial.Translate(new Vector3(0, inventory.Count * 0.21F, 0));
            inventory.Add(item);

            return true;
        }
        return false;
    }


    public bool DropItem()
    {
        if (inventory.Count > 0)
        {
            var item = inventory[inventory.Count - 1];
            var spatial = item.Spatial;
            var owner = spatial.Owner;

            inventory.RemoveAt(inventory.Count - 1);
            spatial.GetParent().RemoveChild(spatial);
            GetParent().AddChild(spatial);

            spatial.Owner = owner;
            spatial.Translation = Translation;
        }

        return true;
    }

    public void AddDuty(Func<bool> func) => AddDuty(new SomeDuty(func));
    public void AddDuty(Duty duty) => Duties.Add(duty);
    private Assignment CreateAssignment(string name, Func<bool> func) => new SomeAssignment(name, () => AddDuty(func));

    public List<Assignment> GetAssignmentsWith(RaycastResponse raycast)
    {
        var assignments = new List<Assignment>();
        var pos = raycast.position;
        var block = raycast.collider as Block;

        if (block != null)
            assignments.AddRange(GetBuildOnBlockAssignments(block));
        
        // assignments.Add(CreateAssignment("Move", () => MoveTo(pos) && Stop()));
        
        return assignments;
    }

    //

    private IEnumerable<Assignment> GetBuildOnBlockAssignments(Block block) {
        foreach (var building in block.PossibleBuildings)
        {
            yield return CreateAssignment(
                "Build " + building.GetPath().ReplaceN("res://", "").BaseName(),
                () => BuildOnBlock(block, building)
            );
        }
    }

    private bool BuildOnBlock(Block block, Resource building) =>
        MoveTo(block.ConstructionPoint.origin) &&
        Stop() &&
        ThingsHappen.InstantiateAt(block.ConstructionPoint, building, GetParent()) != null;

    // Colorful

    MeshInstance VisualBody { get => GetNode<MeshInstance>("Visual/Body"); }

    public void SetColor(Color color)
    {
        VisualBody.MaterialOverride = MaterialCache.FromColor(color);
    }

    public void ResetColor()
    {
        VisualBody.MaterialOverride = null;
    }

    public async Task ShortlySetColor(Color color, float seconds)
    {
        SetColor(color);
        await ToSignal(GetTree().CreateTimer(seconds), "timeout");
        ResetColor();
    }

    // HasVisibilityNotifier

    public VisibilityNotifier VisibilityNotifier => GetNode<VisibilityNotifier>("VisibilityNotifier");
}

public class Vec
{
    public static Vector3 Clamp(Vector3 vec, float length)
    {
        return vec.Length() > length ? vec.Normalized() * length : vec;
    }
}