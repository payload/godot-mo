extends Spatial

var cam: Spatial

func _ready():
	cam = get_node("/root/World/Camera")

func _process(_delta):
	transform = transform.looking_at(cam.translation, Vector3.UP)
