uniform float time;
uniform float branchCount;
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
	float offsetedRadius = radius + timespeedup * 0.00528 * 2.0;
	// float offsetedRadius = radius;

	// The radus offset (and dephased radius offset) are values to know when to start the main and the flipped values.
	// The 30.0 is the slope of the spiral (more = more slopes)
	// the 5.0 is the size of each band (more = more bands) 
	float radiusOffset = max(min(30.0 * tan(offsetedRadius * 5.0), 100.0), -100.0);
	float dephasedRadiusOffset = - max(min(30.0 * tan(240.0 + offsetedRadius * 5.0), 100.0), -100.0);

	// Check which is the greatest value, and apply it to the algorithm.
	float radiusValue = radiusOffset;
	if(abs(radiusOffset) > abs(dephasedRadiusOffset)) {
		radiusValue = 1.0;
	}

	// Compute the spin and inverted spin value.
	// We will combine the two based on the minimum value of both.
	float spinValue = 1.2 + 2.0 * (mod(mod(angle + timespeedup + radiusValue, 60.0), 20.0) - 10.0) / 10.0;
	float invertedSpinValue = 1.2 - 2.0 * (mod(mod(angle + timespeedup + radiusValue, 60.0), 20.0) - 10.0) / 10.0;

	// This is the final value.
	return min(min(spinValue, invertedSpinValue), 1.0);
}

float getComputedPosition(vec2 start, vec2 end, vec2 point, float maxLength) {
	if(length(end - point) < length(start - point))
		return 0.0;
	float distanceFromLine = abs((end.y - start.y) * point.x - (end.x - start.x) * point.y + end.x * start.y - end.y * start.x)
		/ sqrt((end.y - start.y) * (end.y - start.y) + (end.x - start.x) * (end.x - start.x));
	if(distanceFromLine > maxLength)
		return 0.0;
	if(distanceFromLine == 0.0)
		return 1.0;
	return exp(- 1.0 / (maxLength - distanceFromLine));
}

// Main method : entry point of the application.
// NOTE
// Only have one variable set each time (if you uncomment a line, comment the alternative line)
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	// Used when freezing time for tests.
	// float timespeedup = mod(60.0*0.0, 120.0);

	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 timePositionOffset = vec2(cos(timespeedup * 0.0528), sin(timespeedup * 0.0528));
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	//vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy + 0.25 * timePositionOffset;
	float angle = 0.0;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = atan(position.y, position.x);
	}

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(min(cos(radius * 5.0 * cos(radius * 10.0 + timespeedup * 3.1415 / 60.0)) * cos(angle + timespeedup * 3.1415 / 60.0) , 1.0), 0.0);

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(bgColor, fgColor, flareValue);
}
