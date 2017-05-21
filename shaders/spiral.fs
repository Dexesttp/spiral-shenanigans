uniform float time;
uniform vec2 resolution;
uniform vec2 aspect;

void main(void) {
	vec2 position = -aspect.xy + 2.0 * gl_FragCoord.xy / resolution.xy * aspect.xy + 0.5 * vec2(0.05*cos(time), 0.05*sin(time));
	float angle = 0.0 ;
	float radius = length(position) ;
	if (position.x != 0.0 && position.y != 0.0){
		angle = degrees(atan(position.y,position.x)) ;
	}
	float timespeedup = 50.0*time + time*min(0.7*time, 30.0);
	// float timespeedup = 60.0*time;
	// float timespeedup = 0.0;
	float pulseValue = 0.0;
	if(mod(time, 5.0) < 3.14) {
		float pulseFactor = -pow(cos(mod(time, 5.0) - 1.07), 2.0);
		float radiusFactor = exp(- pow(pulseFactor * 2.0 - radius, 2.0) * 0.1);
		pulseValue = radiusFactor * pulseFactor;
	}
	vec4 colorVector = mix(vec4(0.4, 0.8, 1.0, 1.0), vec4(0.0, 0.0, 0.5, 1.0), pulseValue);
	float spinValue = exp( - pow(mod(angle - timespeedup - 120.0*log(radius), 30.0) * 0.3, 2.0) * 0.01);
	vec4 spinVector = mix(colorVector, vec4(0.2, 0.3, 0.6, 1.0), spinValue);
	float flareValue = max(0.0, min(radius / 2.0 - 0.02, 1.0));
	gl_FragColor = mix(vec4(0.2, 0.2, 0.4, 0.5), spinVector, flareValue);
}
