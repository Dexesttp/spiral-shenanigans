#define M_PI 3.1415926535897932384626433832795
#define M_2PI 6.283185307179586476925286766559
#define M_PI_OVER_2 1.5707963267948966192313216916398

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

float getAngle(vec2 position) {
	float angle = 0.0;
	if (position.x != 0.0 || position.y != 0.0) {
		angle = atan(position.y, position.x);
	}
	return angle;
}

float sharpSin(float inputValue, float percent) {
	float value = mod(inputValue, M_2PI);
	if(value < M_PI_OVER_2 * percent)
		return sin(value / percent);
	if(value < M_PI - M_PI_OVER_2 * percent)
		return 1.0;
	if(value < M_PI + M_PI_OVER_2 * percent)
		return sin(value / percent + M_PI - (M_PI / percent));
	if(value < M_2PI - M_PI_OVER_2 * percent)
		return - 1.0;
	return sin(value / percent + M_2PI - (M_2PI / percent));
}

float offsetSin(float inputValue, float percent) {
	return (min(sin(inputValue) + 2.0 - percent, 1.0) - 1.0 + percent) * (1.0 / percent);
}

// Main method : entry point of the application.
// NOTE
// Only have one variable set each time (if you uncomment a line, comment the alternative line)
void main(void) {
	// This variable is used for time manipulation.
	float timespeedup = mod(60.0*time, 120.0);
	// RadTime is the time but in a [0, 2Pi[ range
	float radTime = M_PI * timespeedup / 60.0;

	// Get the position of the pendulum center, and convert it to (r, theta) coordinates.
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float radius = length(position);
	float angle = getAngle(position);
	float rab =  - rotation * angle * branchCount;
	float dr = direction * radTime;

	float spiral1 = offsetSin(100. * log(radius + 2.0) + rab + dr, 0.1);
	float spiral2 = offsetSin(100. * log(radius + 2.0) + rab - 2. * dr, 0.1);
	float selector = sharpSin(radius * 15. + dr, .1) * .5 + .5;

	// This is the color value at a given point of the spin
	vec4 spinVector = mix(
		fgColor,
		bgColor,
		mix(spiral1, spiral2, selector)
	);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(0., min(radius / 0.05 - 0.4, 1.));
	vec4 spiral = mix(fgColor, spinVector, flareValue);

	float circle1 = offsetSin(5. * log(radius + 1.) - radTime + .0, 0.02);
	float circle2 = offsetSin(5. * log(radius + 1.) - radTime + 1.5, 0.02);
	float circle3 = offsetSin(5. * log(radius + 1.) - radTime + 3., 0.02);


	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(pulseColor, spiral, circle1 * circle2 * circle3);
}
