uniform float time;
uniform float branchCount;
uniform float direction;
uniform float rotation;

uniform vec2 resolution;
uniform vec2 aspect;

uniform vec4 bgColor;
uniform vec4 fgColor;

void main(void) {
	float timespeedup = mod(60.0*time, 120.0);
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = degrees(atan(position.y, position.x));
	}

	float spinValue = mod(- direction * rotation * angle - direction * 1.5 * timespeedup - 120.0*log(radius), 360.0 / branchCount) * 0.1 + 3.141;
	float sharpenedSpinValue = min(sin(spinValue)
		+ sin(3.0 * spinValue) / 3.0
		+ sin(5.0 * spinValue) / 5.0, 0.7) * 1.3;
	vec4 spinVector = mix(bgColor, fgColor, sharpenedSpinValue);
	float flareValue = max(0.0, min(radius / 0.1 - 0.2, 1.0));
	gl_FragColor = mix(bgColor, spinVector, flareValue);
}
