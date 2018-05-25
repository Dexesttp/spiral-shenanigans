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

mat2 getMatrix(float angle) {
	return mat2(cos(angle), -sin(angle), sin(angle), cos(angle));
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

#define MAX_VALUES 21
	vec2 dots[MAX_VALUES];
	vec2 dotStart = vec2(3.0, 0.0);
	float offset = -2.0 * radTime;
	vec2 dotStart2 = vec2(3.5, 3.5);
	float offset2 = -1.0 * radTime;
	vec2 dotStart3 = vec2(0.5, 4.0);
	float offset3 = 1.0 * radTime;
	dots[0] = vec2(0.0, 0.0);
	dots[1] = dotStart * getMatrix(offset + M_PI * 0.);
	dots[2] = dotStart * getMatrix(offset + M_PI * 0.25);
	dots[3] = dotStart * getMatrix(offset + M_PI * 0.5);
	dots[4] = dotStart * getMatrix(offset + M_PI * 0.75);
	dots[5] = dotStart * getMatrix(offset + M_PI * 1.);
	dots[6] = dotStart * getMatrix(offset + M_PI * 1.25);
	dots[7] = dotStart * getMatrix(offset + M_PI * 1.5);
	dots[8] = dotStart * getMatrix(offset + M_PI * 1.75);
	dots[9] = dotStart2 * getMatrix(offset2 + M_PI * 0.);
	dots[10] = dotStart2 * getMatrix(offset2 + M_PI * 0.25);
	dots[11] = dotStart2 * getMatrix(offset2 + M_PI * 0.5);
	dots[12] = dotStart2 * getMatrix(offset2 + M_PI * 0.75);
	dots[13] = dotStart2 * getMatrix(offset2 + M_PI * 1.);
	dots[14] = dotStart2 * getMatrix(offset2 + M_PI * 1.25);
	dots[15] = dotStart2 * getMatrix(offset2 + M_PI * 1.5);
	dots[16] = dotStart2 * getMatrix(offset2 + M_PI * 1.75);
	dots[17] = dotStart3 * getMatrix(offset3 + M_PI * 0.);
	dots[18] = dotStart3 * getMatrix(offset3 + M_PI * 0.25);
	dots[19] = dotStart3 * getMatrix(offset3 + M_PI * 1.);
	dots[20] = dotStart3 * getMatrix(offset3 + M_PI * 1.25);
	float strengths[MAX_VALUES];
	float commonStrength = 4.0;
	float commonStrength2 = 2.0;
	float commonStrength3 = 2.0;
	strengths[0] = 10.0;
	strengths[1] = commonStrength;
	strengths[2] = commonStrength;
	strengths[3] = commonStrength;
	strengths[4] = commonStrength;
	strengths[5] = commonStrength;
	strengths[6] = commonStrength;
	strengths[7] = commonStrength;
	strengths[8] = commonStrength;
	strengths[9] = commonStrength2;
	strengths[10] = commonStrength2;
	strengths[11] = commonStrength2;
	strengths[12] = commonStrength2;
	strengths[13] = commonStrength2;
	strengths[14] = commonStrength2;
	strengths[15] = commonStrength2;
	strengths[16] = commonStrength2;
	strengths[17] = commonStrength3;
	strengths[18] = commonStrength3;
	strengths[19] = commonStrength3;
	strengths[20] = commonStrength3;

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
