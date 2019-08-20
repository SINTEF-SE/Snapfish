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
        this.ready = new Promise((resolve) => {
            resolve(this.initialize());
        });
    }

    async initialize() {
        this.data = await this.dataContainer.getDataContainer();

        this.canvasSetup();
        this.containerSetup();
        await this.webglSetup();
        this.draw();
        return true;
    }

    canvasSetup() {
        this.canvas = document.createElement('canvas');
        this.canvas.className = 'gl-plot-canvas';
        this.canvas.id = this.canvasId;
        
        this.canvas.onmousedown = (e) => {
            this.canvas.style.cursor = 'grabbing';
            this.mouseDown = true;
            this.lastClientY = e.clientY;
        }
        window.addEventListener('mouseup', () => {
            this.mouseDown = false;
            this.canvas.style.cursor = 'grab';
        });
        window.addEventListener('mousemove', (e) => this.handlePan(e));
        window.addEventListener('wheel', (e) => this.handleZoom(e));
    }

    containerSetup() {
        this.addBaseHtml();

        // todo: move styling somewhere else
        this.container.style.width = '340px';
        this.container.style.height = '410px';
        this.container.querySelector('.canvas-div').appendChild(this.canvas);

        this.setTitle();
        this.setYAxis();
        // this.setXAxis();
        this.addInterpolationToggle();
        this.addRangeSlider();
        this.setColorScale();

        const observer = new MutationObserver((mutations) => this.draw(true));
        observer.observe(this.container, { attributes: true, attributeFilter: ['style']});
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

        this.dataTexture = WebGLUtil.createDataTexture(gl, this.data, this.slider.noUiSlider.get(), this.interpolated);
        this.program = program;
        this.gl = gl;
    }

    draw(throttle) {
        if (throttle && Date.now() - this.preDraw < 45)
            return;
        
        const gl = this.gl;
        gl.clear(gl.COLOR_BUFFER_BIT);

        const width = parseFloat(this.container.style.width);
        const height = parseFloat(this.container.style.height);
        this.canvas.width = width;
        this.canvas.height = height;
        this.checkViewportBounds();
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

        gl.drawArrays(gl.TRIANGLES, 0, 6);

        this.updateYLabels();

        if (throttle)
            this.preDraw = Date.now();
    }

    // todo: cleanup
    handlePan(event) {
        if (!this.mouseDown || this.lastClientY == event.clientY)
            return;

        this.viewportOffset += this.lastClientY - event.clientY;
        
        this.lastClientY = event.clientY;
        this.draw(true);
    }

    handleZoom(event) {
        if (event.target.id != this.canvasId)
            return;

        const originalExtent = this.canvas.height * this.zoomLevel;
        this.zoomLevel += event.deltaY > 0 ? -0.045 : 0.045;
        this.zoomLevel = this.zoomLevel < 1 ? 1 : this.zoomLevel;

        const rect = this.canvas.getBoundingClientRect();
        const yPct = 1 - (event.clientY - rect.top) / this.canvas.height;

        this.viewportOffset += yPct * (originalExtent - this.canvas.height * this.zoomLevel);

        this.draw();
    }

    checkViewportBounds() {
        // avoid panning beyond bottom
        this.viewportOffset = this.viewportOffset > 0
            ? 0
            : this.viewportOffset;

        // avoid panning beyond top - todo: clean up
        this.viewportOffset = this.canvas.height - this.viewportOffset > this.canvas.height * this.zoomLevel
            ? this.canvas.height - this.canvas.height * this.zoomLevel
            : this.viewportOffset;
    }

    updateYLabels() {
        const viewport = this.gl.getParameter(this.gl.VIEWPORT);
        const dataHeight = this.data.Slices[0].Range;
        const diff = viewport[3] - this.canvas.height;
        const diffRatio = diff / viewport[3];
        const offsetRatio = Math.abs(viewport[1]) / viewport[3];
        const top = dataHeight - diffRatio * dataHeight + dataHeight * offsetRatio;
        const bottom = top - (dataHeight - dataHeight * diffRatio);
        const labels = this.container.querySelector('.y-labels').children;
        const range = top - bottom;
        for (let i = 1; i < labels.length - 1; i++) {
            labels[i].innerHTML = dataHeight - Math.round((range / 5) * (5-i) + bottom);
        }
    }

    // TODO: Reimplement to look similar to EK80
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
        const labels = document.createElement('div');
        const spacer = document.createElement('div');
        const axis = this.container.querySelector('.y-axis');

        for (let i = 5; i > -1; i--) {
            const label = document.createElement('div');
            labels.appendChild(label);
        }
        
        spacer.className = 'y-axis-spacer';
        labels.className  = 'y-labels';
        axis.appendChild(labels);
        axis.appendChild(spacer);
    }

    setTitle() {
        this.container.querySelector('.plot-title').innerHTML = this.title;
    }

    setColorScale() {
        const container = this.container.querySelector('.color-scale');
        const height = 30;
        const colorScale = document.createElement('img');
        colorScale.src = '/src/color-scale.png';
        colorScale.style.width = "100%";
        colorScale.height = height;

        container.insertBefore(colorScale, container.firstChild);
        const filterRange = this.slider.noUiSlider.get();
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
        const slider = this.container.querySelector('.filter-slider');

        // TODO: Get proper power level thresholding and min/max values from actual data
        const min = -9500;
        const max = -1000;

        noUiSlider.create(slider, {
            start: [min, max],
            tooltips: [true, true],
            connect: true,
            range: {
                'min': min,
                'max': max
            }
        });

        slider.noUiSlider.on('set', (range) => this.filterValues(range));

        this.slider = slider;
    }

    addInterpolationToggle() {
        const container = this.container.querySelector('.interpolated-toggle');
        const toggle = document.createElement('input');
        toggle.type = 'checkbox';
        toggle.onchange = (e) => this.setInterpolated(e.target.value, true);
        container.appendChild(toggle);
        container.appendChild(document.createTextNode('Interpolated'));
    }

    setInterpolated(interpolated) {
        const filterRange = this.slider.noUiSlider.get();
        this.interpolated = !this.interpolated;  // TODO: FIX!

        this.dataTexture = WebGLUtil.createDataTexture(this.gl, this.data, filterRange, this.interpolated);

        this.draw();
    }

    filterValues() {
        const filterRange = this.slider.noUiSlider.get();
        this.dataTexture = WebGLUtil.createDataTexture(this.gl, this.data, filterRange, this.interpolated);
        this.draw();
        this.setColorScaleLabels(filterRange[0], filterRange[1]);
    }

    addBaseHtml() {
        this.container.innerHTML = `
            <div class='plot-title'></div>
            <div class='plot-time'></div>
            <div class='chart-row'>
                <div class='y-axis'></div>
                <div class='canvas-col'>
                    <div class='canvas-div'></div>
                    <div class='x-axis'></div>
                </div>
            </div>
            <div class='slider-row'>
                <div class='play-button-col'></div>
                <div class='slider-col'></div>
            </div>
            <div class='interpolation-row'>
                <div class="split-dropdown"></div>
                <label class='interpolated-toggle'></label>
            </div>
            <div class='filter-row'>
                <div class="filter-slider"></div>
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
