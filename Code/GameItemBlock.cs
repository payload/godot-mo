using Godot;
using System;

public class GameItemBlock : Spatial, GameItem
{
    public Spatial Spatial => this;

    public int Amount => 1;

    public GameItem Combine(GameItem item)
    {
        return this;
    }
}
