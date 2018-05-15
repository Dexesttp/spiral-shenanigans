#define M_PI 3.1415926535897932384626433832795
#define M_2PI 6.283185307179586476925286766559
#define M_PI_OVER_2 1.5707963267948966192313216916398

#define MAX_VALUES 4
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

float computeValue(
	float angle,
	float radius,
	float radTime,
	vec2[MAX_VALUES] dots,
	float[MAX_VALUES] strengths
) {
	float logVariation = 0.0;
	float minVariation = 0.0;
	for(int i = 0; i < MAX_VALUES; i++) {
		vec2 dot = dots[i];
		float strength = strengths[i];
		vec2 fgCenterA = -aspect.xy + dot * (.1 + radius * .1) + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
		float angleA = getAngle(fgCenterA);
		float radiusA = length(fgCenterA);
		logVariation += log(abs(10.0 * radiusA - radius * 0.2 + 0.1)) * 10.0;
		minVariation += min(1. / (radiusA * 80.0 * (sin(radTime) * 0.5 + 1.0) + .1), 1.0);
	}
	float bgOndul = sharpSin(logVariation - radTime * 2.0 + angle * 6.0, 0.7) * 0.5 + 0.5;
	return bgOndul * max(1.0 - minVariation, 0.0);
}

// Main method : entry point of the application.
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	// RadTime is the time but in a [0, 2Pi[ range
	float radTime = 3.1415 * timespeedup / 60.0;
	// float radTime = 0.0;

	// Transform (x, y) into (r, a) coordinates
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = getAngle(position);
	float radius = length(position);

	// mat2 rotationMatrix = mat2(cos(radTime), -sin(radTime), sin(radTime), cos(radTime));

	vec2 dots[MAX_VALUES];
	dots[0] = vec2(0.0, 0.0);
	dots[1] = vec2(1.0, 3.0) * 1.5;
	dots[2] = vec2(0.0, 0.0);
	dots[3] = vec2(1.0, -3.0) * 2.0;
	float strengths[MAX_VALUES];
	strengths[0] = 1.0;
	strengths[1] = 1.0;
	strengths[2] = 1.0;
	strengths[3] = 1.0;
	float fullValue = computeValue(angle, radius, radTime, dots, strengths);

	gl_FragColor = mix(bgColor, fgColor, fullValue);
}
