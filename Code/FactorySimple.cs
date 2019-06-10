using Godot;
using System;

public class FactorySimple : Spatial, Factory
{	
    [Export]
    NodePath Spawn = "Spawn";

    [Export]
    Resource Some;

    [Export]
    PackedScene SSSS;

    [Export]
    NodePath Dynamic = "../../Dynamic";

    PackedScene Product; 

    public override void _Ready()
    {
        Product = GD.Load<PackedScene>("res://Scenes/Dude.tscn");
        Produce();
    }

    public void Produce() {
        var spawn = GetNode<Spatial>(Spawn);
        var dynamic = GetNode(Dynamic);
        var instance = Product.Instance() as Spatial;
        instance.Transform = spawn.GlobalTransform;
        instance.AddToGroup("Dudes", true);
        dynamic.AddChild(instance);
    }
}
