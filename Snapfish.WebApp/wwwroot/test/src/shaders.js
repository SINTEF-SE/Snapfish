const vertexShaderText = `
    precision mediump float;

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
        precision mediump float;

        uniform sampler2D data;
        uniform float zoomLevel;

        varying vec2 dataPos;

        void main(void) {
            // float x = mix(0.0, zoomLevel, dataPos.x);

            // vec2 colorPos = vec2(
            //     x,
            //     dataPos.y
            // );

            gl_FragColor = texture2D(data, 1.0 - dataPos);
        }
`;
