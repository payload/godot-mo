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

public class Dude : RigidBody, Colorful, DudeControl
{
    float Delta;

    public override void _Process(float delta)
    {
        Delta = delta;
        ProcessTask();

        if (LinearVelocity.LengthSquared() > 0.01) {
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
        if (!done) {
            ApplyCentralImpulse(Vec.Clamp(dest - Translation, Delta));
        }
        return done;
    }

    int items = 0;

    public bool PickUp(GameItem item)
    {
        if (Translation.DistanceSquaredTo(item.Spatial.Translation) < 0.2)
        {
            var hands = GetNode<Spatial>("Visual/Hands");
            var items = hands.GetChildCount();

            var owner = item.Spatial.Owner;
            item.Spatial.GetParent().RemoveChild(item.Spatial);
            hands.AddChild(item.Spatial);
            item.Spatial.Owner = owner;
            
            item.Spatial.Transform = Transform.Identity;
            item.Spatial.Translate(new Vector3(0, items * 0.21F, 0));

            return true;
        }
        return false;
    }

    public void AddDuty(Func<bool> func)
    {
        Duties.Add(new SomeDuty(func));
    }

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
}

public class Vec
{
    public static Vector3 Clamp(Vector3 vec, float length)
    {
        return vec.Length() > length ? vec.Normalized() * length : vec;
    }
}