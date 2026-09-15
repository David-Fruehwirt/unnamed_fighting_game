extends Node2D

@onready var soldier: CharacterBody2D = $Soldier
@onready var state_label: Label = $HUD/State

func _process(_delta: float) -> void:
	state_label.text = String(soldier.motion_state).to_upper()

func _ready() -> void:
	var args := OS.get_cmdline_user_args()
	if "--capture" in args:
		var index := args.find("--capture")
		var path := args[index + 1] if index + 1 < args.size() else "user://arena.png"
		_capture(path)

func _capture(path: String) -> void:
	await get_tree().create_timer(0.4).timeout
	if "--run-pose" in OS.get_cmdline_user_args():
		soldier.set_physics_process(false)
		soldier.animations.play("run")
		soldier.animations.seek(0.12, true)
		soldier.animations.pause()
	await RenderingServer.frame_post_draw
	var error := get_viewport().get_texture().get_image().save_png(path)
	print("Screenshot: ", path, " result=", error)
	get_tree().quit(0 if error == OK else 1)
