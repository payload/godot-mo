using Godot;
using Godot.Collections;
using System;

public class SomeBlock : RigidBody, Block
{
    public Spatial Spatial => this;
    public Transform ConstructionPoint => Transform.Translated(Vector3.Up);
    
	[Export]
	public BlockKind Kind { get; set; }
	
	[Export((PropertyHint)24, "17/17:PackedScene")]
    public Array<PackedScene> PossibleBuildings { get; set; }
}
