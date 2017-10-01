"use strict";
//@ts-check

/** @type {HTMLCanvasElement} */
/*
 * adapted from http://mrdoob.com/lab/javascript/webgl/glsl/02/ by MrDoob
 * adapted from answer for http://stackoverflow.com/questions/4638317
 */

var oldColor;
var newColor;
var colorGradient;

var oldOffsetCenter;
var newOffsetCenter;
var offsetGradient;

var rewardText;
var eventTimeout = Date.now() + 5000;
var emptyTimeout = 0.;

var script = [];
/*
var parameters = {
	time: 0,
	screenWidth: 0,
	screenHeight: 0,
    direction: 1,
    rotation: 1,
    branchCount: 4,
    speedFactor: 1,
    offsetCenter: 0.,
    colors: { fg: {}, bg: {}, dim: {}, pulse: {} },
};
*/

var loopData;

init(getConfigFromURL())
.then(function(data) {
	loopData = data;
	function loopFrame() {
		loopData.parameters.time += TIME_INTERVAL * loopData.parameters.speedFactor;
		loop(loopData.data.currentProgram, loopData.data.locations, loopData.parameters);
		captureCanvas(canvas);
	}
	setInterval(loopFrame, TIME_INTERVAL);
});

function getCanvasData(fileName, canvas, parameters) {
	return Promise.all([
		fetch("shaders/spiral.vs").then(function (r) { return r.text() }),
		fetch("shaders/" + fileName + ".fs").then(function (r) { return r.text() }),
	])
	.then(function (/** @type [string, string] */fetchedText) {
		return {
			data: initWebGL(canvas, fetchedText[0], fetchedText[1]),
			parameters: parameters,
		};
	});
}

function largeInterface() {
    const params = new URLSearchParams(location.search);
	params.delete("canvas");
	window.location.href = "" + location.pathname + "?" + params;
}

function smallInterface() {
    const params = new URLSearchParams(location.search);
	params.set("canvas", "small");
	window.location.href = "" + location.pathname + "?" + params;
}

function fullScreen() {
    const params = new URLSearchParams(location.search);
	params.delete("interface");
	params.set("canvas", "big");
	window.location.href = "" + location.pathname + "?" + params;
}

function init(config) {
	// Spiral parameters to use and/or change.
	var parameters = {
		time: 0,
		screenWidth: 0,
		screenHeight: 0,
		direction: config.direction,
		rotation: config.rotation,
		branchCount: config.branchCount,
		colors: config.colors,
		speedFactor: config.speedFactor,
	};

	// These are the URI parameters.
	createCapturer(config.exportName || undefined);
	// Hide the export button if needed
	if(config.interface) {
		// Shaders
		document.getElementById("shaders-list").innerHTML = shadersList
			.map(function(s) { return "<option" + (s === config.shaderFileName ? " selected=\"selected\"" : "") + ">" + s + "</option>"; })
			.join("");
		document.getElementById("shaders-list").addEventListener("change", function(ev) {
			var shaderName = document.getElementById("shaders-list").value;
			setURLParameter("file", shaderName);
			getCanvasData(shaderName, canvas, parameters)
			.then(function(data) {
				loopData = data;
			});
		});
		// Base Parameters
		document.getElementById("branch-count").value = config.branchCount;
		document.getElementById("branch-count").addEventListener("change", function(ev) {
			var branchCount = +(document.getElementById("branch-count").value);
			if(branchCount > 0) { setURLParameter("branch", branchCount); parameters.branchCount = branchCount; }
		});
		document.getElementById("counterclockwise").checked = config.rotation === -1;
		document.getElementById("counterclockwise").addEventListener("change", function(ev) {
			var checked = document.getElementById("counterclockwise").checked;
			addOrRemoveParameter("counterclockwise", checked);
			parameters.rotation = checked ? -1 : 1;
		});
		document.getElementById("inwards").value = config.direction === -1;
		document.getElementById("inwards").addEventListener("change", function(ev) {
			const checked = document.getElementById("inwards").checked;
			addOrRemoveParameter("inwards", checked);
			parameters.direction = checked ? 1 : -1;
		});
		// Colors parameters
		document.getElementById("bg-color").value = rgbToHex(config.colors.bg);
		document.getElementById("bg-color").addEventListener("change", function(ev) {
			var value = (document.getElementById("bg-color").value).substr(1);
			var color = hexToRgb(value);
			if(color) { setURLParameter("bg", value); parameters.colors.bg = color; }
		});
		document.getElementById("fg-color").value = rgbToHex(config.colors.fg);
		document.getElementById("fg-color").addEventListener("change", function(ev) {
			var value = (document.getElementById("fg-color").value).substr(1);
			var color = hexToRgb(value);
			if(color) { setURLParameter("fg", value); parameters.colors.fg = color; }
		});
		document.getElementById("pulse-color").value = rgbToHex(config.colors.pulse);
		document.getElementById("pulse-color").addEventListener("change", function(ev) {
			var value = (document.getElementById("pulse-color").value).substr(1);
			var color = hexToRgb(value);
			if(color) { setURLParameter("pulse", value); parameters.colors.pulse = color; }
		});
		document.getElementById("dim-color").value = rgbToHex(config.colors.dim);
		document.getElementById("dim-color").addEventListener("change", function(ev) {
			var value = (document.getElementById("dim-color").value).substr(1);
			var color = hexToRgb(value);
			if(color) { setURLParameter("dim", value); parameters.colors.dim = color; }
		});
		console.log(rgbToHex(config.colors.pulse));
		// Display the area
		document.getElementById("interface").style.display = "";
		document.getElementById("test").classList.add("hasInterface");
	}

	var	canvas = document.getElementById('canvas');
	
	return getCanvasData(config.shaderFileName, canvas, parameters)
	.then(function(data) {
		// Setup program size.
		onWindowResize(config, canvas, data.parameters);
		window.addEventListener("resize", function(ev) { onWindowResize(config, canvas, data.parameters); }, false);
		return data;
	});
}

function gradient(color1, color2, value) {
	return {
		r: color1.r * (1 - value) + color2.r * (value),
		g: color1.g * (1 - value) + color2.g * (value),
		b: color1.b * (1 - value) + color2.b * (value),
	}
}

function onWindowResize(config, canvas, parameters) {
	if(config.canvasSize === "small") {
		canvas.width = 400;
		canvas.height = 300;
	}
	else if(config.canvasSize === "interface") {
		var width = 2 * window.innerWidth / 3;
		var height = 3 * width / 4;
		if(height > window.innerHeight) {
			width = 4 * window.innerHeight / 3;
			height = window.innerHeight;
		}
		canvas.width = width;
		canvas.height = height;
	}
	else if(config.canvasSize === "screen") {
		canvas.width = 1280;
		canvas.height = 720;
	}
	else if(config.canvasSize === "1080") {
		canvas.width = 1920;
		canvas.height = 1080;
	}
	else if(config.canvasSize === "16-9") {
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

function loop(/**@type {WebGLProgram}*/program, /**@type {Locations}*/locations, parameters) {
	if (!program) return;
	// Clear the screen. Not necessary so far since we draw everywhere on it.
	// gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

	if(colorGradient != null) {
		colorGradient += 0.02;
		parameters.colors.fg = gradient(oldColor, newColor, colorGradient);
		if(colorGradient >= 1.0)
			colorGradient = null;
	}
	
	if(offsetGradient != null) {
		offsetGradient += 0.01;
		parameters.offsetCenter = oldOffsetCenter * (1 - offsetGradient) + newOffsetCenter * (offsetGradient);
		if(offsetGradient >= 1.0)
			offsetGradient = null;
	}

	if(script[0] && (Date.now() > eventTimeout)) {
		document.getElementById("screenText").style.opacity = 1;
		document.getElementById("screenText").innerHTML = script[0].waitText;
	}
	else if(emptyTimeout && (Date.now() > emptyTimeout)) {
		document.getElementById("screenText").style.opacity = 0;
		emptyTimeout = null;
	}

	renderCanvas(program, locations, parameters);
}
