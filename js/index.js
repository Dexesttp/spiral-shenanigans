"use strict";
// @ts-check

/**
 * Interval between two frames.
 */
var TIME_INTERVAL = 30;



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
    var result = /^([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    var resultSmall = /^([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})$/i.exec(hex);
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

function getConfigFromURL() {
	return {
		shaderFileName: getParameterByName("file") || "spiral",
		// Size of the canvas, either "small" (400*300) or "big" (whole screen)
		canvasSize: getParameterByName("big") !== null ? "big" : getParameterByName("canvas") || "small",
		speedFactor: +getParameterByName("speed") || 1,
		rotation: getParameterByName("counterclockwise") === null ? 1 : -1,
		direction: getParameterByName("inwards") === null ? -1 : 1,
		branchCount: +getParameterByName("branch") || 4,
		showButton: getParameterByName("showButton") !== null,
		exportName: getParameterByName("exportName") || undefined,
		keyboardControls: getParameterByName("keyboardControls") !== null,
		// These colors are stored as rgba() vectors. red, green, and blue should be between 0 and 1. Let alpha be at 1.
		colors: {
			bg: hexToRgb(getParameterByName("bg")) || {r: 0, g: 0, b: 0},
			fg: hexToRgb(getParameterByName("fg")) || {r: 1.0, g: 1.0, b: 1.0},
			pulse: hexToRgb(getParameterByName("pulse")) || {r: 0.7, g: 0.3, b: 0.9},
			dim: hexToRgb(getParameterByName("dim")) || {r: 0, g: 0, b: 0},
		},
	};
}

/*
 * adapted from http://mrdoob.com/lab/javascript/webgl/glsl/02/ by MrDoob
 * adapted from answer for http://stackoverflow.com/questions/4638317
 */

/** @type {HTMLElement} */
var effectDiv;
/** @type {HTMLCanvasElement} */
var canvas;

/** @type {WebGLRenderingContext} */
var gl;
/** @type {WebGLBuffer} */
var buffer;
/** @type {WebGLProgram} */
var currentProgram;
/** CCapture handler */
var capturer;
var canvasSize = "small";
var speedFactor = 1;
var direction = 1;
var rotation = 1;
var branchCount = 4;
var colors = { fg: {}, bg: {}, dim: {}, pulse: {} };
var parameters = {
	time: 0,
	screenWidth: 0,
	screenHeight: 0,
};

var oldColor;
var newColor;
var colorGradient;
/** Locations buffer */
var locations;

init()
.then(function(data) {
	function loopFrame() {
		parameters.time += TIME_INTERVAL * speedFactor;
		loop(data.currentProgram, data.locations);
		capturer.capture(canvas);
	}
	setInterval(loopFrame, TIME_INTERVAL);
});

function exportGif() {
	capturer.start();
}

function init() {
	// These are the URI parameters.
	// Spiral shader to use.
	var config = getConfigFromURL();
	canvasSize = config.canvasSize;
	colors = config.colors;
	speedFactor = config.speedFactor;
	direction = config.direction;
	rotation = config.rotation;
	branchCount = config.branchCount;

	capturer = new CCapture({
		format: 'gif',
		workersPath: 'js/',
		timeLimit: 1.1,
		framerate: 60,
		verbose: false,
		name: config.exportName || undefined,
	});

	// Hide the export button if needed
	if(!config.showButton)
		document.getElementById("exportButton").style.display = "none";

	return Promise.all([
		fetch("shaders/spiral.vs").then(function (r) { return r.text() }),
		fetch("shaders/" + config.shaderFileName + ".fs").then(function (r) { return r.text() }),
	])
	.then(function (/** @type [string, string] */fetchedText) {
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
		currentProgram = createProgram(fetchedText[0], fetchedText[1]);

		locations = getLocations(currentProgram);

		// Setup program size.
		onWindowResize();
		window.addEventListener("resize", onWindowResize, false);

		if(config.keyboardControls) {
			window.addEventListener("keydown", (ev) => {
				switch(ev.code) {
					case "KeyQ": newColor = {r: 1.0, g: 0, b: 0}; break;
					case "KeyW": newColor = {r: 1.0, g: 1.0, b: 0}; break;
					case "KeyE": newColor = {r: 0, g: 1.0, b: 0}; break;
					case "KeyR": newColor = {r: 0, g: 1.0, b: 1.0}; break;
					case "KeyT": newColor = {r: 0, g: 0, b: 1.0}; break;
					case "KeyY": newColor = {r: 1.0, g: 0, b: 1.0}; break;
					case "KeyU": newColor = {r: 1.0, g: 1.0, b: 1.0}; break;
					case "KeyI": newColor = {r: 0, g: 0, b: 0}; break;
					default: return;
				}
				oldColor = colors.fg;
				colorGradient = 0.0;
			});
		}

		return {
			currentProgram: currentProgram,
			locations: locations,
		}
	});
}

function gradient(color1, color2, value) {
	return {
		r: color1.r * (1 - value) + color2.r * (value),
		g: color1.g * (1 - value) + color2.g * (value),
		b: color1.b * (1 - value) + color2.b * (value),
	}
}

function createShader(/**@type {string}*/src, /**@type {number}*/type) {
	var shader = gl.createShader(type);
	gl.shaderSource(shader, src);
	gl.compileShader(shader);
	if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
		alert((type == gl.VERTEX_SHADER ? "VERTEX" : "FRAGMENT" ) + " SHADER:\n" + gl.getShaderInfoLog(shader));
		return null;
	}
	return shader;
}

function createProgram(/**@type {string}*/vertex, /**@type {string}*/fragment) {
	var program = gl.createProgram();
	var vs = createShader(vertex, gl.VERTEX_SHADER);
	var fs = createShader("precision highp float;\n\n" + fragment, gl.FRAGMENT_SHADER);
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
	gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(0);
	return program;
}

/**
 * @typedef Locations
 * @property {WebGLUniformLocation} time
 * @property {WebGLUniformLocation} branchCount
 * @property {WebGLUniformLocation} direction
 * @property {WebGLUniformLocation} rotation
 * @property {WebGLUniformLocation} resolution
 * @property {WebGLUniformLocation} aspect
 * @property {WebGLUniformLocation} bgColor
 * @property {WebGLUniformLocation} fgColor
 * @property {WebGLUniformLocation} pulseColor
 * @property {WebGLUniformLocation} dimColor
 */

/**@returns {Locations}*/
function getLocations(/** @type {WebGLProgram} */program) {
	return {
		time: gl.getUniformLocation(program, 'time'),
		branchCount: gl.getUniformLocation(program, 'branchCount'),
		direction: gl.getUniformLocation(program, 'direction'),
		rotation: gl.getUniformLocation(program, 'rotation'),
		resolution: gl.getUniformLocation(program, 'resolution'),
		aspect: gl.getUniformLocation(program, 'aspect'),
		bgColor: gl.getUniformLocation(program, 'bgColor'),
		fgColor: gl.getUniformLocation(currentProgram, 'fgColor'),
		pulseColor: gl.getUniformLocation(currentProgram, 'pulseColor'),
		dimColor: gl.getUniformLocation(currentProgram, 'dimColor'),
	};
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
	else if(canvasSize === "16-9") {
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
	parameters.aspectY = 1.0;
	gl.viewport(0, 0, canvas.width, canvas.height);
}

function loop(/**@type {WebGLProgram}*/program, /**@type {Locations}*/locations) {
	if (!program) return;
	// Clear the screen. Not necessary so far since we draw everywhere on it.
	// gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

	if(colorGradient != null) {
		colorGradient += 0.05;
		colors.fg = gradient(oldColor, newColor, colorGradient);
		if(colorGradient >= 1.0)
			colorGradient = null;
	}

	// Load program into GPU
	gl.useProgram(program);
	// Set all the needed values as program variables
	gl.uniform1f(locations.time, parameters.time / 1000);
	gl.uniform1f(locations.branchCount, branchCount);
	gl.uniform1f(locations.direction, direction);
	gl.uniform1f(locations.rotation, rotation);
	gl.uniform2f(locations.resolution, parameters.screenWidth, parameters.screenHeight);
	gl.uniform2f(locations.aspect, parameters.aspectX, parameters.aspectY);
	gl.uniform4f(locations.bgColor, colors.bg.r, colors.bg.g, colors.bg.b, 1.0);
	gl.uniform4f(locations.fgColor, colors.fg.r, colors.fg.g, colors.fg.b, 1.0);
	gl.uniform4f(locations.pulseColor, colors.pulse.r, colors.pulse.g, colors.pulse.b, 1.0);
	gl.uniform4f(locations.dimColor, colors.dim.r, colors.dim.g, colors.dim.b, 1.0);

	// Render using the given "full screen" geometry
	gl.drawArrays(gl.TRIANGLES, 0, 6);
}
