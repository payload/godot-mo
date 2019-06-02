using Godot;
using System;
using System.Threading.Tasks;

public class Dude : RigidBody, Colorful
{
    Vector3 Destination;

    public override void _Ready()
    {
        Destination = Translation;
        SetMeta("Spatial", true);
        SetMeta("Colorful", true);
    }

    public override void _Process(float delta)
    {
        if (ToDestination().LengthSquared() < 0.1)
        {
            LinearDamp = 0.98F;
        }

        if (LinearVelocity.LengthSquared() < 0.1)
        {
            LinearDamp = -1F;
        }
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
