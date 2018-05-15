uniform float time;
uniform float branchCount;
uniform float direction;
uniform float rotation;

uniform vec2 resolution;
uniform vec2 aspect;

uniform vec4 bgColor;
uniform vec4 fgColor;
uniform vec4 pulseColor;

// Main method : entry point of the application.
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	float radTime = timespeedup * 3.1415 / 60.0;

	// Transform (x, y) into (r, a) coordinates
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0 ;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = atan(position.y, position.x);
	}
	
	// The inner slope.
	// This slope is weird. If you put both inner and outer at the same value you get a rotating circle.
	float innerSlope = 5.0;
	// The outer slope. Same rules as above.
	float outerSlope = 7.0;
	// The speed of the spiral.
	float innerSpeed = 2.0;
	// This is the value multiplier for the inner data. Setting it to more than 0.5 will make some parts disappear.
	float innerStrength = 0.5;
	// This is the value multiplier for the outer data. Setting it to more than 0.5 will make some parts disappear
	float outerStrength = 0.75;
	// Influences the slope of the spiral
	float spiralSlope = 10.0;


	float flareValue = max(min(radius
		* (pow(
			cos(radius * branchCount * spiralSlope + direction * radTime * innerSpeed - rotation * angle * innerSlope) * innerStrength
			+ cos(radTime - direction * rotation * angle * outerSlope) * outerStrength
		, 2.0))
		, 1.0), 0.0);
	float pulseValue = sin(mod(timespeedup * 0.0528 + radius * 20.0 + 2.0, 6.2832)) * 0.5 + 0.5;

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(bgColor, mix(fgColor, pulseColor, pulseValue), flareValue);
}
