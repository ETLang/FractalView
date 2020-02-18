# Fractal View
Fractal View allows you to view Mandelbrot and Julia fractals rendered in real-time, up to zoom levels of around 10 trillion.

Zoom levels up to around 50,000x are rendered using GPU.
From there to around 10,000,000,000,000x is rendered using SIMD and parallelism on CPU using Unity's Burst compiler.

Project targets Unity 2019.3.0f6 and can be built as Desktop app, or published as WebGL. (See https://etlang.itch.io/fractal-explorer)

Note: Burst doesn't work for WebGL, so zoom levels over ~50,000x render very slowly in WebGL.

### Things to Do
- Play around with the values of Cr and Ci to alter the shape of the fractal.
- Increase Max I to get more fine details. Decrease Max I to improve performance.
- Adjust the gradient spread to highlight detail
- Switch between the classic "thumbprint" Mandelbrot set and burning ship fractal
- Zoom and pan using mouse/touch

### In Progress
- Save as PNG for wallpaper/lock screen/whatever you like
- Save a high-quality TIFF suitable for printing and framing.

### Coming soon
- Configurable color gradients
- Additional visualization modes
- Bookmarks and slideshow
- double-double precision using Burst compiler, allowing zoom levels up to somewhere around 10^30.
- Mobile and UWP support
