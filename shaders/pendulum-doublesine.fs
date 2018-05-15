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

float getComputedPosition(vec2 start, vec2 end, vec2 point, float maxLength) {
	if(length(end - point) < length(start - point))
		return 0.0;
	float distanceFromLine = abs(
			(end.y - start.y) * point.x - (end.x - start.x) * point.y
			+ end.x * start.y - end.y * start.x
		) / sqrt(
			(end.y - start.y) * (end.y - start.y)
			+ (end.x - start.x) * (end.x - start.x)
		);
	if(distanceFromLine > maxLength)
		return 0.0;
	if(distanceFromLine == 0.0)
		return 1.0;
	return exp(- 1.0 / (maxLength - distanceFromLine));
}

// Main method : entry point of the application.
// NOTE
// Only have one variable set each time (if you uncomment a line, comment the alternative line)
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	float radTime = timespeedup * M_PI / 60.0;
	// Used when freezing time for tests.
	// float timespeedup = mod(60.0*0.0, 120.0);

	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 timePositionOffset = vec2(1.0*sin(timespeedup * 0.0523), 0.09 * pow(cos(timespeedup * 0.0523), 2.0));
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy + timePositionOffset;
	float angle = 0.0;
	float radius = length(position);
	if (position.x != 0.0 || position.y != 0.0) {
		angle = degrees(atan(position.y,position.x));
	}

	vec2 pendulumStart = vec2(resolution.x / 2.0, resolution.y * 2.0);
	vec2 pendulumEnd = vec2(resolution.x / 2.0, - resolution.y) - timePositionOffset * resolution.xy / aspect.xy;
	float distanceToStart = distance(gl_FragCoord.xy, pendulumStart);
	float worldLayerValueA = getComputedPosition(
		pendulumStart,
		pendulumEnd,
		gl_FragCoord.xy + vec2(mix(sin(distanceToStart / 2.0) * 5.0, 1.0, cos(3.0*radTime)), 1.0),
		3.0
	);
	float worldLayerValueB = getComputedPosition(
		pendulumStart,
		pendulumEnd,
		gl_FragCoord.xy + vec2(mix(cos(radTime + distanceToStart / 2.0) * 5.0, 1.0, sin(5.0 * radTime)), 1.0),
		3.0
	);
	
	vec4 worldLayer = mix(
		vec4(0.0, 0.0, 0.0, 1.0),
		mix(fgColor, pulseColor, (worldLayerValueA - worldLayerValueB) * 0.5 + 0.5),
		min(worldLayerValueA + worldLayerValueB, 1.0)
	);


	float spinValue = (min(cos(
		radius * 14.0 + 0.1 * sin(50.0 * radians(angle - 3.0 * radTime)) * (0.5 * sin(2.0 * radTime) + 0.5)
	) + 1.99, 1.0) - 0.99) * 100.0;
	float spinValue2 = (min(cos(
		radius * 14.0 + 3.0 * radTime + 0.3 * cos(50.0 * radians(angle - radTime)) * (0.5 * sin(7.0 * radTime) + 0.5)
	) + 1.99, 1.0) - 0.99) * 100.0;

	// This is the color value at a given point of the spin
	vec4 pendulumColor = mix(
		mix(fgColor, pulseColor, (spinValue - spinValue2) / 2.0 + 0.5),
		bgColor,
		spinValue * spinValue2
	);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareValue = max(0.0, min(radius / 0.1 - 0.4, 1.0));

	float globalAlphaValue = 0.0;
	if(radius > 0.3)
	{
		globalAlphaValue = 1.0;
	}
	else if(radius > 0.2)
	{
		float radiusValue = (radius - 0.2) * 10.0;
		float h = exp(- 1.0 / (radiusValue));
		float hp = exp(- 1.0 / (1.0 - radiusValue));
		globalAlphaValue = h / (h + hp);
	}

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = mix(pendulumColor, worldLayer, globalAlphaValue);
}
