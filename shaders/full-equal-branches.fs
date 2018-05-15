uniform float time;
uniform float branchCount;
uniform float direction;
uniform float rotation;

uniform vec2 resolution;
uniform vec2 aspect;

uniform vec4 bgColor;
uniform vec4 fgColor;
uniform vec4 pulseColor;

void main(void) {
	float timespeedup = mod(60.0*time, 120.0);

	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = degrees(atan(position.y,position.x)) ;
	}

	float spinValue = mod(- rotation * angle - direction * timespeedup * 1.5 - 500.0*radius, 360.0 / branchCount) * 0.1 + 3.1415;
	float pulseValue = sin(mod(timespeedup * 0.0528 + radius * 2.0 + 2.0, 6.2832)) * 0.5 + 0.5;

	vec4 colorVector = mix(fgColor, pulseColor, pulseValue);
	float sharpenedSpinValue = min(sin(spinValue)
		+ sin(3.0 * spinValue) / 3.0
		+ sin(5.0 * spinValue) / 5.0, 0.7) * 1.3;
	vec4 spinVector = mix(bgColor, colorVector, sharpenedSpinValue);
	float flareValue = max(0.0, min(radius / 0.1 - 0.15, 1.0));
	gl_FragColor = mix(bgColor, spinVector, flareValue);
}
