using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface Colorful
{
    void SetColor(Color color);
    void ResetColor();
    Task ShortlySetColor(Color color, float seconds = 0.66F);
}

public interface DudeControl
{
    List<Duty> Duties { get; }

    bool Stop();
    bool MoveTo(Vector3 pos);
    bool PickUp(GameItem item);
    bool DropItem();
    void AddDuty(Func<bool> func);
}

public interface GameItem
{
    Spatial Spatial { get; }

    int Amount { get; }

    GameItem Combine(GameItem item);
}

public interface Duty
{
    bool Tick();
}

public interface Factory {
    void Produce();
}