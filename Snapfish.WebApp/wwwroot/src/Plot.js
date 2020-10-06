class Plot {
    constructor(options) {
        this.options = options;
        this.container = document.getElementById(options.domId);
        this.canvasId = options.domId + '-canvas';
        this.title = options.title;
        this.dataContainer = options.data;
        this.interpolated = false;
        this.zoomLevel = 1;
        this.viewportOffset = 0;
        this.mouseDown = false;

        this.xOffset = 0;

        this.ready = new Promise((resolve) => {
            resolve(this.initialize());
        });
    }

    async initialize() {
        this.data = await this.dataContainer.getDataContainer();

        this.canvasSetup();
        this.canvas2Setup();
        this.containerSetup();
        await this.webglSetup();
        this.pzSetup();
        this.draw();
        this.updateBiomass();
        return true;
    }

    // Main canvas to the left
    canvasSetup() {
        this.canvas = document.createElement('canvas');
        this.canvas.className = 'gl-plot-canvas';
        this.canvas.id = this.canvasId;
    }

    // Canvas on right hand side showing details of last slice
    canvas2Setup() {
        this.canvas2 = document.createElement('canvas');
        this.canvas2.className = 'gl-plot-canvas-2';
        this.canvas2.id = this.canvasId + '-2';
        this.canvas2.width = 80;
    }

    containerSetup() {
        this.addBaseHtml();
        this.canvasParent = this.container.querySelector('.canvas-div');
        this.canvasParent.appendChild(this.canvas);
        this.container.querySelector('.canvas-2-div').appendChild(this.canvas2);
        this.resizeCanvas();

        this.setYAxis();
        // this.setXAxis();
        // this.addInterpolationToggle();
        this.addRangeSlider();
        this.setColorScale();

        // make canvas size responsive
        new ResizeObserver(_ => {
            this.resizeCanvas();
            this.draw(true)
        }).observe(this.canvasParent);
    }

    resizeCanvas() {
        const height = Math.floor(this.canvasParent.clientHeight);
        const width = this.data.NumberOfSlices;  // Math.floor(this.canvasParent.clientWidth);
        this.canvas.width = width;
        this.canvas.height = height;
        this.canvas2.height = height;
    }

    async webglSetup() {
        const gl = this.canvas.getContext('webgl');

        if (gl === null)
            console.log("Unable to initialize WebGL");

        gl.clearColor(0.0, 0.0, 0.0, 0.0);
        gl.clear(gl.COLOR_BUFFER_BIT);

        const program = WebGLUtil.createShaderProgram(gl, vertexShaderText, fragmentShaderText);
        gl.useProgram(program);

        const aPosLocation = gl.getAttribLocation(program, 'pos');
        gl.bindBuffer(gl.ARRAY_BUFFER, WebGLUtil.createSquareBuffer(gl));
        gl.vertexAttribPointer(aPosLocation, 2, gl.FLOAT, false, 0, 0);
        gl.enableVertexAttribArray(aPosLocation);

        this.colorScale = await WebGLUtil.createDefaultColorScaleTexture(gl);

        this.dataTexture = WebGLUtil.createDataTexture(gl, this.data, this.interpolated);
        this.program = program;
        this.gl = gl;

        await this.webgl2Setup();
    }

    async webgl2Setup() {
        const gl = this.canvas2.getContext('webgl');

        gl.clearColor(0.0, 0.0, 0.0, 0.0);
        gl.clear(gl.COLOR_BUFFER_BIT);

        const program = WebGLUtil.createShaderProgram(gl, vertexShaderText2, fragmentShaderText2);
        gl.useProgram(program);

        const aPosLocation = gl.getAttribLocation(program, 'pos');
        gl.bindBuffer(gl.ARRAY_BUFFER, WebGLUtil.createSquareBuffer(gl));
        gl.vertexAttribPointer(aPosLocation, 2, gl.FLOAT, false, 0, 0);
        gl.enableVertexAttribArray(aPosLocation);

        this.colorScale2 = await WebGLUtil.createDefaultColorScaleTexture(gl);
        this.dataTexture2 = WebGLUtil.createDataTexture(gl, this.data, this.interpolated);
        this.program2 = program;
        this.gl2 = gl;
    }

    // Set up pan and zoom handling
    pzSetup() {
        d3.select("#" + this.canvasId).call(d3.zoom()
            .scaleExtent([1, 8])
            .translateExtent([[-this.canvas.clientWidth, 0], [this.canvas.clientWidth, this.canvas.clientHeight]])
            .constrain((transform) => {
                if (this.zoomLevel != transform.k) {
                    transform.x = this.xOffset;
                } else {
                    transform.x = Math.min(transform.x, this.data.NumberOfSlices - 1);
                    transform.x = Math.max(transform.x, 0);
                }
                transform.y = Math.max(transform.y, 0);
                transform.y = Math.min(this.canvas.height * transform.k - this.canvas.height, transform.y)
                return transform;
            })
            .on("zoom", ({transform}) => this.zoomed(transform)));
    }

    // Updates values for drawing based on transform and redraws canvases and biomass
    zoomed(transform) {
        this.xOffset = transform.x;
        this.viewportOffset = -transform.y;
        this.zoomLevel = transform.k;
        this.draw();
        this.updateBiomass();
    }

    draw(throttle) {
        if (this.gl == undefined || this.gl2 == undefined || (throttle && performance.now() - this.preDraw < 20))
            return;

        const gl = this.gl;
        gl.clear(gl.COLOR_BUFFER_BIT);

        const width = this.canvas.width;
        const height = this.canvas.height;
        // this.checkViewportBounds();
        gl.viewport(0, this.viewportOffset, width, height * this.zoomLevel);

        const texture = this.dataTexture;
        const uDataLocation = gl.getUniformLocation(this.program, 'data');
        gl.uniform1i(uDataLocation, 0);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);

        const uColorScaleLocation = gl.getUniformLocation(this.program, 'colorScale');
        gl.activeTexture(this.gl.TEXTURE0 + 1);
        gl.bindTexture(this.gl.TEXTURE_2D, this.colorScale);
        gl.uniform1i(uColorScaleLocation, 1);

        const uThresholdLocation = gl.getUniformLocation(this.program, 'threshold');
        gl.uniform1f(uThresholdLocation, parseFloat(this.slider.value));
        const uMaxLocation = gl.getUniformLocation(this.program, 'max');
        gl.uniform1f(uMaxLocation, -1000);
        const uXOffsetLocation = gl.getUniformLocation(this.program, 'xOffset');
        gl.uniform1f(uXOffsetLocation, (this.xOffset / this.data.NumberOfSlices));

        gl.drawArrays(gl.TRIANGLES, 0, 6);

        this.updateYLabels();

        this.draw2();

        if (throttle)
            this.preDraw = performance.now();
    }

    draw2() {
        const gl = this.gl2;
        gl.clear(gl.COLOR_BUFFER_BIT);

        const width = this.canvas2.width;
        const height = this.canvas2.height;
        this.checkViewportBounds();
        gl.viewport(0, this.viewportOffset, width, height * this.zoomLevel);

        const texture = this.dataTexture2;
        const uDataLocation = gl.getUniformLocation(this.program2, 'data');
        gl.uniform1i(uDataLocation, 0);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);

        const uColorScaleLocation = gl.getUniformLocation(this.program2, 'colorScale');
        gl.activeTexture(this.gl.TEXTURE0 + 1);
        gl.bindTexture(this.gl.TEXTURE_2D, this.colorScale2);
        gl.uniform1i(uColorScaleLocation, 1);

        const uThresholdLocation = gl.getUniformLocation(this.program2, 'threshold');
        gl.uniform1f(uThresholdLocation, parseFloat(this.slider.value));
        const uMaxLocation = gl.getUniformLocation(this.program2, 'max');
        gl.uniform1f(uMaxLocation, -1000);
        const uXOffsetLocation = gl.getUniformLocation(this.program2, 'xOffset');
        const halfPixelWidth = (1 / this.data.NumberOfSlices) / 2;
        gl.uniform1f(uXOffsetLocation, (this.xOffset / this.data.NumberOfSlices) + halfPixelWidth);

        gl.drawArrays(gl.TRIANGLES, 0, 6);
    }

    updateBiomass() {
        const dataIndex = (this.data.NumberOfSlices - 1) - Math.round(this.xOffset);
        this.container.querySelector(".current-biomass").innerHTML = this.data.Slices[dataIndex].Biomass;
    }

    checkViewportBounds() {
        // avoid panning beyond bottom
        this.viewportOffset = this.viewportOffset > 0 ?
            0 :
            this.viewportOffset;

        // avoid panning beyond top - todo: clean up
        this.viewportOffset = this.canvas.height - this.viewportOffset > this.canvas.height * this.zoomLevel ?
            this.canvas.height - this.canvas.height * this.zoomLevel :
            this.viewportOffset;
    }

    updateYLabels() {
        const viewport = this.gl.getParameter(this.gl.VIEWPORT);
        const dataHeight = this.data.Slices[0].Range;
        const diff = viewport[3] - this.canvas.height;
        const diffRatio = diff / viewport[3];
        const offsetRatio = Math.abs(viewport[1]) / viewport[3];
        const top = dataHeight - diffRatio * dataHeight + dataHeight * offsetRatio;
        const bottom = top - (dataHeight - dataHeight * diffRatio);

        // a bit hacky...
        this.container.querySelector('.y-axis').style.height = this.canvas.height + "px";

        const labels = this.container.querySelector('.y-axis').children;
        const range = top - bottom;
        for (let i = 1; i < labels.length - 1; i++) {
            labels[i].innerHTML = dataHeight - Math.round((range / 5) * (5 - i) + bottom) + "m";
        }
    }

    // TODO: Reimplement to look similar to EK80 (not used currently)
    setXAxis() {
        const axis = document.createElement('div');
        axis.className = 'x-labels';
        const xValues = this.dataContainer.groups.x.top(Infinity).map((uniqueValue) => uniqueValue.key).sort();
        for (let value of xValues) {
            const label = document.createElement('div');
            label.innerHTML = new Date(Math.floor(value)); // TODO: Add setting for xAxis of timestamps or not
            axis.appendChild(label);
        }
        this.container.querySelector('.x-axis').appendChild(axis);
    }

    setYAxis() {
        const labels = this.container.querySelector('.y-axis');

        for (let i = 5; i > -1; i--) {
            const label = document.createElement('div');
            labels.appendChild(label);
        }
    }

    setColorScale() {
        const container = this.container.querySelector('.color-scale');
        const height = 30;
        const colorScale = document.createElement('img');
        colorScale.src = '/src/color-scale.png';
        colorScale.style.width = "100%";
        colorScale.height = height;

        container.insertBefore(colorScale, container.firstChild);
        const filterRange = [this.slider.value, this.slider.max];
        this.setColorScaleLabels(filterRange[0], filterRange[1]);
    }

    setColorScaleLabels(min, max) {
        const labels = this.container.querySelector('.color-scale-labels').children;
        min = parseFloat(min);
        max = parseFloat(max);
        const mid = (max - min) / 2 + min;
        labels[0].innerHTML = min.toFixed(2);
        labels[1].innerHTML = mid.toFixed(2);
        labels[2].innerHTML = max.toFixed(2);
    }

    addRangeSlider() {
        const slider = document.createElement("input");
        const minus = this.container.querySelector(".filter-minus");
        const plus = this.container.querySelector(".filter-plus");
        slider.type = "range";
        slider.min = -9500;
        slider.max = -1000;
        slider.value = slider.min;

        slider.oninput = () => {
            this.draw();
            this.setColorScaleLabels(slider.value, slider.max);
        };

        minus.onclick = () => {
            slider.value--;
            this.draw();
            this.setColorScaleLabels(slider.value, slider.max);
        };

        plus.onclick = () => {
            slider.value++;
            this.draw();
            this.setColorScaleLabels(slider.value, slider.max);
        };

        this.container.querySelector('.filter-slider').appendChild(slider);
        this.slider = slider;
    }

    // not used currently
    addInterpolationToggle() {
        const container = this.container.querySelector('.interpolated-toggle');
        const toggle = document.createElement('input');
        toggle.type = 'checkbox';
        toggle.onchange = () => this.setInterpolated(toggle.checked);
        container.appendChild(toggle);
        container.appendChild(document.createTextNode('Interpolated'));
    }
    // not used currently
    setInterpolated(interpolated) {
        this.interpolated = interpolated;
        this.dataTexture = WebGLUtil.createDataTexture(this.gl, this.data, this.interpolated);
        this.draw();
    }

    addBaseHtml() {
        this.container.innerHTML = `
            <div class="biomass-row">
                Biomass: <span class="current-biomass"></span>
            </div>
            <div class='chart-row'>
                <div class='echogram-col'>
                    <div class='y-axis'></div>
                    <div class='canvas-div'></div>
                    <div class='x-axis'></div>
                </div>
                <div class="canvas-splitter"></div>
                <div class='latest-slice-col'>
                    <div class='canvas-div canvas-2-div'></div>
                </div>
            </div>
            <div class='filter-row'>
                <div class="filter-minus">-</div>
                <div class="filter-slider"></div>
                <div class="filter-plus">+</div>
            </div>
            <div class='color-scale-row'>
                <div class='color-scale'>
                    <div class='color-scale-labels'>
                        <div class='color-scale-label'></div>
                        <div class='color-scale-label'></div>
                        <div class='color-scale-label'></div>
                    </div>
                </div>
            <div>
        `;
    }
}