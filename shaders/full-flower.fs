uniform float time;
uniform float branchCount;
uniform float direction;
uniform float rotation;

uniform vec2 resolution;
uniform vec2 aspect;

uniform vec4 bgColor;
uniform vec4 fgColor;
uniform vec4 pulseColor;
uniform vec4 dimColor;

// Main method : entry point of the application.
// NOTE
// Only have one variable set each time (if you uncomment a line, comment the alternative line)
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	float radTime = timespeedup * 3.1415 / 60.0;
	// Used when freezing time for tests.
	// float timespeedup = mod(60.0*0.0, 120.0);

	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = atan(position.y, position.x);
	}

	float flareValue = max(min(radius
		* (pow(cos(radius * 10.0 + direction * radTime) * 0.5 + cos(- rotation * radTime - angle * branchCount) * 0.75, 2.0))
		, 1.0), 0.0);

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(bgColor, fgColor, flareValue);
}
