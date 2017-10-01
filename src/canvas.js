
/** @type {WebGLRenderingContext} */
var gl;
/** @type {WebGLBuffer} */
var buffer;

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
 * @property {WebGLUniformLocation} offsetCenter
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
		offsetCenter: gl.getUniformLocation(program, 'offsetCenter'),
		resolution: gl.getUniformLocation(program, 'resolution'),
		aspect: gl.getUniformLocation(program, 'aspect'),
		bgColor: gl.getUniformLocation(program, 'bgColor'),
		fgColor: gl.getUniformLocation(program, 'fgColor'),
		pulseColor: gl.getUniformLocation(program, 'pulseColor'),
		dimColor: gl.getUniformLocation(program, 'dimColor'),
	};
}

function initWebGL(canvas, shaderVS, shaderFS) {
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
    var currentProgram = createProgram(shaderVS, shaderFS);
    // Create Program
    return {
        currentProgram: currentProgram,
        locations: getLocations(currentProgram),
    };
}

function renderCanvas(/**@type {WebGLProgram}*/program, /**@type {Locations}*/locations, parameters) {
	// Load program into GPU
	gl.useProgram(program);
	// Set all the needed values as program variables
	gl.uniform1f(locations.time, parameters.time / 1000);
	gl.uniform1f(locations.branchCount, parameters.branchCount);
	gl.uniform1f(locations.direction, parameters.direction);
	gl.uniform1f(locations.rotation, parameters.rotation);
	gl.uniform1f(locations.offsetCenter, parameters.offsetCenter);
	gl.uniform2f(locations.resolution, parameters.screenWidth, parameters.screenHeight);
	gl.uniform2f(locations.aspect, parameters.aspectX, parameters.aspectY);
	gl.uniform4f(locations.bgColor, parameters.colors.bg.r, parameters.colors.bg.g, parameters.colors.bg.b, 1.0);
	gl.uniform4f(locations.fgColor, parameters.colors.fg.r, parameters.colors.fg.g, parameters.colors.fg.b, 1.0);
	gl.uniform4f(locations.pulseColor, parameters.colors.pulse.r, parameters.colors.pulse.g, parameters.colors.pulse.b, 1.0);
	gl.uniform4f(locations.dimColor, parameters.colors.dim.r, parameters.colors.dim.g, parameters.colors.dim.b, 1.0);

	// Render using the given "full screen" geometry
	gl.drawArrays(gl.TRIANGLES, 0, 6);
}
