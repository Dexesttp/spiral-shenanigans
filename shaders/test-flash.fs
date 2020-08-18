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

float getSpin(
	float radius,
	float angle,
	float radTime,
	float offset,
	float factor
) {
	return (min(
		sin(
			(
				10.0 * log(radius + 1.0)
				+ sin(5.0 * angle + offset) * sin(2.0 * radTime + factor * radius) * 0.1 * (radius + 1.0)
				+ 1.0
			) * 5.0
			+ 4.0 * radTime
			+ angle
		) + 1.95,
		1.0
	) - 0.95) * 20.0;
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

	float spinValue = getSpin(radius, - rotation * angle * branchCount, direction * radTime, 0.0, 4.0);
	float spinValue2 = getSpin(radius, - rotation * angle * branchCount, direction * radTime, M_PI_OVER_2, 2.0);

	// This is the color value at a given point of the spin
	vec4 screenWithSpiral = mix(
		mix(fgColor, pulseColor, (spinValue - spinValue2) / 2.0 + 0.5),
		bgColor,
		spinValue * spinValue2
	);

	// Add a flare in the middle of the spiral to hide the moiré effects when the spiral gets tiny.
	// The flare holds for 10% of the radius unit, and starts at -0.1.
	// 0.1 => percent of the picture used for the flare
	// -0.1 => starting offset
	float flareRadius = radius / 0.05 - 0.4;
	float flareValue = max(0.0, min(flareRadius, 1.0));

	vec4 screenWithFlare = mix(bgColor, screenWithSpiral, flareValue);


	vec4 flashScreenContentX = mix(fgColor, bgColor, 0.5 + 0.5 * sin(gl_FragCoord.x * sin(radTime * 13.0) / 5.0));
	vec4 flashScreenContentY = mix(bgColor, flashScreenContentX, 0.5 + 0.5 * sin(gl_FragCoord.y * sin(radTime * 7.0) / 5.0));
	vec4 flashScreenContentZ = mix(bgColor, flashScreenContentY, 0.5 + 0.5 * sin(gl_FragCoord.x * sin(radTime * 17.0) / 5.0));
	vec4 flashScreenContent = mix(fgColor, flashScreenContentZ, 0.5 + 0.5 * sin(gl_FragCoord.y * sin(radTime * 11.0) / 5.0));


	// The flash data.
	float flashTime = time * time / 10.0;
	float flashRawSine = 0.5 + 0.5 * sin(flashTime);
	// Varies between 0.99 and 0.9 based on time. Percent of the sine wave to consider.
	float flashPercent = 0.9 + max(min(1.0 / flashTime, 0.09), 0.0);
	// THe flash value (i.e. 0 => no flash, 1 => flash)
	// This works by :
	// - Upping the [0;1] value to [percent;percent+1]
	// - Trimming it down to [percent;1] (splitting the "percent" top half)
	// - Bringing it down to [0;1-percent] (leaving off the first few % of the value)
	// - Re-scaling it to [0;1]
	float flashValue = (min(flashRawSine + flashPercent, 1.0) - flashPercent) / (1.0 - flashPercent);
	vec4 screenWithFlash = mix(flashScreenContent, screenWithFlare, flashValue);

	float timeRadiusIncrease = sin(time / 10.0);
	vec2 pulsatingCenter = vec2(sin(time * 2.0) * 100.0 * timeRadiusIncrease, cos(time * 2.0) * 100.0 * timeRadiusIncrease);
	vec2 pulsatingPosition = -aspect.xy + 2.0 * (gl_FragCoord.xy - pulsatingCenter) / resolution.xy * aspect.xy;
	float pulsatingRadius = length(pulsatingPosition);
	float pulsatingFlareRadius = pulsatingRadius * (0.1 * sin(radTime * 4.0) + 0.2) / 0.002 - 0.4;
	float pulsatingFlareValue = max(0.0, min(pulsatingFlareRadius, 1.0));
	vec4 pulsatingFlareColor = vec4(1.0, 0.0, 0.0, 1.0);

	vec4 screen = mix(pulsatingFlareColor, screenWithFlash, pulsatingFlareValue);

	// Mix the spin vector and the flare. This is the final step.
	gl_FragColor = screen;
}
