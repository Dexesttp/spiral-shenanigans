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

float lattice(float radLog, float angle, float radTime, float offset) {
    float data = 
        radLog
        + angle
        + radTime
        + offset;
    // return sharpSin(data, 0.3) * 0.5 + 0.5;
    // return (min(sin(data) + 1.9, 1.0) - 0.9) * 10.0;
    // return (min(sharpSin(data, 0.8) + 1.8, 1.0) - 0.8) * 5.0;
    return sin(data) * 0.5 + 0.5;
}

// Main method : entry point of the application.
void main(void) {
	// This variable is used for time manipulation.
	// Stays at a constant "60.0" spin speed.
	float timespeedup = mod(60.0*time, 120.0);
	// RadTime is the time but in a [0, 2Pi[ range
	float radTime = M_PI * timespeedup / 60.0;

	// Transform (x, y) into (r, a) coordinates
	vec2 position = -aspect.xy
		+ 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = getAngle(position);
	float radius = length(position);
	float angleOffset = 0.0;

    float radLog = log(radius * 3.0 + 1.0) * 5.0;

    float latticeA = lattice(
        radLog,
        radius * 3.0 *sin(angle * branchCount - radTime) + angle,
        -radTime * 3.0,
        - radius * 50.0
    );

    float latticeB = lattice(
        radLog,
        -radius * 5.0 * cos(angle * branchCount + radTime) + angle,
        -radTime * 3.0,
        - radius * 50.0
    );

    float spiral = sin(
        log(radius + 1.0) * 50.0
        + angle * branchCount * rotation
        + 3.0 * direction * radTime
    ) * 0.5 + 0.5;

    // float latticeSelection = sharpSin(1.0 * (radLog + direction * radTime - direction * M_PI * 1.625), 0.1) * 0.5 + 0.5;
    float latticeSelection = sharpSin(1.0 * radLog, 0.7) * 0.5 + 0.5;
    // float spiralSelection = sharpSin(2.0 * (radLog + direction * radTime - direction * M_PI * 1.125), 0.7) * 0.5 + 0.5;
    float spiralSelection = sharpSin(2.0 * radLog - radTime + M_PI, 0.7) * 0.5 + 0.5;
	float flareValue = max(0.0, min(radius / 0.1 - 0.2, 1.0));
	gl_FragColor = mix(
        bgColor,
        mix(
            mix(
                bgColor, fgColor, spiral
            ),
            mix(
                mix(fgColor, pulseColor, latticeA),
                mix(fgColor, dimColor, latticeB),
                latticeSelection
            ),
            spiralSelection // * (0.5 * cos(radius * 2.0) + 0.5)
        ),
        flareValue
    );
}