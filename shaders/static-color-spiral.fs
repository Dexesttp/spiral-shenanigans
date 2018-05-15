uniform float time;
uniform vec2 resolution;
uniform vec2 aspect;

void main(void) {
	float timespeedup = mod(60.0*time, 120.0);

	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0;
	float radius = length(position) ;
	if (position.x != 0.0 || position.y != 0.0) {
		angle = degrees(atan(position.y, position.x));
	}

	float pulseTiming = timespeedup * 0.0528 + radius * 5.0;
	float pulseValue = sin(pulseTiming) * 0.5 + 0.5;
	vec4 pulseColor = vec4(sin(pulseTiming), sin(pulseTiming + radius * 5.0), sin(pulseTiming + radius * 10.0), 1.0);
	vec4 colorVector = mix(vec4(1.0, 1.0, 1.0, 1.0), pulseColor, pulseValue);
	float spinValue = mod(angle - timespeedup * 1.5 - 120.0*log(radius), 90.0) * 0.1 + 3.141;
	float sharpenedSpinValue = min(sin(spinValue)
		+ sin(3.0 * spinValue) / 3.0
		+ sin(5.0 * spinValue) / 5.0, 0.7) * 1.3;
	vec4 spinVector = mix(vec4(0.0, 0.0, 0.0, 1.0), colorVector, sharpenedSpinValue);
	float flareValue = max(0.0, min(radius / 0.1 - 0.2, 1.0));
	gl_FragColor = mix(vec4(0.0, 0.0, 0.0, 1.0), spinVector, flareValue);
}
