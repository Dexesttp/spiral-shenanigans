#define M_PI 3.1415926535897932384626433832795
#define M_2PI 6.283185307179586476925286766559
#define M_PI_OVER_2 1.5707963267948966192313216916398

#define C_SPIRAL_SPEED 2.0

uniform float time;
uniform vec2 resolution;
uniform vec2 aspect;

uniform float branchCount;
uniform float direction;
uniform float rotation;
uniform float lowerLimit;

uniform vec4 bgColor;
uniform vec4 fgColor;


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
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	// RadTime is the time but in a [0, 2Pi[ range
	float radTime = 3.1415 * timespeedup / 60.0;
	// float radTime = M_PI;

	// Transform (x, y) into (r, a) coordinates
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = getAngle(position);
	float radius = length(position);

	mat2 rotationMatrix = mat2(cos(2. * radTime), -sin(2. * radTime), sin(2. * radTime), cos(2. * radTime));
#define MAX_VALUES 4
	vec2 dots[MAX_VALUES];
	dots[0] = vec2(0.0, 0.0);
	dots[1] = vec2(2.0, 2.0) * rotationMatrix;
	dots[2] = vec2(0.0, -3.0) * rotationMatrix;
	dots[3] = vec2(-2.0, -2.0) * rotationMatrix;
	float strengths[MAX_VALUES];
	strengths[0] = 10.0;
	strengths[1] = 0.0;
	strengths[2] = 5.0 * (0.5 * sharpSin(radTime, 0.7) + 0.5);
	strengths[3] = 5.0 * (0.5 * sharpSin(radTime, 0.7) + 0.5);

	float logVariation = 0.;
	float radiusProximity = radius;
	float minVariation = 0.0;
	for(int i = 0; i < MAX_VALUES; i++) {
		vec2 dot = dots[i];
		float strength = strengths[i];
		vec2 fgCenterA = -aspect.xy + dot * (.1 + radius * .1) + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
		float radiusA = length(fgCenterA);
		if(strength > 0.) {
			radiusProximity = min(radiusProximity, radiusA / (strength * .05));
		}
		logVariation += log(abs(radiusA - 0.1 * radius + 0.1)) * strength;
		minVariation += strength * min(1. / (radiusA * 50.0 + .1), 1.0) / 10.;
	}
	float bgOndul = 1.0 - offsetSin(
		logVariation + rotation * angle * branchCount + direction * radTime * 2.0,
		max(0.1 / (0.2 + radiusProximity), lowerLimit)
	);
	float fullValue = max(bgOndul, 1.0 - max(1.0 - minVariation, 0.0));

	gl_FragColor = mix(bgColor, fgColor, fullValue);
}
