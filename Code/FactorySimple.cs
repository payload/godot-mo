using Godot;
using System;

public class FactorySimple : Spatial
{	
    public override void _Ready()
    {
        var product = GD.Load<PackedScene>("res://Scenes/Dude.tscn");
        var instance = product.Instance() as Spatial;

        var spawn = GetNodeOrNull<Spatial>("Spawn");
        if (spawn != null)
        {
            instance.Transform = spawn.GlobalTransform;
        }

        GetNode("/root/World/Dynamic").AddChild(instance);
    }

    public override void _Process(float delta)
    {
    }
}
