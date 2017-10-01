"use strict";
//@ts-check

function setURLParameter(name, value) {
    const params = new URLSearchParams(location.search);
	params.set(name, value);
	window.history.replaceState({}, '', "" + location.pathname + "?" + params);
}

function addOrRemoveParameter(name, shouldAdd) {
    const params = new URLSearchParams(location.search);
    if(shouldAdd)
        params.set(name, "");
    else
        params.delete(name);
	window.history.replaceState({}, '', "" + location.pathname + "?" + params);
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

function rgbToHexInternal(r, g, b) {
    return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
}

function rgbToHex(rgb) {
    return rgbToHexInternal(
        Math.floor(rgb.r * 255),
        Math.floor(rgb.g * 255),
        Math.floor(rgb.b * 255)
    );
}
