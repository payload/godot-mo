using Godot;
using Godot.Collections;
using System;
using System.Linq;

[Tool]
class AreaGenerator : Spatial
{
    [Export]
    OpenSimplexNoise Noise;

    [Export]
    PackedScene scene;

    [Export]
    float Min { get => min; set { min = value; Regenerate(); } }
    float min;

    [Export]
    float Max { get => max; set { max = value; Regenerate(); } }
    float max;

    [Export]
    Vector2 Size = new Vector2(20, 20);

    float osn_period;
    float osn_persistence;
    float osn_luco;
    int osn_octaves;
    int osn_seed;

    void Generate(Node parent)
    {
        for (float z = -Size.y; z <= Size.y; ++z)
            for (float x = -Size.x; x <= Size.x; ++x)
            {
                var v = Noise.GetNoise2d(x, z);
                var pos = new Vector3(x, 0, z);
                var transform = Transform.Identity.Translated(pos);

                if (v >= Min && v <= Max)
                    ThingsHappen.InstantiateAt(transform, scene, parent);
            }
    }

    public void Regenerate()
    {
        if (Noise != null && scene != null)
        {
            SaveNoise();
            ClearChildren();
            Generate(this);
        }
    }

    private void ClearChildren()
    {
        foreach (var node in GetChildren().Cast<Node>())
            RemoveChild(node);
    }

    public override void _Process(float delta)
    {
        if (NoiseChanged())
            Regenerate();
    }

    private void SaveNoise() {
        osn_period = Noise.Period;
        osn_persistence = Noise.Persistence;
        osn_octaves = Noise.Octaves;
        osn_luco = Noise.Lacunarity;
        osn_seed = Noise.Seed;
    }

    private bool NoiseChanged() =>
        Noise.Period != osn_period ||
        Noise.Persistence != osn_persistence ||
        Noise.Octaves != osn_octaves ||
        Noise.Lacunarity != osn_luco ||
        Noise.Seed != osn_seed;

    public override void _EnterTree()
    {
        SetProcess(true);
        Regenerate();
    }

    public override void _ExitTree()
    {
        ClearChildren();
    }
}