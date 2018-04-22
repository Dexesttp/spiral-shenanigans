# SPIRAL SHENANIGANS

[Direct link to the latest spiral](https://dexesttp.github.io/spiral-shenanigans/?big)

[The customization screen when you can play around with spirals](https://dexesttp.github.io/spiral-shenanigans/?interface)

## Scope

This project intends to propose a bunch of shader-based spiral patterns, as well as some simple customizables options for them and a way to export the spirals as gifs. You can customize some things using URL parameters (documentation [here](/DOCUMENTATION.md#how-to-use-url-parameters)), or use the `?interface` query parameter to get a more user-friendly way to change it.

## License

This project is MIT.

If you want, say the shaders/gifs/etc... are from here or based from here. You don't have to though.

This project is originally based on the [following StackOverflow answer](https://stackoverflow.com/questions/4638317/how-to-implement-this-rotating-spiral-in-webgl), but has evolved far from its scope. Given how little of the code from that is still in it, I decided to re-license it. If you're the author of the above snippet and want to challenge that, shoot me an issue or mail.

## How to contribute

The usual. Fork, send a PR, open an issue explaining what you want or what your issue is.

You can also e-mail me but I wouldn't recommend it, I'm not good at replying to emails.

You could also [check out the small documentation](/DOCUMENTATION.md)

### How to run locally

`npm install`

`node server`

Reload manually after each change.

I use VSCode, but you can use whatever.

### Rules for this project

- This project is for fun. If you want to start a more serious project based on this, you should fork.
- Every spiral must be WebGL-only. Every variable coming from Javascript _must be static_, except for time.
- The project must run "as-is" : The GitHub project page _is_ the main branch.
- NO TESTS. If the project ever become big enough to need tests, it must be forked.
- NO TRANSPILING. That means there should only be es5 code in the /src folder ; intended compatibility is IE11, latest edge, chrome and FF (as much as possible).
- No watermarks, no things disturbing the spiral itself (unless behind an URL flag).
