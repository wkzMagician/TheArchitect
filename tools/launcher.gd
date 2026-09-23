extends Control

var worker := Thread.new()

func _ready() -> void:
	if OS.get_name() != "Windows":
		$Status.text = "This launcher currently supports Windows."
		return
	worker.start(_launch)

func _launch() -> void:
	var output: Array = []
	var args := PackedStringArray([
		"-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
		ProjectSettings.globalize_path("res://tools/launch-sts2.ps1"),
		"-GodotPath", OS.get_executable_path()
	])
	if "--verify-only" in OS.get_cmdline_user_args():
		args.append("-VerifyOnly")
	var code := OS.execute("powershell.exe", args, output, true, false)
	_finished.call_deferred(code, "\n".join(output))

func _finished(code: int, output: String) -> void:
	worker.wait_to_finish()
	print(output)
	if code == 0:
		get_tree().quit()
	else:
		$Status.text = "Launch failed. See .artifacts/launch-sts2.log\n\n" + output.right(3500)
		push_error($Status.text)
		if "--verify-only" in OS.get_cmdline_user_args():
			get_tree().quit(code)

func _exit_tree() -> void:
	if worker.is_started():
		worker.wait_to_finish()
