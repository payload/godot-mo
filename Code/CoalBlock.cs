using Godot;
using System;

class CoalBlock : RigidBody, Block
{
    public Spatial Spatial => this;
    public Transform ConstructionPoint => Transform.Translated(Vector3.Up);
    public BlockKind Kind => BlockKind.Coal;
    public PackedScene[] PossibleBuildings => new PackedScene[]{
        GD.Load<PackedScene>("res://Scenes/Dude.tscn"),
        GD.Load<PackedScene>("res://Scenes/Dude.tscn")
    };
}
