using Godot;
using Godot.Collections;

class SomeItem : RigidBody, GameItem
{
    public Spatial Spatial => this;

    public int Amount { get; set; }

    public GameItem Combine(GameItem item)
    {
        if (item != this && item.Name == Name)
            Amount += item.Amount;
        return this;
    }

    public SomeItem(string name) {
        Amount = 1;
        Name = name;
    }
}