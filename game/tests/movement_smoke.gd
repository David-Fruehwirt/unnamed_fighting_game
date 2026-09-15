extends SceneTree
## Run: godot --headless --path game --script res://tests/movement_smoke.gd
var failures: Array[String] = []
var arena: Node2D
var player: CharacterBody2D

func _initialize() -> void:
	_run.call_deferred()

func check(condition: bool, message: String) -> void:
	if condition:
		print("PASS: ", message)
	else:
		failures.append(message)
		push_error(message)

func frames(count: int) -> void:
	for i in count:
		await physics_frame
		await process_frame

func _run() -> void:
	arena = load("res://scenes/arena.tscn").instantiate()
	root.add_child(arena)
	player = arena.get_node("Soldier")
	await frames(10)
	check(player.is_on_floor(), "Soldier starts on the platform")
	check(absf(player.position.y - 412.0) < 1.0, "Collision feet align with the line")
	var sprites := player.find_children("*", "Sprite2D", true, false)
	check(sprites.size() == 16, "All sixteen reusable body parts are present")
	check(player.find_children("WeaponSocket", "Marker2D", true, false).size() == 2,
		"Both hands have future weapon attachment points")
	for clip in ["idle", "run", "jump", "fall"]:
		check(player.animations.has_animation(clip), "Animation exists: " + clip)
	var start_x: float = player.position.x
	Input.action_press("move_right")
	await frames(25)
	check(player.position.x > start_x + 60.0, "Right movement advances the character")
	check(player.motion_state == &"run", "Movement selects run animation")
	Input.action_release("move_right")
	await frames(15)
	check(absf(player.velocity.x) < 1.0, "Releasing movement brakes to a stop")
	Input.action_press("move_left")
	await frames(20)
	check(player.visual.scale.x == -1.0, "Left movement flips the entire rig")
	Input.action_release("move_left")
	await frames(15)
	var floor_y: float = player.position.y
	Input.action_press("jump")
	await frames(12)
	check(player.position.y < floor_y - 45.0, "Jump raises the player above the platform")
	check(not player.is_on_floor() and player.motion_state == &"jump",
		"Ascending selects the jump pose")
	Input.action_release("jump")
	await frames(14)
	check(player.motion_state == &"fall", "Descending selects the fall pose")
	await frames(40)
	check(player.is_on_floor(), "Gravity lands the player on the platform")
	check(absf(player.position.y - floor_y) < 1.0, "Landing does not sink through the line")
	# A jump briefly after leaving the edge is accepted.
	player.position.x = 881.0
	await frames(2)
	Input.action_press("jump")
	await frames(2)
	check(player.velocity.y < -100.0, "Coyote time permits a jump just after leaving the edge")
	Input.action_release("jump")
	player.reset()
	await frames(5)
	# Walking beyond the end of the line must trigger a real fall and recovery.
	player.position.x = 900.0
	await frames(55)
	check(player.position.distance_to(player.spawn_position) < 2.0,
		"Falling off the platform automatically respawns the player")
	player.position = Vector2(600, 250)
	Input.action_press("reset")
	await frames(2)
	Input.action_release("reset")
	check(player.position.distance_to(player.spawn_position) < 2.0, "R resets the player")
	player.position = Vector2(935, 240)
	Input.action_press("move_right")
	await frames(10)
	check(player.position.x <= 938.0, "Player stays inside the visible horizontal area")
	Input.action_release("move_right")
	# A buffered jump near landing fires once ground contact becomes available.
	player.reset()
	await frames(4)
	player.position.y = 390.0
	player.velocity.y = 220.0
	player.coyote_left = 0.0
	await frames(2)
	Input.action_press("jump")
	await frames(9)
	check(player.velocity.y < -100.0, "Jump input is buffered across landing")
	Input.action_release("jump")
	print("RESULT: ", failures.size(), " failures")
	quit(0 if failures.is_empty() else 1)
