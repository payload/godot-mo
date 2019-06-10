extends RigidBody

export(Material) onready var material = null setget set_material, get_material

func set_material(mat):
	$MeshInstance.set_surface_material(0, mat)

func get_material():
	return $MeshInstance.get_surface_material(0)