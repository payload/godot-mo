using Godot;
using System;
using System.Collections.Generic;

public class DropBox : Spatial, Container
{
    public Spatial Spatial => this;

    public List<GameItem> Items => items;
    List<GameItem> items = new List<GameItem>();

    public bool PutItem(GameItem item)
    {
        if (item is Spatial spatial)
        {
            var transform = GetNode<Spatial>("Drop").Transform;
            ThingsHappen.Reseat(spatial, this, transform);
        }

        items.Add(item);
        return true;
    }

    public bool TakeItem(GameItem item)
    {
        if (item is Spatial spatial)
            RemoveChild(spatial);

        return items.Remove(item);
    }
}
