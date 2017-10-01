/** CCapture handler */
var capturer;

function createCapturer(name) {
    capturer = new CCapture({
        format: 'gif',
		workersPath: 'js/',
		timeLimit: 1.1,
		framerate: 60,
		verbose: false,
		name: name,
	});
}

function captureCanvas(canvas) {
	capturer.capture(canvas);
}

function exportGif() {
capturer.start();
}
