class WebglUtils {
    static createShaderProgram(gl, vsText, fsText) {
        const vertexShader = WebglUtils.loadShader(gl, gl.VERTEX_SHADER, vsText);
        const fragmentShader = WebglUtils.loadShader(gl, gl.FRAGMENT_SHADER, fsText);
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

    static createDataTexture(gl, imageData, interpolated) {
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

    static createDataTextures(gl, data, colorScale, interpolated) {
        const tt = performance.now();
        const textures = {};
        const colors = colorScale.domain([data.max, data.min]);
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        
        for (let ts of data.timestamps) {
            const imageData = WebglUtils.createImageData(ctx, data.width, data.height, data.records[ts], colors);
            textures[ts] = WebglUtils.createDataTexture(gl, imageData, interpolated);
        }
        console.log('creating data textures', performance.now() - tt, 'ms');
        return textures;
    }

    //  todo: avoid so many calls to chromajs(colorScale) for speedup
    //  maybe use webworker for other than first texture (color speedup might be enough)
    static createImageData(context, width, height, data, colorScale) {
        const imageData = context.createImageData(width, height);
        const keys = Object.keys(data);
    
        let index = 0;
        for (let key in keys) {
            for (let entry of data[keys[key]]) {
                const color = colorScale(entry[1]).rgb();
                imageData.data[index]     = color[0];
                imageData.data[index + 1] = color[1];
                imageData.data[index + 2] = color[2];
                imageData.data[index + 3] = 255;
                index += 4;
            } 
        }
        return imageData;
    }

}