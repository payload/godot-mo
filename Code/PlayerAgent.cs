using Godot;
using System;

public class PlayerAgent : Node
{
    [Export]
    NodePath AgentPath = "";

    RigidBody agent;

    public override void _PhysicsProcess(float delta)
    {
        agent = GetNodeOrNull<RigidBody>(AgentPath);
        if (agent == null) {
            return;
        }

        if (Input.IsActionPressed("ui_left")) {
            MoveDirectly(Vector3.Left, delta);
        }
        if (Input.IsActionPressed("ui_right")) {
            MoveDirectly(Vector3.Right, delta);
        }
        if (Input.IsActionPressed("ui_up")) {
            MoveDirectly(Vector3.Forward, delta);
        }
        if (Input.IsActionPressed("ui_down")) {
            MoveDirectly(Vector3.Back, delta);
        }
    }

    void MoveDirectly(Vector3 direction, float delta) {
        agent.ApplyCentralImpulse(direction * delta);
    }
}
