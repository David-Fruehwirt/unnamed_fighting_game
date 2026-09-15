extends CharacterBody2D
## Physics is separate from the sixteen replaceable sprites in soldier.tscn.

@export var move_speed: float = 235.0
@export var acceleration: float = 1700.0
@export var braking: float = 2200.0
@export var gravity: float = 1350.0
@export var jump_speed: float = 465.0
@export var coyote_time: float = 0.11
@export var jump_buffer_time: float = 0.12

@onready var visual: Node2D = $Visual
@onready var animations: AnimationPlayer = $AnimationPlayer
var spawn_position: Vector2
var coyote_left: float = 0.0
var jump_buffer: float = 0.0
var facing: float = 1.0
var motion_state: StringName = &"idle"

func _ready() -> void:
	spawn_position = global_position
	_setup_input()
	animations.play("idle")

func _setup_input() -> void:
	var bindings: Dictionary = {
		"move_left": [KEY_A, KEY_LEFT],
		"move_right": [KEY_D, KEY_RIGHT],
		"jump": [KEY_SPACE, KEY_W, KEY_UP],
		"reset": [KEY_R]
	}
	for action: String in bindings:
		if InputMap.has_action(action):
			continue
		InputMap.add_action(action)
		for key: int in bindings[action]:
			var event := InputEventKey.new()
			event.physical_keycode = key as Key
			InputMap.action_add_event(action, event)

func _physics_process(delta: float) -> void:
	if Input.is_action_just_pressed("reset") or global_position.y > 700.0:
		reset()
		return
	var axis := Input.get_axis("move_left", "move_right")
	if is_on_floor():
		coyote_left = coyote_time
	else:
		coyote_left = maxf(0.0, coyote_left - delta)
		velocity.y = minf(velocity.y + gravity * delta, 800.0)
	jump_buffer = maxf(0.0, jump_buffer - delta)
	if Input.is_action_just_pressed("jump"):
		jump_buffer = jump_buffer_time
	if jump_buffer > 0.0 and coyote_left > 0.0:
		velocity.y = -jump_speed
		jump_buffer = 0.0
		coyote_left = 0.0
	if Input.is_action_just_released("jump") and velocity.y < -170.0:
		velocity.y = -170.0
	velocity.x = move_toward(velocity.x, axis * move_speed,
		(acceleration if axis != 0.0 else braking) * delta)
	if axis != 0.0:
		facing = signf(axis)
	visual.scale.x = facing
	move_and_slide()
	# Keep the player in the visible arena horizontally.
	position.x = clampf(position.x, 22.0, 938.0)
	_update_animation()

func _update_animation() -> void:
	var next: StringName = &"idle"
	if not is_on_floor():
		next = &"jump" if velocity.y < -20.0 else &"fall"
	elif absf(velocity.x) > 15.0:
		next = &"run"
	if next != motion_state:
		motion_state = next
		animations.play(next, 0.09)
	animations.speed_scale = clampf(absf(velocity.x) / move_speed, 0.5, 1.15) if next == &"run" else 1.0

func reset() -> void:
	global_position = spawn_position
	velocity = Vector2.ZERO
	coyote_left = 0.0
	jump_buffer = 0.0
	facing = 1.0
	visual.scale.x = facing
	motion_state = &"idle"
	animations.play("idle")
