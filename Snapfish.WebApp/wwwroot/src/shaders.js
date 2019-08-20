const vertexShaderText = `
    precision highp float;

    attribute vec2 pos;
    varying vec2 dataPos;

    void main(void) {
        dataPos = pos; //vec2(pos.y, pos.x);

        gl_Position= vec4(
            1.0 - 2.0 * pos,
            0.0, 1.0
        );
    }
`;

const fragmentShaderText = `
        precision highp float;

        uniform sampler2D data;
        uniform sampler2D colorScale;
        uniform float zoomLevel;

        varying vec2 dataPos;

        void main(void) {
            // float x = mix(0.0, zoomLevel, dataPos.x);

            // vec2 colorPos = vec2(
            //     x,
            //     dataPos.y
            // );

            vec4 dataColor = texture2D(data, 1.0 - dataPos);
            vec4 color;

            if (dataColor.a == 0.0)
                color = vec4(1.0, 1.0, 1.0, 1.0);
            else
                color = texture2D(colorScale, vec2(dataColor.r, 0.5));

            gl_FragColor = color;
        }
`;
