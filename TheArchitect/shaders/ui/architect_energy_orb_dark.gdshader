shader_type canvas_item;

void fragment() {
	vec4 orb = texture(TEXTURE, UV);
	float gray = dot(orb.rgb, vec3(0.299, 0.587, 0.114));
	orb.rgb = mix(vec3(gray), orb.rgb, 0.5) * 0.85;
	COLOR = orb * COLOR;
}
