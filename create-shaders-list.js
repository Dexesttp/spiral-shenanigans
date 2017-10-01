const fileName = "src/shaders-list.js";
const extension = ".fs";

const fs = require("fs");
const fileList = fs.readdirSync("shaders");

const shadersList = fileList
    .filter(f => f.endsWith(".fs"))
    .map(f => f.slice(0, -extension.length))
    .map(f => `"${f}"`);

if(fs.existsSync(fileName)) {
    fs.unlinkSync(fileName);
}

const textToWrite = `"use strict";
var shadersList = [
    ${shadersList.join(`,
    `)}
];
`;
fs.writeFileSync(fileName, textToWrite);