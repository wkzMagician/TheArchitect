extends SceneTree

# Pass the extracted base-game project after --. Copied assets must not
# redirect the original game's UID references when the mod is mounted.
var seen: Dictionary = {}
var changed := 0
var uid_pattern := RegEx.new()

func _initialize() -> void:
	uid_pattern.compile('uid="(uid://[^"]+)"')
	var args := OS.get_cmdline_user_args()
	if args.is_empty() or not DirAccess.dir_exists_absolute(args[0]):
		push_error("Pass the extracted base-game project directory.")
		quit(1)
		return
	visit(args[0], false)
	visit("res://TheArchitect", true)
	visit("res://TheArchitectCode", true)
	print("Fixed %d conflicting resource UIDs (including base-game collisions)." % changed)
	quit()

func visit(path: String, repair: bool) -> void:
	for file in DirAccess.get_files_at(path):
		if not file.get_extension() in ["import", "uid", "tscn", "tres"]:
			continue
		var full_path := path.path_join(file)
		var content := FileAccess.get_file_as_string(full_path)
		var uid := ""
		if file.ends_with(".uid"):
			uid = content.strip_edges()
		else:
			var definition := content if file.ends_with(".import") else content.get_slice("\n", 0)
			var found := uid_pattern.search(definition)
			if found:
				uid = found.get_string(1)
		if not uid.begins_with("uid://"):
			continue
		if repair and seen.has(uid):
			var replacement := ResourceUID.id_to_text(ResourceUID.create_id())
			while seen.has(replacement):
				replacement = ResourceUID.id_to_text(ResourceUID.create_id())
			content = content.replace(uid, replacement)
			FileAccess.open(full_path, FileAccess.WRITE).store_string(content)
			uid = replacement
			changed += 1
		seen[uid] = full_path
	for directory in DirAccess.get_directories_at(path):
		if directory in [".godot", ".git", ".artifacts", "bin", "obj", "packages", "mods", "node_modules"]:
			continue
		visit(path.path_join(directory), repair)
