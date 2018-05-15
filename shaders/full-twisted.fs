uniform float time;
uniform float branchCount;
uniform float direction;
uniform float rotation;

uniform vec2 resolution;
uniform vec2 aspect;

uniform vec4 bgColor;
uniform vec4 fgColor;
uniform vec4 pulseColor;

// -------------------
// Spiral scalar field
// -------------------
float getSpin(float angle, float radius, float timespeedup) {
	// The radus offset (and dephased radius offset) are values to know when to start the main and the flipped values.
	// the 5.0 is the size of each band (more = more bands)
	float twistRadius = 5.0;
	// The 30.0 is the slope of the spiral (more = more slopes)
	float twistSlope = 30.0;
	float radiusOffset = max(min(twistSlope * tan(radius * twistRadius), 100.0), -100.0);
	float dephasedRadiusOffset = - max(min(twistSlope * tan(240.0 + radius * twistRadius), 100.0), -100.0);

	// Check which is the greatest value, and apply it to the algorithm.
	float radiusValue = radiusOffset;
	if(abs(radiusOffset) > abs(dephasedRadiusOffset))
		radiusValue = dephasedRadiusOffset;

	// Compute the spin and inverted spin value.
	// We will combine the two based on the minimum value of both.
	float angleValue = angle * rotation * branchCount / 18.0;
	float spinValue = 1.2 + 2.0 * (mod(mod(angleValue - direction * rotation * timespeedup + radiusValue, 60.0), 20.0) - 10.0) / 10.0;
	float invertedSpinValue = 1.2 - 2.0 * (mod(mod(angleValue - direction * rotation * timespeedup + radiusValue, 60.0), 20.0) - 10.0) / 10.0;

	// This is the final value.
	return min(min(spinValue, invertedSpinValue), 1.0);
}

// Main method : entry point of the application.
void main(void) {
	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = degrees(atan(position.y, position.x));
	}

	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);

	// The commented line remove the spiral totally, allowing you to test the dim algorithm.
	float totalSpinValue = getSpin(angle, radius, timespeedup);

	// This is the pulse value (pulses a new color in the foreground color).
	// For now, it's just a wavey circle. We can make it smaller / sharpen one side / make it slower as it gets further later.
	float pulseValue = sin(timespeedup * 0.0528 + log(radius) * 5.0 + 0.2 * sin(3.14 * angle / 15.0 + sin(radius * 10.0))) * 0.5 + 0.5;

	// This is the color value at a given point of the spin
	vec4 processedFgColor = mix(fgColor, pulseColor, pulseValue);
	vec4 spinVector = mix(bgColor, processedFgColor, totalSpinValue);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(0.0, min(radius / 0.1 - 0.1, 1.0));

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(bgColor, spinVector, flareValue);
}
