extends SceneTree

# Format conversion only: preserve generated artwork/alpha, fit to native UI sizes,
# derive white hover masks, and emit Godot resources. No gameplay changes.
var project_root: String
var report: Array = []

func fitted(source: Image, side: int, padding: int) -> Image:
	var bounds := source.get_used_rect()
	var art := source.get_region(bounds)
	var scale_factor := float(side - padding * 2) / maxf(bounds.size.x, bounds.size.y)
	art.resize(maxi(1, roundi(bounds.size.x * scale_factor)), maxi(1, roundi(bounds.size.y * scale_factor)), Image.INTERPOLATE_LANCZOS)
	var output := Image.create(side, side, false, Image.FORMAT_RGBA8)
	output.fill(Color.TRANSPARENT)
	output.blit_rect(art, Rect2i(Vector2i.ZERO, art.get_size()), Vector2i((side - art.get_width()) / 2, (side - art.get_height()) / 2))
	return output

func outline(source: Image) -> Image:
	var result := Image.create(source.get_width(), source.get_height(), false, Image.FORMAT_RGBA8)
	result.fill(Color.TRANSPARENT)
	for y in range(source.get_height()):
		for x in range(source.get_width()):
			var alpha := 0.0
			for dy in range(-2, 3):
				for dx in range(-2, 3):
					if dx * dx + dy * dy > 5:
						continue
					var sx := x + dx
					var sy := y + dy
					if sx >= 0 and sy >= 0 and sx < source.get_width() and sy < source.get_height():
						alpha = maxf(alpha, source.get_pixel(sx, sy).a)
			result.set_pixel(x, y, Color(1, 1, 1, alpha))
	return result

func save_image(img: Image, relative: String) -> void:
	var full := project_root.path_join(relative)
	DirAccess.make_dir_recursive_absolute(full.get_base_dir())
	assert(img.save_png(full) == OK, "Cannot write " + full)
	report.append({"path": relative, "width": img.get_width(), "height": img.get_height(), "alpha": img.detect_alpha() != Image.ALPHA_NONE})

func save_resource(relative: String, texture: String, side: int) -> void:
	var full := project_root.path_join(relative)
	DirAccess.make_dir_recursive_absolute(full.get_base_dir())
	var file := FileAccess.open(full, FileAccess.WRITE)
	file.store_string('[gd_resource type="AtlasTexture" load_steps=2 format=3]\n\n[ext_resource type="Texture2D" path="res://%s" id="1"]\n\n[resource]\natlas = ExtResource("1")\nregion = Rect2(0, 0, %d, %d)\n' % [texture, side, side])

func _initialize() -> void:
	project_root = OS.get_cmdline_user_args()[0]
	var data: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(project_root.path_join("assets/relic-potion-production/prompts.json")))
	var preview := Image.create(1200, 960, false, Image.FORMAT_RGBA8)
	preview.fill(Color("171e2b"))
	var index := 0
	for item in data.assets:
		var id: String = item.id
		var source := Image.load_from_file(project_root.path_join("assets/relic-potion-production/source/" + id + ".png"))
		assert(source != null, "Missing source: " + id)
		source.convert(Image.FORMAT_RGBA8)
		var is_relic: bool = item.type == "relic"
		var side := 85 if is_relic else 80
		var base := "TheArchitect/images/relics/" if is_relic else "TheArchitect/images/potions/"
		var big := fitted(source, 256, 12)
		var small := fitted(source, side, 4)
		var mask := outline(small)
		var small_path := base + id + ".png" if is_relic else base + "packed/" + id + ".png"
		var outline_path := base + id + "_outline.png" if is_relic else base + "packed/" + id + "_outline.png"
		save_image(big, base + "big/" + id + ".png" if is_relic else base + id + ".png")
		save_image(small, small_path)
		save_image(mask, outline_path)
		var atlas_type := "relic" if is_relic else "potion"
		save_resource("TheArchitect/images/atlases/" + atlas_type + "_atlas.sprites/" + id + ".tres", small_path, side)
		save_resource("TheArchitect/images/atlases/" + atlas_type + "_outline_atlas.sprites/" + id + ".tres", outline_path, side)
		var cell := Vector2i((index % 4) * 300, (index / 4) * 320)
		var display := fitted(source, 210, 6)
		preview.blend_rect(display, Rect2i(Vector2i.ZERO, display.get_size()), cell + Vector2i(45, 10))
		preview.fill_rect(Rect2i(cell + Vector2i(35, 225), Vector2i(100, 90)), Color("ede4d0"))
		preview.blend_rect(small, Rect2i(Vector2i.ZERO, small.get_size()), cell + Vector2i(42, 228))
		preview.blend_rect(mask, Rect2i(Vector2i.ZERO, mask.get_size()), cell + Vector2i(165, 228))
		print("Packed ", id, " : 256 + ", side, " + outline")
		index += 1
	save_image(preview, "assets/relic-potion-production/preview/contact-sheet.png")
	FileAccess.open(project_root.path_join("assets/relic-potion-production/format-report.json"), FileAccess.WRITE).store_string(JSON.stringify(report, "\t"))
	quit()

