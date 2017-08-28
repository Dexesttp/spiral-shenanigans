"use strict";
const puppeteer = require("puppeteer");
const fs = require("fs");
const moment = require("moment");

// 120s timeout
const timeout = 120e3;
// 1s export time
const postProcessWait = 1e3;
// Screen canvas size on export
const canvasSize = "screen";

const fileNames = [
	"full-bicolor",
	"full-bicolor-pulse",
	"full-dot",
	"full-dot-pulse",
	"full-equal-branches",
	"full-equal-branches-pulse",
	"full-half-circle",
	"full-pulse",
	"full-sharded",
	"full-sharded-pulse",
	"full-twisted",
	"full-flower",
	"full-twisted-dots",
	"full-twisted-lines",
	"full-twisted-pulse",
	"pendulum-bicolor",
	"pendulum-bicolor-pulse",
	"pendulum-circles",
	"pendulum-disc",
	"pendulum-dot",
	"pendulum-oval",
	"pendulum-oval-intermittent-spiral",
	"pendulum-spiral",
	"pendulum-total-disc",
	"pendulum-twisted-spiral-going-inwards",
	"spiral",
	"test-dot",
	"test-double-color",
	"test-expanding-twists",
	"test-flower-colors-sin",
	"test-full-dots",
	"test-rainbow-spiral",
	"test-small-dots",
];

(async () => {
	process.stdout.write("Starting Puppeteer... ");
	const browser = await puppeteer.launch({headless: false});
	process.stdout.write("Starting browser... ");
	const page = await browser.newPage();
	process.stdout.write(`Browser started at ${moment().format("YYYY-MM-DD HH:mm:ss")}\n`);
	for(let fileName of fileNames) {
		await page.goto(`http://localhost:9080?showButton&canvas=${canvasSize}&file=${fileName}&exportName=${fileName}`);
		process.stdout.write(`Opening ${fileName}... `);
		await page.waitFor(200);
		const button = await page.$("#exportButton");
		button.click();
		process.stdout.write(`Exporting... `);
		await new Promise((resolve, reject) => {
			let resolved = false;
			let onConsole = (msg) => {
				if(msg.startsWith("rendering finished")) {
					page.removeListener('console', onConsole);
					process.stdout.write(`Export success\n`);
					resolved = true;
					resolve();
				}
			};
			page.on('console', onConsole);
			// Wait at most a timeout
			setTimeout(() => {
				if(resolved)
					return;
				process.stdout.write(`Export failed (${Math.floor(timeout)}min timeout)\n`);
				reject();
			}, timeout);
		})
		// Wait for the DL to end.
		await page.waitFor(postProcessWait);
	}
	process.stdout.write("Export ended. Closing...\n");	
	browser.close();
})().catch(() => {
	process.stdout.write("\nInterrupted. Closing...\n");
});