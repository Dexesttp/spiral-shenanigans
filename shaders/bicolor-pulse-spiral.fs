uniform float time;
uniform float branchCount;
uniform vec2 resolution;
uniform vec2 aspect;

void main(void) {
	// Transform (x, y) into (r, a) coordinates based on (0, 0) defined as below
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy;
	float angle = 0.0 ;
	float radius = length(position) ;
	if (position.x != 0.0 && position.y != 0.0){
		angle = degrees(atan(position.y,position.x)) ;
	}

	// Colors, to extract on the long term
	vec4 pulseColor = vec4(1.0, 0.6, 0.7, 1.0);
	vec4 bgColor = vec4(0.0, 0.0, 0.0, 1.0);
	vec4 fgColor = vec4(0.5, 0.0, 0.0, 1.0);

	float timespeedup = 60.0*time;
	float spinValue = mod(angle - timespeedup - 120.0*log(radius), 360.0 / branchCount) * 0.1 + 3.141;
	float pulseValue = sin(time * 17.1 / branchCount + radius * 2.0 + 2.0) * 0.5 + 0.5;
	float dimValue = sin(- time * 34.2 / branchCount + radius * 5.0 + 2.0) * 0.5 + 0.5;
	vec4 colorVector = mix(fgColor, pulseColor, pulseValue);
	vec4 dimmedColorVector = mix(colorVector, bgColor, dimValue);
	float sharpenedSpinValue = min(sin(spinValue)
		+ sin(3.0 * spinValue) / 3.0
		+ sin(5.0 * spinValue) / 5.0
		+ sin(7.0 * spinValue) / 7.0, 0.7) * 1.3;
	vec4 spinVector = mix(bgColor, dimmedColorVector, sharpenedSpinValue);
	float flareValue = max(0.0, min(radius / 0.1 - 0.15, 1.0));
	gl_FragColor = mix(dimmedColorVector, spinVector, flareValue);
}
