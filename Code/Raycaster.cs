using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

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

class Raycaster : Spatial
{
    private const float rayLength = 1000;
    private List<Raycast> raycasts = new List<Raycast>();

    public Task<Node> CastTo(Vector3 from, Vector3 to)
    {
        var promise = new TaskCompletionSource<Node>();
        Task.Delay(1000).ContinueWith((Task task) => promise.TrySetCanceled());

        raycasts.Add(new Raycast { From = from, To = to, Promise = promise });
        return promise.Task;
    }

    public Task<Node> Cast(Vector3 from, Vector3 direction)
    {
        return CastTo(from, from + direction * rayLength);
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