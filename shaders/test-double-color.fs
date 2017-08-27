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


float getAngle(vec2 position) {
	float angle = 0.0 ;
	if (position.x != 0.0 && position.y != 0.0){
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

	float spiralValue = sin(- 6.0 * radTime + 6.0 * angle + 20.0 * radius) * 0.5 + 0.5;
	float invertedSpiralValue = sin(- 6.0 * radTime - 6.0 * angle + 20.0 * radius) * 0.5 + 0.5;
	float circleValue = sharpSin(5.0 * radius - radTime, 0.3) * 0.5 + 0.5;
	// float circleValue = sin(5.0 * radius - radTime) * 0.5 + 0.5;

	vec2 fgCenterL = -aspect.xy + vec2(-cos(radTime), sin(radTime)) * .3 + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float fgAngleL = getAngle(fgCenterL);
	float linesValueL = sin(4.0 * radTime + 2.0 * fgAngleL) * 0.05 + 0.5;
	float radValueL = sin(- 1.0 * radTime + 5.0 * radius) * 0.3;
	vec4 newFgL = vec4(hsv2rgb(vec3(1.0 - linesValueL + radValueL, 0.7, 1.0)), 1.0);
	vec2 fgCenterR = -aspect.xy + vec2(cos(radTime), sin(radTime)) * (.6 + radius * .1) + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float fgAngleR = getAngle(fgCenterR);
	float linesValueR = sin(4.0 * radTime + 10.0 * fgAngleR) * 0.05 + 0.7;
	float radValueR = sin(- 1.0 * radTime + 5.0 * radius) * 0.2;
	vec4 newFgR = vec4(hsv2rgb(vec3(1.0 - linesValueR + radValueR, 0.7, 1.0)), 1.0);
	vec4 newFg = mix(newFgL, newFgR, 0.5);

	// vec4 newBg = vec4(hsv2rgb(vec3(linesValue, 0.3, 1.0)), 1.0);
	vec4 newBg = bgColor;

	// Mix the spin vector and the flare. This is the final step.
	vec4 spiralColor = mix(newBg, newFg, spiralValue);
	vec4 invertedSpiralColor = mix(newBg, newFg, invertedSpiralValue);
	vec4 fullColor = mix(spiralColor, invertedSpiralColor, circleValue);
	vec4 bgColor = mix(newBg, newFg, 0.5);
	float centerValue = min(1.0 / (radius * 20.0 + 0.5), 1.0);
	gl_FragColor = newFg;
}
