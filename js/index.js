"use strict";
/**
 * adapted from http://mrdoob.com/lab/javascript/webgl/glsl/02/ by mrdoob
 * adapted from answer for http://stackoverflow.com/questions/4638317
 */

var effectDiv,
	canvas,
	gl,
	buffer,
	vertex_shader,
	fragment_shader,
	currentProgram,
    vertex_position,
	capturer,
	parameters = {
		start_time: new Date().getTime(),
		time: 0,
		screenWidth: 0,
		screenHeight: 0
	};
	

init()
.then(() => {
	function loopFrame() {
		loop();
		capturer.capture(canvas);
		requestAnimationFrame(loopFrame);
	}
	requestAnimationFrame(loopFrame);
});

function exportGif() {
	capturer.start();
}

function init() {
	capturer = new CCapture({
		format: 'gif',
		workersPath: 'js/',
		timeLimit: 5,
		framerate: 30,
		verbose: false,
	});
	return Promise.all([
		fetch("shaders/spiral.vs").then(r => r.text()),
		fetch("shaders/spiral.fs").then(r => r.text()),
	]).then(([vsText, fsText]) => {
		vertex_shader = vsText;
		fragment_shader = fsText;
	})
	.then(() => {
		effectDiv = document.getElementById('test');
		canvas = document.createElement('canvas');
		effectDiv.appendChild(canvas );

		// Initialize WebGL
		try {
			gl = canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
		}
		catch(e) { }
		if(!gl) {
			alert("WebGL not supported");
			throw "cannot create webgl context";
		}

		// Create Vertex buffer (2 triangles)
		buffer = gl.createBuffer();
		gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
		gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([-1.0, -1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, 1.0]), gl.STATIC_DRAW);

		// Create Program
		currentProgram = createProgram(vertex_shader, fragment_shader);

		onWindowResize();
		window.addEventListener("resize", onWindowResize, false);
	});
}

function createProgram(vertex, fragment) {
	var program = gl.createProgram();
	var vs = createShader(vertex, gl.VERTEX_SHADER);
	var fs = createShader("#ifdef GL_ES\nprecision highp float;\n#endif\n\n" + fragment, gl.FRAGMENT_SHADER);
	if (vs == null || fs == null) return null;
	gl.attachShader(program, vs);
	gl.attachShader(program, fs);
	gl.deleteShader(vs);
	gl.deleteShader(fs);
	gl.linkProgram(program);
	if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
		alert("ERROR:\n" +
		"VALIDATE_STATUS: " + gl.getProgramParameter(program, gl.VALIDATE_STATUS) + "\n" +
		"ERROR: " + gl.getError() + "\n\n" +
		"- Vertex Shader -\n" + vertex + "\n\n" +
		"- Fragment Shader -\n" + fragment);
		return null;
	}
	return program;
}

function createShader(src, type) {
	var shader = gl.createShader(type);
	gl.shaderSource(shader, src);
	gl.compileShader(shader);
	if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
		alert((type == gl.VERTEX_SHADER ? "VERTEX" : "FRAGMENT" ) + " SHADER:\n" + gl.getShaderInfoLog(shader));
		return null;
	}
	return shader;
}

function onWindowResize(event) {
	canvas.width = window.innerWidth;
	canvas.height = window.innerHeight;
	parameters.screenWidth = canvas.width;
	parameters.screenHeight = canvas.height;
	parameters.aspectX = canvas.width/canvas.height ;
	parameters.aspectY = 1.0 ;
	gl.viewport(0, 0, canvas.width, canvas.height);
}

function loop() {
	if (!currentProgram) return;
	parameters.time = new Date().getTime() - parameters.start_time;
	gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

	// Load program into GPU
	gl.useProgram(currentProgram);

	// Set values to program variables
	gl.uniform1f(gl.getUniformLocation(currentProgram, 'time'), parameters.time / 1000);
	gl.uniform2f(gl.getUniformLocation(currentProgram, 'resolution'), parameters.screenWidth, parameters.screenHeight);
	gl.uniform2f(gl.getUniformLocation(currentProgram, 'aspect'), parameters.aspectX, parameters.aspectY);

	// Render geometry
	gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
	gl.vertexAttribPointer(vertex_position, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(vertex_position);
	gl.drawArrays(gl.TRIANGLES, 0, 6);
	gl.disableVertexAttribArray(vertex_position);
}
