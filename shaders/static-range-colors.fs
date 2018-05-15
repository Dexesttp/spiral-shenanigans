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

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
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
	
	float spinValue = sharpSin(
		radius * pow(
			cos(radius * branchCount * spiralSlope + direction * radTime * innerSpeed - rotation * angle * innerSlope) * innerStrength
			+ cos(radTime - direction * rotation * angle * outerSlope) * outerStrength
		, 2.0)
	, 0.7);

	vec4 iridiscence = vec4(hsv2rgb(vec3(sharpSin(radTime - radius, 0.9) * 0.1, 1.0, 1.0)), 1.0);

	// This is the color value at a given point of the spin
	vec4 spinVector = mix(bgColor, iridiscence, spinValue);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(0.0, min(radius / 0.1 - 0.4, 1.0));

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(bgColor, spinVector, flareValue);
}
