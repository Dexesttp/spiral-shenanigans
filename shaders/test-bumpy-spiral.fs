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

	float posValue = branchCount * angle + 1.0 * radTime + 5.0 * radius;
	float invertedPosValue = branchCount * angle - 1.0 * radTime - 5.0 * radius;

	float leftValue = max(sharpSin(posValue, 0.1), 0.0) * max(sharpSin(posValue + M_PI_OVER_2, 0.1), 0.0);
	float rightValue = max(sharpSin(invertedPosValue, 0.1), 0.0) * max(sharpSin(invertedPosValue + M_PI_OVER_2, 0.1), 0.0);

	float mixedValue = mix(rightValue, leftValue, sharpSin(radius * 10.0 + sin(radTime) + M_PI_OVER_2, 0.1) * 0.5 + 0.5);

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(
		bgColor,
		fgColor,
		mixedValue
	);
}