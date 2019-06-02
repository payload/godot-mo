using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;


interface DudeTask {
    bool Tick();
}

abstract class AbstractDudeTask : DudeTask {
    IEnumerator<object> MyEnumerator;

    protected AbstractDudeTask() {
        MyEnumerator = Enumerator();
    }

    public bool Tick() {
        return MyEnumerator.MoveNext();
    }

    abstract protected IEnumerator<object> Enumerator();
}

class MoveToTask : AbstractDudeTask {
    public Vector3 dest;
    public Dude dude;

    override protected IEnumerator<object> Enumerator() {
        dude.LinearDamp = -1F;
        
        while (!dude.Arrived(dest)) {
            dude.MoveTo(dest);
            yield return null;
        }

        dude.LinearDamp = 0.98F;

        while (dude.LinearVelocity.LengthSquared() > 0.1) {
            yield return null;
        }
    }
}

public class Dude : RigidBody, Colorful
{
    Vector3 Destination;
    List<DudeTask> Tasks = new List<DudeTask>();
    float Delta;

    public override void _Ready()
    {
        Destination = Translation;
        Tasks.Add(new MoveToTask{
            dude = this,
            dest = new Vector3(-2, 0, 1)
        });
        Tasks.Add(new MoveToTask{
            dude = this,
            dest = new Vector3(-2, 0, -2)
        });
    }

    public override void _Process(float delta)
    {
        Delta = delta;
        if (Tasks.Count > 0) {
            if (!Tasks[0].Tick()) {
                Tasks.RemoveAt(0);
            }
        }
    }

    public bool Arrived(Vector3 dest) {
        return (dest - Translation).LengthSquared() < 0.1;
    }

    public void MoveTo(Vector3 dest) {
        ApplyCentralImpulse((dest - Translation).Normalized() * Delta);
        // AddCentralForce((dest - Translation).Normalized());
    }

    Vector3 ToDestination() => Destination - Translation;

    private void LookFollow(PhysicsDirectBodyState state, Transform currentTransform, Vector3 targetPosition)
    {
        var upDir = new Vector3(0, 1, 0);
        var curDir = currentTransform.basis.Xform(new Vector3(0, 0, 1));
        var targetDir = (targetPosition - currentTransform.origin).Normalized();
        var rotationAngle = Mathf.Acos(curDir.x) - Mathf.Acos(targetDir.x);

        state.SetAngularVelocity(upDir * (rotationAngle / state.GetStep()));
    }

    public override void _IntegrateForces(PhysicsDirectBodyState state)
    {
        // LookFollow(state, Transform.Identity, GetLinearVelocity());
    }

    //

    MeshInstance GetMeshInstance() => GetNode<MeshInstance>("MeshInstance");

    public void SetColor(Color color)
    {
        GetMeshInstance().MaterialOverride = MaterialCache.FromColor(color);
    }

    public void ResetColor() {
        GetMeshInstance().MaterialOverride = null;
    }

    public async Task ShortlySetColor(Color color, float seconds)
    {
        SetColor(color);
        await ToSignal(GetTree().CreateTimer(seconds), "timeout");
        ResetColor();
    }
}
