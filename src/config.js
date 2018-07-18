"use strict";

function getConfigFromURL() {
	var params = new URLSearchParams(window.location.search);
    var showInterface = params.has("interface");
	return {
		shaderFileName: params.get("file") || "spiral",
		// Size of the canvas, either "small" (400*300) or "big" (whole screen)
		canvasSize:
			showInterface
				? params.get("canvas") === "small" ? "small"
				: params.get("canvas") === "portrait" ? "portrait"
				: "interface"
            : params.has("big") ? "big"
            : params.get("canvas") || "small",
		speedFactor: +params.get("speed") || 1,
		rotation: !params.has("counterclockwise") ? 1 : -1,
		direction: !params.has("inwards") ? -1 : 1,
		branchCount: +params.get("branch") || 4,
		interface: showInterface,
		exportName: params.get("exportName") || undefined,
		keyboardControls: params.has("keyboardControls"),
		// These colors are stored as rgba() vectors. red, green, and blue should be between 0 and 1. Let alpha be at 1.
		colors: {
			bg: hexToRgb(params.get("bg")) || {r: 0, g: 0, b: 0},
			fg: hexToRgb(params.get("fg")) || {r: 1.0, g: 1.0, b: 1.0},
			pulse: hexToRgb(params.get("pulse")) || {r: 0.7, g: 0.3, b: 0.9},
			dim: hexToRgb(params.get("dim")) || {r: 0, g: 0, b: 0},
		},
	};
}