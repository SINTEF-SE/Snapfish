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

    static createDataTextures(gl, dataContainer, value, colorScale, interpolated, filteredRange) {
        const textures = {};
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');

        // TODO: option for split on time
        const textureValues = dataContainer.groups.split.top(Infinity).map((uniqueValue) => uniqueValue.key).sort();
        
        // Todo: try lazy
        for (let textureValue of textureValues) {
            dataContainer.dimensions.split.filter(textureValue);
            const imageData = WebglUtils.createImageData(ctx, dataContainer, value, colorScale, filteredRange);
            textures[textureValue] = WebglUtils.createDataTexture(gl, imageData, interpolated);
            dataContainer.dimensions.split.filterAll();
        }

        return textures;
    }

    static createImageData(context, dataContainer, value, colorScale, filteredRange) {
        const width = dataContainer.metadata.width;
        const height = dataContainer.metadata.height;

        const imageData = context.createImageData(width, height);

        // TODO: Ensure that x values also come out sorted
        const data = dataContainer.dimensions.y.bottom(Infinity);
        const valueIndex = dataContainer.headers[value];

        const reversedFilteredRange = filteredRange.slice().reverse();
        const colors = colorScale.domain(reversedFilteredRange); // colorScale.domain([dataContainer.metadata.max, dataContainer.metadata.min]);
        
        let index = 0;
        for (let i = 0; i < width*height; i++) {
                const color = WebglUtils.getColor(data[i][valueIndex], colors, filteredRange);
                imageData.data[index]     = color[0];
                imageData.data[index + 1] = color[1];
                imageData.data[index + 2] = color[2];
                imageData.data[index + 3] = 255;
                index += 4;
        }
        return imageData;
    }

    static getColor(value, scale, range) {
        if (value < range[0] || value > range[1])
            return [255, 255, 255];
        return scale(value).rgb();
    }
}