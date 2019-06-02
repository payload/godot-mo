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

class RushAndShootTask : AbstractDudeTask {
    public Vector3 dest;
    public Node enemy;
    public Dude dude;

    override protected IEnumerator<object> Enumerator() {
        while (!dude.Arrived(dest)) {
            dude.MoveTo(dest);
            dude.Shoot(enemy);
            yield return null;
        }
        while (!dude.EnemyNotDead(enemy)) {
            dude.Shoot(enemy);
            yield return null;
        }
    }
}

public class Dude : RigidBody, Colorful
{
    Vector3 Destination;
    List<DudeTask> Tasks = new List<DudeTask>();

    public override void _Ready()
    {
        Destination = Translation;
        Tasks.Add(new RushAndShootTask{
            dude = this,
            enemy = null,
            dest = new Vector3(1,2,3)
        });
    }

    public override void _Process(float delta)
    {
        if (Tasks.Count > 0) {
            if (!Tasks[0].Tick()) {
                Tasks.RemoveAt(0);
            }
        }

        if (ToDestination().LengthSquared() < 0.1)
        {
            LinearDamp = 0.98F;
        }

        if (LinearVelocity.LengthSquared() < 0.1)
        {
            LinearDamp = -1F;
        }
    }

    public bool Arrived(Vector3 dest) {
        GD.Print("Arrived");
        return true;
    }

    public void MoveTo(Vector3 dest) {
        GD.Print("Move");
    }

    public bool EnemyNotDead(Node enemy) {
        GD.Print("EnemyNotDead");
        return true;
    }

    public void Shoot(Node enemy) {
        GD.Print("Shoot");
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
        LookFollow(state, GetGlobalTransform(), Destination);
        ApplyCentralImpulse(ToDestination().Normalized() * state.GetStep());
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
