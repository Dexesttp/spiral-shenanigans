
	
	/*
	if(config.keyboardControls) {
		script = [
			{ waitText: "Put your fingers on W and O<br />Press O", wantedKey: "KeyO", offset: .5, timeout: 4000 },
			{ waitText: "Press W", wantedKey: "KeyW", color: {r: .25, g: .25, b: .25}, timeout: 5000 },
			{ waitText: "Press W", wantedKey: "KeyW", offset: .25, timeout: 7000 },
			{ waitText: "Press O", wantedKey: "KeyO", color: {r: .1, g: .1, b: .1}, timeout: 9000 },
			{ waitText: "Press W", wantedKey: "KeyW", color: {r: 0, g: 0, b: 0}, timeout: 10000 },
			{ waitText: "Press W", wantedKey: "KeyW", offset: 0.15, timeout: 10000 },
			{ waitText: "Press O", wantedKey: "KeyO", offset: 0., timeout: 10000, rewardText: "Drop" },
		]
		rewardText = "Good. Keep staring";
		window.addEventListener("keydown", function(ev) {
			if(script[0]
				&& (ev.code === script[0].wantedKey)
				&& (Date.now() > eventTimeout)
			) {
				if(script[0].color) {
					newColor = script[0].color;
					oldColor = colors.fg;
					colorGradient = 0.0;
				}
				if(script[0].offset !== undefined) {
					newOffsetCenter = script[0].offset;
					oldOffsetCenter = offsetCenter;
					offsetGradient = 0.0;
				}
				if(script[0].rewardText) {
					rewardText = script[0].rewardText;
				}
				document.getElementById("screenText").innerHTML = rewardText;
				emptyTimeout = Date.now() + 2000;
				eventTimeout = Date.now() + script[0].timeout;
				script.shift();
				return;
			}
		});
	}
	*/