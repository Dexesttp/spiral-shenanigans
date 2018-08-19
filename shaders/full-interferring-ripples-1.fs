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

float offsetSin(float inputValue, float percent) {
	return (min(sin(inputValue) + 2.0 - percent, 1.0) - 1.0 + percent) * (1.0 / percent);
}

float getSpin(
	float radius,
	float angle,
	float radTime,
	float offset,
	float factor,
	float activation
) {
	return offsetSin(
			(
				10.0 * log(radius + 1.0)
				+ sin(5.0 * angle + offset) * sin(2.0 * radTime + factor * radius + offset) * 0.05 * (radius + 2.0) * activation
				+ 1.0
			) * 5.0
			+ 1.0 * radTime
			+ angle
		, 0.3
	);
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

	float spinValue1 = offsetSin(15.0 * radius - 3. * dr + rab, 0.3);
	float spinValue3 = offsetSin(11.0 * radius - 2. * dr + rab, 0.3);
	float spinValue = max(min(
		spinValue1 * sin(2. * radTime + radius * 5. + M_PI_OVER_2 * 1.)
		+ spinValue3 * sin(radTime + radius * 7. + M_PI_OVER_2 * 3.)
		, 1.), 0.);

	// This is the color value at a given point of the spin
	vec4 spinVector = mix(
		bgColor,
		fgColor,
		spinValue
	);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(0., min(radius / 0.05 - 0.4, 1.));
	float flareValue2 = max(0., min(1. - log(1. + radius * 1.8), 1.));

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(bgColor, mix(bgColor, spinVector, flareValue), flareValue2);
}
