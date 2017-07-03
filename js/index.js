"use strict";

/**
 * Get the requested URL parameter from the given URL, or from the current URL if no URL is given.
 * From https://stackoverflow.com/a/901144
 * @param {string} name 
 * @param {string|undefined} url 
 */
function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

/** 
 * This is used above to convert the hex value from the html5 color picker into rgb values
 * from http://esotericsoftware.com/forum/WebGL-Tinting-Slot-7693
 * @param {string} hex the hexadecimal code. Must match the #FFFFFF format.
 * @returns {{r: number, g: number, b: number}|null}
 */
function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    var resultSmall = /^#?([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16) / 255,
        g: parseInt(result[2], 16) / 255,
        b: parseInt(result[3], 16) / 255,
    } : resultSmall ? {
        r: parseInt(resultSmall[1], 16) / 15,
        g: parseInt(resultSmall[2], 16) / 15,
        b: parseInt(resultSmall[3], 16) / 15,
	} : null;
}

/**
 * adapted from http://mrdoob.com/lab/javascript/webgl/glsl/02/ by MrDoob
 * adapted from answer for http://stackoverflow.com/questions/4638317
 */
var effectDiv,
	canvas,
	canvasSize = "small",
	gl,
	buffer,
	vertex_shader,
	fragment_shader,
	currentProgram,
    vertex_position,
	capturer,
	colors = {
		fg: {},
		bg: {},
		dim: {},
		pulse: {},
	},
	parameters = {
		time: 0,
		screenWidth: 0,
		screenHeight: 0
	};

var TIME_INTERVAL = 30;
	

init()
.then(() => {
	function loopFrame() {
		parameters.time += TIME_INTERVAL;
		loop();
		capturer.capture(canvas);
	}
	setInterval(loopFrame, TIME_INTERVAL);
});

function exportGif() {
	capturer.start();
}

function init() {
	capturer = new CCapture({
		format: 'gif',
		workersPath: 'js/',
		timeLimit: 1.1,
		framerate: 60,
		verbose: false,
	});

	// These are the URI parameters.
	// Spiral shader to use.
	var spiralFile = getParameterByName("file") || "spiral";
	// Size of the canvas, either "small" (400*300) or "big" (whole screen)
	canvasSize = getParameterByName("big") !== null ? "big" : getParameterByName("canvas") || "small";
	if(getParameterByName("showButton") == null)
	{
		document.getElementById("btninfo").style.display = "none";
	}

	// These colors are stored as rgba() vectors. red, green, and blue should be between 0 and 1. Let alpha be at 1.
	colors.bg = hexToRgb(getParameterByName("bg")) || {r: 0, g: 0, b: 0};
	colors.fg = hexToRgb(getParameterByName("fg")) || {r: 1.0, g: 1.0, b: 1.0};
	colors.pulse = hexToRgb(getParameterByName("pulse")) || {r: 0.7, g: 0.3, b: 0.9};
	colors.dim = hexToRgb(getParameterByName("dim")) || {r: 0, g: 0, b: 0};

	return Promise.all([
		fetch("shaders/spiral.vs").then(r => r.text()),
		fetch("shaders/" + spiralFile + ".fs").then(r => r.text()),
	]).then(([vsText, fsText]) => {
		vertex_shader = vsText;
		fragment_shader = fsText;
	})
	.then(() => {
		effectDiv = document.getElementById('test');
		canvas = document.createElement('canvas');
		effectDiv.appendChild(canvas);

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
	if(canvasSize === "small") {
		canvas.width = 400;
		canvas.height = 300;
	}
	else if(canvasSize === "screen") {
		canvas.width = 1280;
		canvas.height = 720;
	}
	else if(canvasSize === "1080") {
		canvas.width = 1920;
		canvas.height = 1080;
	}
	else if(canvasSize === "1080") {
		canvas.width = 1600;
		canvas.height = 900;
	}
	else {
		canvas.width = window.innerWidth;
		canvas.height = window.innerHeight;
	}
	parameters.screenWidth = canvas.width;
	parameters.screenHeight = canvas.height;
	parameters.aspectX = canvas.width/canvas.height;
	parameters.aspectY = 1.0 ;
	gl.viewport(0, 0, canvas.width, canvas.height);
}

function loop() {
	if (!currentProgram) return;
	gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

	// Load program into GPU
	gl.useProgram(currentProgram);

	// Set values to program variables
	gl.uniform1f(gl.getUniformLocation(currentProgram, 'time'), parameters.time / 1000);
	gl.uniform1f(gl.getUniformLocation(currentProgram, 'branchCount'), 4);
	gl.uniform2f(gl.getUniformLocation(currentProgram, 'resolution'), parameters.screenWidth, parameters.screenHeight);
	gl.uniform2f(gl.getUniformLocation(currentProgram, 'aspect'), parameters.aspectX, parameters.aspectY);

	gl.uniform4f(gl.getUniformLocation(currentProgram, 'bgColor'), colors.bg.r, colors.bg.g, colors.bg.b, 1.0);
	gl.uniform4f(gl.getUniformLocation(currentProgram, 'fgColor'), colors.fg.r, colors.fg.g, colors.fg.b, 1.0);
	gl.uniform4f(gl.getUniformLocation(currentProgram, 'pulseColor'), colors.pulse.r, colors.pulse.g, colors.pulse.b, 1.0);
	gl.uniform4f(gl.getUniformLocation(currentProgram, 'dimColor'), colors.dim.r, colors.dim.g, colors.dim.b, 1.0);

	// Render geometry
	gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
	gl.vertexAttribPointer(vertex_position, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(vertex_position);
	gl.drawArrays(gl.TRIANGLES, 0, 6);
	gl.disableVertexAttribArray(vertex_position);
}
