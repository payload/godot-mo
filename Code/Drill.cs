using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Drill : RigidBody, Container
{
    public Spatial Spatial => this;

    public List<GameItem> Items => items;
    private List<GameItem> items = new List<GameItem>();

    public bool PutItem(GameItem item)
    {
        Items.Add(item);
        return true;
    }

    public bool TakeItem(GameItem item)
    {
        GD.Print("Remove", Items.Count);
        return Items.Remove(item);
    }

    public void MineItem()
    {
        var item = new SomeItem("Coal");
        Items.Add(item);
    }

    private void _on_Timer_timeout() => MineItem();
}

