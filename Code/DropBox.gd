extends CSGBox

signal items_changed(items)

var items = []

func _ready():
	$Area.connect("body_entered", self, "on_item_entered")
	$Area.connect("body_exited", self, "on_item_exited")

func on_item_entered(node: Node):
	items.append(node)
	emit_signal("items_changed", items)

func on_item_exited(node: Node):
	items.erase(node)
	emit_signal("items_changed", items)
