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
        while (!dude.MoveTo(dest)) yield return null;
        while (!dude.Stop()) yield return null;
    }
}

public class Dude : RigidBody, Colorful, DudeControl
{
    float Delta;

    public override void _Process(float delta)
    {
        Delta = delta;
        ProcessTask();

        var visual = GetNode<Spatial>("Visual");
        var angle = Mathf.Atan2(LinearVelocity.x, LinearVelocity.z);
        visual.Rotation = new Vector3(0, angle, 0);
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
        LinearDamp = -1F;
        ApplyCentralImpulse((dest - Translation).Normalized() * Delta);
        return Translation.DistanceSquaredTo(dest) < 0.1;
    }

    public bool PickUp(GameItem item)
    {
        if (Translation.DistanceSquaredTo(item.Spatial.Translation) < 0.2)
        {
            AddChild(item.Spatial);
            item.Spatial.Translation = Vector3.Forward;
            return true;
        }
        return false;
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
