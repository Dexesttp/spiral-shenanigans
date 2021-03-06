#define M_PI 3.1415926535897932384626433832795
#define M_2PI 6.283185307179586476925286766559
#define M_PI_OVER_2 1.5707963267948966192313216916398


#define C_SPIRAL_SPEED 2.0

uniform float time;
uniform float branchCount;
uniform float direction;
uniform float rotation;

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

vec3 hsv2rgb(vec3 c)
{
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
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = getAngle(position);
	float radius = length(position);
	
	vec2 fgCenterA = -aspect.xy + vec2(-2.0, 0.0) * (.1 + radius * .1) + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angleA = getAngle(fgCenterA);
	float radiusA = length(fgCenterA);
	
	vec2 fgCenterB = -aspect.xy + vec2(2.0, 0.0) * (.1 + radius * .1) + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angleB = getAngle(fgCenterB);
	float radiusB = length(fgCenterB);

	float bgOndul = sharpSin(
			log(abs(10.0 * radiusA - radius * sin(radTime) + 0.1)) * 10.0
			+ log(abs(10.0 * radiusB - radius * sin(-radTime) + 0.1)) * 10.0
			+ angle * 6.0
			- radTime * 6.0
		, .7)
		* 0.5 + 0.5;
	float fullValue = bgOndul * max(1.0
		- min(1. / (radiusA * 80.0 * (sin(-radTime) * 0.5 + 1.0) + .1), 1.0)
		- min(1. / (radiusB * 80.0 * (sin(radTime) * 0.5 + 1.0) + .1), 1.0)
		, 0.0
	);

	gl_FragColor = mix(bgColor, fgColor, fullValue);
}
