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
	float distanceFromLine = abs((end.y - start.y) * point.x - (end.x - start.x) * point.y + end.x * start.y - end.y * start.x)
		/ sqrt((end.y - start.y) * (end.y - start.y) + (end.x - start.x) * (end.x - start.x));
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
	float radTime = timespeedup * 3.1415 / 60.0;
	// Used when freezing time for tests.
	// float timespeedup = mod(60.0*0.0, 120.0);

	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 timePositionOffset = 0.5 * vec2(1.5*cos(timespeedup * 0.0528), 0.4*sin(timespeedup * 0.0528)*sin(timespeedup * 0.0528));
	//vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy + timePositionOffset;
	float angle = 0.0 ;
	float radius = length(position);
	if (position.x != 0.0 && position.y != 0.0){
		angle = degrees(atan(position.y,position.x));
	}

	vec2 pendulumStart = vec2(resolution.x / 2.0, resolution.y * 2.0);
	vec2 pendulumEnd = vec2(resolution.x / 2.0, - resolution.y) - timePositionOffset * resolution.xy / aspect.xy;
	float worldLayerValue = getComputedPosition(pendulumStart, pendulumEnd, gl_FragCoord.xy, 5.0);
	
	vec4 worldLayer = mix(vec4(0.0, 0.0, 0.0, 1.0), fgColor, worldLayerValue);

	// The commented line remove the spiral totally, allowing you to test the dim algorithm.
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


	float totalSpinValue = max(min(radius
		* (pow(
			cos(radius * branchCount * spiralSlope + direction * radTime * innerSpeed - rotation * radians(angle) * innerSlope) * innerStrength
			+ cos(radTime - direction * rotation * radians(angle) * outerSlope) * outerStrength
		, 2.0))
		, 1.0), 0.0);
	// float totalSpinValue = 1.0;

	// This is the pulse value (pulses a new color in the foreground color).
	// For now, it's just a wavey circle. We can make it smaller / sharpen one side / make it slower as it gets further later.
	float pulseValue = sin(timespeedup * 0.0528 + log(radius) * 5.0 + 0.2 * sin(3.14 * angle / 15.0 + sin(radius * 10.0))) * 0.5 + 0.5;
	// float pulseValue = 0.0;

	// This is the color value at a given point of the spin
	vec4 processedFgColor = mix(fgColor, pulseColor, pulseValue);
	vec4 spinVector = mix(bgColor, processedFgColor, totalSpinValue);

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
	gl_FragColor = mix(mix(bgColor, spinVector, flareValue), worldLayer, globalAlphaValue);
}
