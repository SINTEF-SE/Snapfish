class WebGLUtil {
    static createShaderProgram(gl, vsText, fsText) {
        const vertexShader = WebGLUtil.loadShader(gl, gl.VERTEX_SHADER, vsText);
        const fragmentShader = WebGLUtil.loadShader(gl, gl.FRAGMENT_SHADER, fsText);
        const program = gl.createProgram();
        gl.attachShader(program, vertexShader);
        gl.attachShader(program, fragmentShader);
        gl.linkProgram(program);
        
        if (!gl.getProgramParameter(program, gl.LINK_STATUS))
            console.log('Unable to initialize the shader program: ' + gl.getProgramInfoLog(program));
    
        return program;
    }

    static loadShader(gl, type, source) {
        const shader = gl.createShader(type);
        gl.shaderSource(shader, source);
        gl.compileShader(shader);
        if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS))
          console.log('An error occurred compiling the shaders: ' + gl.getShaderInfoLog(shader));

        return shader;
    }

    static createSquareBuffer(gl) {
        const squareBuffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, squareBuffer);
        const positions = new Float32Array([0,0, 0,1, 1,0, 1,0, 0,1, 1,1]);
        gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);
      
        return squareBuffer;
    }

    static createDataTexture(gl, data, filteredRange, interpolated) {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const imageData = WebGLUtil.createImageData(ctx, data, filteredRange);

        const filter = interpolated ? gl.LINEAR : gl.NEAREST;
        const texture = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, texture);
    
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, filter);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, filter);
    
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, imageData);
    
        return texture;
    }

    static createImageData(ctx, data, filteredRange) {
        const width = data.NumberOfSlices;
        const height = data.SliceHeight;

        const imageData = ctx.createImageData(width, height);

        const min = filteredRange[0];
        const max = filteredRange[1];

        let colorIndex = 0;
        for (let y = 0; y < height; y++) {
            for (let x = 0; x < width; x++) {
                // TODO: Use more than 1 byte for color resolution
                const value = data.Slices[x].Data[height - y];
                imageData.data[colorIndex] = Math.floor(255 * (value - min) / (max - min));
                imageData.data[colorIndex + 1] = 0;
                imageData.data[colorIndex + 2] = 0;
                if (value < min || value > max)
                    imageData.data[colorIndex + 3] = 0;
                else
                    imageData.data[colorIndex + 3] = 255;
                    colorIndex += 4;
            }
        }

        return imageData;
    }

    static getColor(value, scale, range) {
        if (value < range[0] || value > range[1])
            return [255, 255, 255];
        return scale(value).rgb();
    }

    // Stolen from https://developer.mozilla.org/en-US/docs/Web/API/WebGL_API/Tutorial/Using_textures_in_WebGL
    static createDefaultColorScaleTexture(gl) {
        return new Promise((resolve, reject) => {
            const texture = gl.createTexture();
            const image = new Image();
            image.onload = function() {
                gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA,
                    gl.RGBA, gl.UNSIGNED_BYTE, image);

                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

                resolve(texture);
            };
            image.src = '/src/color-scale.png';
        });
    }
}