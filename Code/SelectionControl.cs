using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

struct Raycast
{
    public Vector3 From;
    public Vector3 To;
    public TaskCompletionSource<Node> Promise;
}

struct RaycastResponse
{
    Node collider;
    int collider_id;
    Vector3 normal;
    Vector3 position;
    RID rid;
    int shade;
}

public class SelectionControl : Spatial
{
    private const float rayLength = 1000;
    private List<Raycast> raycasts = new List<Raycast>();
    private DudeControl selectedDude = null;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == 1)
        {
            SelectThroughCamera(eventMouseButton.Position);
        }
    }

    private async void SelectThroughCamera(Vector2 pos)
    {
        var camera = GetNode<Camera>("/root/World/Camera");
        var from = camera.ProjectRayOrigin(pos);
        var to = from + camera.ProjectRayNormal(pos) * rayLength;

        var node = await MakeRaycast(from, to);

        if (node is DudeControl dude)
            SelectDude(dude);
        else if (node is Spatial spatial && selectedDude != null)
        {
            if (!Input.IsKeyPressed((int)KeyList.Shift))
                selectedDude.Duties.Clear();

            selectedDude.Duties.Add(new MoveToTask
            {
                dest = spatial.Translation,
                dude = selectedDude,
            });

            if (!Input.IsKeyPressed((int)KeyList.Shift))
                DeselectDude();
        }
    }

    private void SelectDude(DudeControl dude)
    {
        selectedDude = dude;

        if (dude is Colorful colorful)
            colorful.SetColor(Colors.Gold);
    }

    private void DeselectDude() {
        if (selectedDude is Colorful colorful)
            colorful.ResetColor();

        selectedDude = null;
    }

    private Task<Node> MakeRaycast(Vector3 from, Vector3 to)
    {
        var promise = new TaskCompletionSource<Node>();
        Task.Delay(1000).ContinueWith((Task task) => promise.TrySetCanceled());

        raycasts.Add(new Raycast { From = from, To = to, Promise = promise });
        return promise.Task;
    }

    public override void _PhysicsProcess(float delta)
    {
        var world = GetWorld();
        var spaceState = world.DirectSpaceState;

        foreach (var raycast in raycasts)
        {
            var result = spaceState.IntersectRay(raycast.From, raycast.To);

            if (result.ContainsKey("collider") && result["collider"] is Node node)
            {
                raycast.Promise.SetResult(node);
            }
            else
            {
                raycast.Promise.SetResult(null);
            }
        }

        raycasts.Clear();
    }
}
