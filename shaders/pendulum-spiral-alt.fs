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
	float angle = 0.0 ;
	if (position.x != 0.0 && position.y != 0.0){
		angle = atan(position.y, position.x);
	}
	return angle;
}

float getComputedPosition(vec2 start, vec2 end, vec2 point, float maxLength) {
	if(length(end - point) < length(start - point))
		return 0.0;
	float distanceFromLine = abs((end.y - start.y) * point.x - (end.x - start.x) * point.y + end.x * start.y - end.y * start.x)
		/ sqrt((end.y - start.y) * (end.y - start.y) + (end.x - start.x) * (end.x - start.x));
	if(distanceFromLine > maxLength)
		return 0.0;
	if(distanceFromLine == 0.0)
		return 1.0;
	return exp(- 1.0 / (maxLength - distanceFromLine));
}

float getGlobalAlpha(float radius, float limitHigh, float limitLow) {
	if(radius > limitHigh)
	{
		return 1.0;
	}
	else if(radius > limitLow)
	{
		float radiusValue = (radius - 0.2) * 10.0;
		float h = exp(- 1.0 / (radiusValue));
		float hp = exp(- 1.0 / (1.0 - radiusValue));
		return h / (h + hp);
	}
	return 0.0;
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

	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 timePositionOffset = vec2(1.0*sin(timespeedup * 0.0523), 0.09 * pow(cos(timespeedup * 0.0523), 2.0));

	// Get the position of the pendulum center, and convert it to (r, theta) coordinates.
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy + timePositionOffset;
	float radius = length(position);
	float angle = getAngle(position);

	// This is the computation for the pendulum arm. It creates a world layer with the arm.
	vec2 pendulumStart = vec2(resolution.x / 2.0, resolution.y * 2.0);
	vec2 pendulumEnd = vec2(resolution.x / 2.0, - resolution.y) - timePositionOffset * resolution.xy / aspect.xy;
	float worldLayerValue = getComputedPosition(pendulumStart, pendulumEnd, gl_FragCoord.xy, 5.0);
	// The world is black
	vec4 worldLayer = mix(vec4(0.0, 0.0, 0.0, 1.0), fgColor, worldLayerValue);

	// The commented line remove the spiral totally, allowing you to test the dim algorithm.
	// float spinValue = 1.0;
	float spinValue = sharpSin(
		log(radius) * 10.0
		+ direction * angle * branchCount
		+ rotation * 5.0 * radTime
	, 0.7 - max(radius - 0.5, .0)) * 0.5 + 0.5;

	// This is the color value at a given point of the spin
	vec4 spinVector = mix(bgColor, fgColor, spinValue);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(0.0, min(radius / 0.05 - 0.4, 1.0));

	float globalAlphaValue = getGlobalAlpha(radius, 0.3, 0.2);

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(mix(bgColor, spinVector, flareValue), worldLayer, globalAlphaValue);
}
