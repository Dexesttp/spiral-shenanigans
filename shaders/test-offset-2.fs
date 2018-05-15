#define M_PI 3.1415926535897932384626433832795
#define M_2PI 6.283185307179586476925286766559
#define M_PI_OVER_2 1.5707963267948966192313216916398

#define C_inter_pattern 3.0
#define C_pattern_spin 0.75
#define C_rotation_speed 3.0
#define C_rotation_speed_2 5.0

uniform float time;
uniform float branchCount;
uniform float direction;
uniform float rotation;
uniform float offsetCenter;

uniform vec2 resolution;
uniform vec2 aspect;

uniform vec4 bgColor;
uniform vec4 fgColor;
uniform vec4 pulseColor;

float getAngle(vec2 position) {
	float angle = 0.0;
	if (position.x != 0.0 || position.y != 0.0) {
		angle = atan(position.y, position.x);
	}
	return angle;
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
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
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	// RadTime is the time but in a [0, 2Pi[ range
	float radTime = 3.1415 * timespeedup / 60.0;

	// Transform (x, y) into (r, a) coordinates
	vec2 position = -aspect.xy
		+ 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = getAngle(position);
	float radius = length(position);

	vec2 positionA = -aspect.xy
		+ vec2(-cos(radTime), sin(radTime)) * (log(1. + offsetCenter) * 1.)
		+ 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angleA = getAngle(positionA);
	float radiusA = length(positionA);

	float bgOndul = (sharpSin(
			pow(1.6, radius) * sin(radius * C_inter_pattern + (angleA - angle)) * C_rotation_speed_2
			- angle * branchCount * rotation * direction
			- radTime * C_rotation_speed * direction,
		.7 - max(radius - 2.0, .0)) * 0.5 + 0.5)
		* (1.0 - min(1. / (radius * (5.0 / branchCount) * max(resolution.x, resolution.y) / 10. + .1), 1.0))
		* (1.0 - min(1. / (radiusA * (5.0 / branchCount) * max(resolution.x, resolution.y) / 10. + .1), 1.0));

	gl_FragColor = mix(bgColor, fgColor, bgOndul);
}
