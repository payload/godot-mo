using Godot;
using Godot.Collections;
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
    bool MoveTo(Vector3 pos, float margin = 0.1F);
    bool PickUp(GameItem item);
    bool DropItem();
    void AddDuty(string name, Func<bool> func);
    void AddDuty(Duty duty);

    Assignment GetDefaultAssignment(List<Assignment> assignments);
    List<Assignment> GetAssignmentsWith(RaycastResponse raycast);
}

public interface HasVisibilityNotifier
{
    VisibilityNotifier VisibilityNotifier { get; }
}

public interface GameItem
{
    Spatial Spatial { get; }
    int Amount { get; }
    string Name { get; }

    GameItem Combine(GameItem item);
}

public interface Container
{
    Spatial Spatial { get; }
    List<GameItem> Items { get; }

    bool TakeItem(GameItem item);
    bool PutItem(GameItem item);
}

public interface Duty
{
    string Name { get; }
    bool Tick();
}

public interface Factory
{
    void Produce();
}

public interface Block
{
    Spatial Spatial { get; }
    Transform ConstructionPoint { get; }
    BlockKind Kind { get; }
    Array<PackedScene> PossibleBuildings { get; }
}

public enum BlockKind { Undefined, Coal, Iron, Wood }

public interface Assignment
{
    string Name { get; }

    void Assign();
}
