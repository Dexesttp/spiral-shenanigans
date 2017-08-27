# SPIRAL SHENANIGANS

[Direct link to the latest spiral](https://dexesttp.github.io/spiral-shenanigans/?big)

## Scope

This project intends to propose a bunch of shader-based spiral patterns, as well as some simple customizables options for them and a way to export the spirals as gifs.

For now, the Javascript part of this project is only a renderer for the patterns. You can customize some things using URL parameters.

The project also has a [puppeteer](https://github.com/GoogleChrome/puppeteer)-based renderer.

Long term, it might also include a control panel to chose from everything that exists with a more user-friendly UI.

## License

This project is originally based on the [following StackOverflow answer](https://stackoverflow.com/questions/4638317/how-to-implement-this-rotating-spiral-in-webgl), but has evolved far from its scope. Given how little of the code from that is still in it, I decided to re-license it. If you're the author of the above snippet and want to challenge that, shoot me an issue or mail.

This project is MIT. Just say the shaders are from here or based from here if you want. Other than that, you're good to use them for whatever you feel like.

## How to use URL parameters

### `?file=<filename>`

Change the shaders from the base `shaders/spiral.fs` file to the wanted `shaders/<filename>.fs` file.

Filename is combined with the `"shaders/" + spiralFile + ".fs"` expression. It might not allow for everything to work.

### `?<name>=9AB` / `?<name>=9ABCDE`

Use to set the hexadecimal code of the background (`bg`), foreground (`fg`), pulse (`pulse`) or dim (`dim`) of the spiral.

### `?speed=<number>`

The period of the spiral. 1.0 is a 2-second loop, 0.5 is a 4-second loop and 2.0 is a 1-second loop.

### `?branch=<number>`

The number of branches for the spiral, when applicable. Defaults to 4.

### `?inwards` / `?counterclockwise`

By default, configurable spirals are outwards and clockwise. Add and/or combine these options to change that. 

### `?canvas=<size>`

Change the canvas size from `small` (default), to `big` or `screen`. You can also set `?big` directly for instead of `?canvas=big` (legacy, works only for `big`)

- `small` : 400x300, ideal for gifs.
- `big` : fit to the screen.
- `screen` : 1280x720, for HD exports.

### `?showButton`

Show the export button. See the ["Capture"](#Capture) section for more infos. 

### `?exportName=<name>`

Set the export name for the downloaded file. See the ["Capture"](#Capture) section for more infos. 

## Capture

This project includes [CCapture.js](https://github.com/spite/ccapture.js), to capture the rendering as gifs.

The gifs length are fit to loop perfectly on the spirals (excepted the `static` spirals). They are taken at 60fps.

Click on the `start export` button to make the gif export start. You'll need the `?showButton` URL parameter for the button to be displayed.

The export might take a while - up to one minute for the `screen` canvas size - and can end up fairly large.
The gif will be downloaded as `<exportName>.gif` if the `exportName` URI parameter is set.
Otherwise, it will be downloaded as `<guid>.gif` with `<guid>` a randomly-generated value.

## How to contribute

The usual. Fork, send a PR, open an issue explaining what you want or what your issue is.

You can also e-mail me but I wouldn't recommend it, I'm not good at replying to emails.

### How to run locally

`npm install`

`node server`

Reload after each change.

I use VSCode, but you can use whatever.

### Rules for this project

- This project is for fun.
- Every spiral must be WebGL-only. Every variable coming from Javascript _must be static_, except for time.
- Don't focus too much on the javascript part. The actual focus would come once starting on the GUI, but even then it should stay optional.
- NO TESTS. If the project ever become big enough to need tests, it must be forked.
- The project must run "as-is" : The GitHub project page is the main branch.
- No watermarks, no things disturbing the spiral itself (unless behind an URL flag).
