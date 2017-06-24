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

// Main method : entry point of the application.
// NOTE
// Only have one variable set each time (if you uncomment a line, comment the alternative line)
void main(void) {
	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	// vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy + 0.5 * vec2(0.05*cos(time), 0.05*sin(time));
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0 ;
	float radius = length(position);
	if (position.x != 0.0 && position.y != 0.0){
		angle = degrees(atan(position.y,position.x)) ;
	}

	// This variable is used for time manipulation.
	// The first line speeds up from "100" , up to a maximum of "170" along a factor of "1.5". 
	// float timespeedup = (100.0 + min(1.5*time, 70.0))*time;
	// The second line doesn't speedup, and stays at a constant "60.0" spin speed.
	float timespeedup = mod(40.0*time, 120.0);

	// This is the actual spiral scalar field.
	// The first formula is a "logarithmic spiral scalar field", adjusted back to radians, with a PI offset to allow for aliasing.
	// The 0.1 influences the number of slopes.
	float radiusOffset = max(min(30.0 * tan(radius * 5.0), 100.0), -100.0);
	float dephasedRadiusOffset = - max(min(30.0 * tan(240.0 + radius * 5.0), 100.0), -100.0);

	float radiusValue = radiusOffset;
	if(abs(radiusOffset) > abs(dephasedRadiusOffset))
		radiusValue = dephasedRadiusOffset;

	float spinValue = 1.2 + 2.0 * (mod(mod(angle - timespeedup + radiusValue, 60.0), 20.0) - 10.0) / 10.0;
	float invertedSpinValue = 1.2 - 2.0 * (mod(mod(angle - timespeedup + radiusValue, 60.0), 20.0) - 10.0) / 10.0;

	float totalSpinValue = min(min(spinValue, invertedSpinValue), 1.0);

	// This is the color value at a given point of the spin
	// vec4 spinVector = mix(bgColor, dimmedColorVector, sharpenedSpinValue);
	vec4 spinVector = mix(bgColor, fgColor, totalSpinValue);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.15.
	// 0.1 => percent of the picture used for the flare
	// -0.15 => starting offset
	float flareValue = max(0.0, min(radius / 0.1 - 0.1, 1.0));

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(bgColor, spinVector, flareValue);
}
