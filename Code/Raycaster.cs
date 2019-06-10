using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

struct Raycast
{
    public Vector3 From;
    public Vector3 To;
    public TaskCompletionSource<RaycastResponse> Promise;
}

public struct RaycastResponse
{
    public Node collider;
    public int collider_id;
    public Vector3 normal;
    public Vector3 position;
    public RID rid;
    public int shape;
}

class Raycaster : Spatial
{
    private const float rayLength = 1000;
    private List<Raycast> raycasts = new List<Raycast>();

    public Task<RaycastResponse> CastTo(Vector3 from, Vector3 to)
    {
        var promise = new TaskCompletionSource<RaycastResponse>();
        Task.Delay(1000).ContinueWith((Task task) => promise.TrySetCanceled());

        raycasts.Add(new Raycast { From = from, To = to, Promise = promise });
        return promise.Task;
    }

    public Task<RaycastResponse> Cast(Vector3 from, Vector3 direction)
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
            var response = new RaycastResponse();

            if (result.ContainsKey("collider") && result["collider"] is Node node)
                response.collider = node;
            if (result.ContainsKey("collider_id") && result["collider_id"] is int collider_id)
                response.collider_id = collider_id;
            if (result.ContainsKey("normal") && result["normal"] is Vector3 normal)
                response.normal = normal;
            if (result.ContainsKey("position") && result["position"] is Vector3 position)
                response.position = position;
            if (result.ContainsKey("rid") && result["rid"] is RID rid)
                response.rid = rid;
            if (result.ContainsKey("shape") && result["shape"] is int shape)
                response.shape = shape;

            raycast.Promise.SetResult(response);
        }

        raycasts.Clear();
    }
}