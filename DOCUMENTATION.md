# Documentation

This is far from exhaustive, but might as well include what little there is somewhere.

## General informations

For now, the Javascript part of this project is only a renderer for the patterns.

The `?interface` query parameter displays a web UI to change some parameters, but not all of them are implemented. Refer to below for a full list.

The project has a [puppeteer](https://github.com/GoogleChrome/puppeteer)-based renderer, which you can use to render a bunch of spirals at once. Check out [the Puppeteer section](#Puppeteer) for more data

## How to use URL parameters

### `?interface`

If present, displays a user interface.

### `?file=<filename>`

Change the shaders from the base `shaders/spiral.fs` file to the wanted `shaders/<filename>.fs` file.

Filename is combined with the `"shaders/" + spiralFile + ".fs"` expression. It might not allow for everything to work.

### `?<name>=9AB` / `?<name>=9ABCDE`

Use to set the hexadecimal code of the background (`bg`), foreground (`fg`), pulse (`pulse`) or dim (`dim`) of the spiral.

### `?speed=<number>`

The looping frequency of the spiral. 1.0 is a 2-second loop, 0.5 is a 4-second loop and 2.0 is a 1-second loop.

### `?branch=<number>`

The number of branches for the spiral, when applicable. Defaults to 4.

### `?inwards` / `?counterclockwise`

By default, configurable spirals are outwards and clockwise. Add and/or combine these options to change that. 

### `?canvas=<size>`

Change the canvas size from `small` (default), to `big` or `screen`. You can also set `?big` directly for instead of `?canvas=big` (legacy, works only for `big`)

- `small` : 400x300, ideal for gifs.
- `big` : fit to the screen.
- `screen` : 1280x720, for HD exports.

### ~~`?showButton`~~

~~Show the export button. See the ["Capture"](#Capture) section for more infos. ~~

Deprecated. Check out `?interface` parameter to access the export system.

### `?exportName=<name>`

Set the export name for the downloaded file. See the ["Capture"](#Capture) section for more infos. 

## Puppeteer

You can use Puppeteer to download a bunch of spirals at once.

The start command is `node download`. It will download all the spirals defined in the file, wih default colors and parameters. It will also save the downloaded spirals in your local download folder, with he same name as the shader.

## Capture

This project includes [CCapture.js](https://github.com/spite/ccapture.js), to capture the rendering as gifs.

The gifs length are fit to loop perfectly on the spirals (excepted the `static` spirals). They are taken at 60fps.

Click on the `start export` button to make the gif export start. You'll need the `?showButton` URL parameter for the button to be displayed.

The export might take a while - up to one minute for the `screen` canvas size - and can end up fairly large.
The gif will be downloaded as `<exportName>.gif` if the `exportName` URI parameter is set.
Otherwise, it will be downloaded as `<guid>.gif` with `<guid>` a randomly-generated value.
