const vertexShaderText = `
    precision highp float;

    attribute vec2 pos;
    varying vec2 dataPos;

    void main(void) {
        dataPos = pos;

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
        uniform float threshold;
        uniform float max;
        uniform float xOffset;

        varying vec2 dataPos;

        float getValue(vec4 color)
        {
            float r = color.r;
            float g = color.g * 255.0 * 65536.0;
            float b = color.b * 255.0 * 256.0;
            float a = color.a * 255.0;
            if(r == 0.0)
                return -1.0 * (g + b + a);
            else
                return g + b + a;
        }

        void main(void) {
            vec2 pos = vec2(dataPos.x + xOffset, dataPos.y);

            if(pos.x > 1.0 || pos.x < 0.0)
                discard;

            vec4 dataColor = texture2D(data, 1.0 - pos);
            float value = getValue(dataColor);
            vec4 color;

            if (value < threshold)
                color = vec4(1.0, 1.0, 1.0, 1.0);
            else {
                float pct = 1.0 - (abs(value) / abs(abs(max) - abs(threshold)));
                color = texture2D(colorScale, vec2(pct, 0.5));
            }

            gl_FragColor = color;
        }
`;

const vertexShaderText2 = `
    precision highp float;

    attribute vec2 pos;
    varying vec2 dataPos;

    void main(void) {
        dataPos = pos;

        gl_Position= vec4(
            1.0 - 2.0 * pos,
            0.0, 1.0
        );
    }
`;

const fragmentShaderText2 = `
        precision highp float;

        uniform sampler2D data;
        uniform sampler2D colorScale;
        uniform float zoomLevel;
        uniform float threshold;
        uniform float max;
        uniform float xOffset;

        varying vec2 dataPos;

        float getValue(vec4 color)
        {
            float r = color.r;
            float g = color.g * 255.0 * 65536.0;
            float b = color.b * 255.0 * 256.0;
            float a = color.a * 255.0;
            if(r == 0.0)
                return -1.0 * (g + b + a);
            else
                return g + b + a;
        }

        void main(void) {
            vec4 dataColor = texture2D(data, vec2(1.0 - xOffset, 1.0 - dataPos.y));
            float value = getValue(dataColor);
            vec4 color;

            if(xOffset > 1.0 || xOffset < 0.0)
                discard;

            if (value > threshold) {
                float pct = 1.0 - (abs(value) / abs(abs(max) - abs(threshold)));

                if(1.0 - dataPos.x > pct)
                    color = vec4(1.0, 1.0, 1.0, 1.0);
                else
                    color = texture2D(colorScale, vec2(pct, 0.5));
            }
            else
                color = vec4(1.0, 1.0, 1.0, 1.0);

            gl_FragColor = color;
        }
`;
