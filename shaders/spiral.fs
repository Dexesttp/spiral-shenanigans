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
	float timespeedup = 60.0*time;

	// This is the actual spiral scalar field.
	// The first formula is a "logarithmic spiral scalar field", adjusted back to radians, with a PI offset to allow for aliasing.
	// The 0.1 influences the number of slopes.
	float spinValue = mod(angle - timespeedup - 120.0*log(radius), 360.0 / branchCount) * (3.1415 / 36.0) + 3.1415;
	// The second formula is a "logarithmic spiral scalar field", adjusted back to radians, with a PI offset to allow for aliasing.
	// The 0.1 influences the number of slopes.
	// float spinValue = mod(angle - timespeedup - 250.0*radius, 360.0 / branchCount) * 0.1 + 3.1415;

	// This is the mask for the spiral. It creates this 'cool' sharding effect.
	// Set to "1.08" ~= PI/2 to remove the sharding. 0.0, 3.14 will make the whole spiral disappear.
	// float maskValue = 1.08;
	float maskValue = mod(- angle - timespeedup*4.0 - 240.0*log(radius), 360.0 / branchCount) * (3.1415 / 36.0) + 3.1415;
	// float maskValue = mod(- angle - timespeedup*2.0 - 300.0*radius, 360.0 / branchCount) * 0.1 + 3.1415;

	// This is the pulse value. It is used to alternate the fg color and the pulse color for the spiral.
	// Put to 0.0 to use the fg color only.
	float pulseValue = sin(mod(time * 17.1 / branchCount + radius * 2.0 + 2.0, 6.2832)) * 0.5 + 0.5;

	// This is the dim value. It is used for the black circle pulsing.
	// Put to 1.0 to remove the black dim. 
	// float dimValue = 1.0;
	float dimValue = sin(mod(- time * 34.2 / branchCount + radius * 5.0 + 2.0, 6.2832)) * 0.5 + 0.5;

	vec4 colorVector = mix(fgColor, pulseColor, pulseValue);
	vec4 dimmedColorVector = mix(dimColor, colorVector, sharpen(dimValue / 2.0));
	
	// Compute the actual spiral shape
	float sharpenedSpinValue = sharpen(spinValue);
	// Same thing, but for the black mask.
	float sharpenedMaskValue = sharpen(maskValue);
	// Combine the spiral and the mask.
	float maskedSpinValue = max(sharpenedSpinValue - max(sharpenedMaskValue, 0.0), 0.0);
	
	// This is the color value at a given point of the spin
	// vec4 spinVector = mix(bgColor, dimmedColorVector, sharpenedSpinValue);
	vec4 spinVector = mix(bgColor, dimmedColorVector, maskedSpinValue);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.15.
	// 0.1 => percent of the picture used for the flare
	// -0.15 => starting offset
	float flareValue = max(0.0, min(radius / 0.1 - 0.15, 1.0));

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(dimmedColorVector, spinVector, flareValue);
}
