# SPIRAL SHENANIGANS

[Direct link to the latest spiral](https://dexesttp.github.io/spiral-shenanigans/?big)

## Scope

This project intends to propose a bunch of shader-based spiral patterns.

For now, the Javascript part of this project is only a renderer for the patterns. You can customize some things using URL parameters.

Long term, it might also include a control panel to chose from everything that exists with a more user-friendly UI.

## License

This project is originally based on the [following StackOverflow answer](https://stackoverflow.com/questions/4638317/how-to-implement-this-rotating-spiral-in-webgl), but has evolved far from its scope. Given how little of the code from that is still in it, I decided to re-license it. If you're the author of the above snippet and want to challenge that, shoot me an issue or mail.

This project is MIT. Just say the shaders are from here or based from here, and you're good to use them for whatever you feel like.

## How to use URL parameters

### `?<name>=9AB` / `?<name>=9ABCDE`

Use to set the hexadecimal code of the background (`bg`), foreground (`fg`), pulse (`pulse`) or dim (`dim`) of the spiral.

### `?file=<filename>`

Change the shaders from the base `shaders/spiral.fs` file to the wanted `shaders/<filename>.fs` file.

Filename is combined with the `"shaders/" + spiralFile + ".fs"` expression. It might not allow for everything to work.

### `?canvas=<size>`

Change the canvas size from `small` (default), to `big` or `screen`. You can also set `?big` directly for instead of `?canvas=big` (legacy, works only for `big`)

- `small` : 400x300, ideal for gifs.
- `big` : fit to the screen.
- `screen` : 1280x720, for HD exports.

### `?showButton`

Show the export button. See the ["Capture"](#Capture) section for more infos. 

## Capture

This project includes `CCapture`, to capture the rendering as gifs.

The gifs length are fit to loop perfectly on the latest spiral. They are usually taken at 60fps.

Click on the `start export` button to make the gif export start. You'll need the `?showButton` URL parameter for it to work.

The export might take a while. The gif will be downloaded as `<guid>.gif`

## How to contribute

The usual. Fork, send a PR, open an issue explaining what you want or what your issue is.

You can also e-mail me but I wouldn't recommend it, I'm not good at replying to emails.

### How to run locally

`npm install`

`node server`

Reload after each change.

I use VSCode, but you can use whatever.

### Rules for this project

- This project is mostly for fun.
- Don't focus too much on the javascript part. The actual focus would come once starting on the GUI, but even then it should stay optional.
- The project must run "as-is" : The GitHub project page is the main branch.
- No watermarks, no things disturbing the spiral itself, unless behind an URL flag.
