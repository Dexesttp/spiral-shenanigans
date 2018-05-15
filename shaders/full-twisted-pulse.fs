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

// Sharpen a gradual value into a square-like value split at 0.5, keeping anti-aliasing as needed.
float sharpen(float value) {
	// IDEA : square(spin) + antialiasing
	// SOLUTION : 3-step fourrier approximation of the square function, maxed to "1.0"
	return min(sin(value) + sin(3.0 * value) / 3.0 + sin(5.0 * value) / 5.0 + sin(7.0 * value) / 7.0, 0.7) * 1.3;
}

float shrinkTo(float value, float limit) {
	if(value > limit)
		return (value - limit) * (1.0 / (1.0 - limit));
	else
		return 0.0;
}

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
// NOTE
// Only have one variable set each time (if you uncomment a line, comment the alternative line)
void main(void) {
	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	// vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy + 0.5 * vec2(0.1*cos(time), 0.1*sin(time));
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = degrees(atan(position.y, position.x));
	}

	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);

	// float globalAlphaValue = shrinkTo(0.45 * sin(timespeedup * 0.0528) + 0.5, 0.5);
	float globalAlphaValue = 0.0;

	// The commented line remove the spiral totally, allowing you to test the dim algorithm.
	float totalSpinValue = getSpin(angle, radius, timespeedup);
	// float totalSpinValue = 1.0;

	// This is the pulse value (pulses a new color in the foreground color).
	// For now, it's just a wavey circle. We can make it smaller / sharpen one side / make it slower as it gets further later.
	float pulseValue = sin(timespeedup * 0.0528 + log(radius) * 5.0 + 0.2 * sin(3.14 * angle / 15.0 + sin(radius * 10.0))) * 0.5 + 0.5;
	// float pulseValue = 0.0;

	// This is the dim value (dims the whole spiral)
	float dimValue = sin(- timespeedup * 0.0528 + log(radius) * 5.0) * 0.5 + 0.5;
	// float dimValue = 0.0;
	dimValue = shrinkTo(dimValue, 0.95);

	// This is the color value at a given point of the spin
	vec4 processedBgColor = mix(bgColor, dimColor, dimValue);
	vec4 processedFgColor = mix(mix(fgColor, pulseColor, pulseValue), dimColor, dimValue);
	vec4 spinVector = mix(processedBgColor, processedFgColor, totalSpinValue);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(0.0, min(radius / 0.1 - 0.1, 1.0));

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(mix(bgColor, spinVector, flareValue), vec4(1.0, 0.0, 1.0, 1.0), globalAlphaValue);
}
