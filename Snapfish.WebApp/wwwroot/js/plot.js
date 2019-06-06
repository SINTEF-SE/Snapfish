
// todo: move some functionality to mixin/util/other class

class Plot {
    constructor(containerId, title, dataContainer, colorScale) {
        this.container = document.getElementById(containerId.domId);
        this.canvasId = containerId.domId + '-canvas';
        this.title = title;
        this.dataContainer = containerId.data;
        this.colorScale = containerId.colors;
        this.playing = false;
        this.zoomLevel = 1;
        this.viewportOffset = 0;
        this.mouseDown = false;
        this.ready = new Promise((resolve) => {
            resolve(this.initialize());
        });
    }

    async initialize() {
        this.data = await this.dataContainer.getData();
        this.currentTimestamp = this.data.timestamps[0];
        
        this.canvasSetup();
        this.containerSetup();
        this.webglSetup();
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

        // todo: move
        this.container.style.width = '680px';
        this.container.style.height = '420px';
        this.container.querySelector('.canvas-div').appendChild(this.canvas);

        this.setTitle();
        this.setYAxis();
        this.setXAxis();
        this.addTimeSlider();
        this.addInterpolationToggle();
        this.setColorScale();

        // Uncomment this to get a dropdown of timestamps (alternative/supplement to timeslider)
        // this.container.appendChild(this.getTimeDropdown());

        const observer = new MutationObserver((mutations) => this.draw(true));
        observer.observe(this.container, { attributes: true, attributeFilter: ['style']});
    }

    webglSetup() {
        const gl = this.canvas.getContext('webgl');

        if (gl === null)
            console.log("Unable to initialize WebGL");
    
        gl.clearColor(0.0, 0.0, 0.0, 0.0);
        gl.clear(gl.COLOR_BUFFER_BIT);
    
        const program = WebglUtils.createShaderProgram(gl, vertexShaderText, fragmentShaderText);
        gl.useProgram(program);

        const uPosLocation = gl.getAttribLocation(program, 'pos');
        gl.bindBuffer(gl.ARRAY_BUFFER, WebglUtils.createSquareBuffer(gl));
        gl.vertexAttribPointer(uPosLocation, 2, gl.FLOAT, false, 0, 0);
        gl.enableVertexAttribArray(uPosLocation);

        this.dataTextures = WebglUtils.createDataTextures(gl, this.data, this.colorScale, false);
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

        const texture = this.dataTextures[this.currentTimestamp];
        const uDataLocation = gl.getUniformLocation(this.program, 'data');
        gl.uniform1i(uDataLocation, 0);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);

        // Alternative implementation of zooming
        // const uZoomLevelLocation = gl.getUniformLocation(this.program, 'zoomLevel');
        // gl.uniform1f(uZoomLevelLocation, this.zoomLevel);

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
        const dataHeight = this.data.width;
        const diff = viewport[3] - this.canvas.height;
        const diffRatio = diff / viewport[3];
        const offsetRatio = Math.abs(viewport[1]) / viewport[3];
        const top = dataHeight - diffRatio * dataHeight + dataHeight * offsetRatio;
        const bottom = top - (dataHeight - dataHeight * diffRatio);
        const labels = this.container.querySelector('.y-labels').children;
        const range = top - bottom;
        for (let i = 0; i < labels.length; i++) {
            labels[i].innerHTML = Math.round((range / 5) * (5-i) + bottom);
        }
    }

    // todo: support dynamically set interval between frames
    play(index) {
        const i = index ? index : 0;
        this.slider.value = i;
        this.setTimestamp(i, true);
        const playFunction = this.play.bind(this);
        if (i+1 < this.data.timestamps.length) {
            // uncomment for realtime playback
            //const delta = this.data.timestamps[i+1] - this.data.timestamps[i];
            const delta = 650;
            this.playTimeout = setTimeout(function(){ playFunction(i+1) }, delta);
        }
    }

    togglePlay() {
        if (this.playing) {
            this.playButton.innerHTML = 'Play';
            clearTimeout(this.playTimeout);
        } else {
            this.playButton.innerHTML = 'Stop';
            this.play();
        }
        this.playing = !this.playing;
    }

    setTimestamp(timestamp, isIndex, externalChange) {
        if (this.currentTimestamp == timestamp) return;
        if (isIndex)
            this.currentTimestamp = this.data.timestamps[timestamp];
        else
            this.currentTimestamp = timestamp;

        if (externalChange)
            this.slider.value = timestamp;
        
        this.draw();
        this.setTitle();
    }

    getPlayButton() {
        const button = document.createElement('button');
        button.innerHTML = 'Play';
        button.onclick = () => this.togglePlay();
        return button;
    }

    getTimeDropdown() {
        const selector = document.createElement('select');
        for (let ts of this.data.timestamps) {
            const option = document.createElement('option');
            const time = new Date(parseFloat(ts));
            option.value = ts;
            option.text = time.toUTCString();
            selector.appendChild(option);
        }
        const changeFunction = this.setTimestamp.bind(this);
        selector.onchange = function() { changeFunction(this.value); };

        return selector;
    }

    addTimeSlider() {
        const container = this.container.querySelector('.slider-col');
        const slider = document.createElement('input');
        slider.type = 'range';
        slider.setAttribute('type', 'range');
        slider.setAttribute('min', '0');
        slider.setAttribute('max', this.data.timestamps.length - 1);
        slider.setAttribute('value', '0');
        slider.style.width = '100%';
        slider.oninput = (e) => this.setTimestamp(e.target.value, true);

        container.appendChild(slider);

        this.slider = slider;
        this.playButton = this.getPlayButton();
        this.container.querySelector('.play-button-col').appendChild(this.playButton);
    }

    setXAxis() {
        const axis = document.createElement('div');
        axis.className = 'x-labels';
        const frequencies = Object.keys(this.data.records[this.currentTimestamp]);
        for (let frequency of frequencies) {
            const label = document.createElement('div');
            label.innerHTML = Math.floor(frequency);
            axis.appendChild(label);
        }
        this.container.querySelector('.x-axis').appendChild(axis);
    }

    setYAxis() {
        const labels = document.createElement('div');
        const spacer = document.createElement('div');
        const axis = this.container.querySelector('.y-axis');
        const step = this.data.width / 5;

        for (let i = 5; i > -1; i--) {
            const label = document.createElement('div');
            label.innerHTML = Math.floor(0 + i * step);
            labels.appendChild(label);
        }
        
        spacer.className = 'y-axis-spacer';
        labels.className  = 'y-labels';
        axis.appendChild(labels);
        axis.appendChild(spacer);
    }

    setTitle() {
        this.container.querySelector('.plot-title').innerHTML = this.title;
        this.container.querySelector('.plot-time').innerHTML = new Date(Math.floor(this.currentTimestamp)).toUTCString();
    }

    setColorScale() {
        const container = this.container.querySelector('.color-scale');
        const width = 400;
        const height = 1;
        const canvas = document.createElement('canvas');
        canvas.className = 'color-scale-canvas';
        canvas.width = width;
        canvas.height = height;
        const ctx = canvas.getContext('2d');
        const colors = this.colorScale.colors(40);
        let gradient = ctx.createLinearGradient(0, 0, width, 0);

        // todo: handle reversing of color scales properly
        for (let i in colors)
            gradient.addColorStop(1 - i * 0.025, colors[i]);

        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, width, height);

        container.insertBefore(canvas, container.firstChild);
        const labels = this.container.querySelector('.color-scale-labels').children;
        const min = this.data.min;
        const max = this.data.max;
        const mid = (max - min) / 2 + min;
        labels[0].innerHTML = min;
        labels[1].innerHTML = mid;
        labels[2].innerHTML = max;
    }

    addInterpolationToggle() {
        const container = this.container.querySelector('.interpolated-toggle');
        const toggle = document.createElement('input');
        toggle.type = 'checkbox';
        toggle.onchange = (e) => this.setInterpolated(e.target.value, true);
        container.appendChild(toggle);
        container.appendChild(document.createTextNode('Interpolated'));
    }

    // TODO: Assumes non-interpolated by default - add option and handle it here
    setInterpolated(interpolated) {
        if (interpolated && !this.inactiveDataTextures)
                this.inactiveDataTextures = WebglUtils.createDataTextures(this.gl, this.data, this.colorScale, true);

        const temp = this.dataTextures;
        this.dataTextures = this.inactiveDataTextures;
        this.inactiveDataTextures = temp;
        this.draw();
    }

    addBaseHtml() {
        this.container.innerHTML = `
            <div class='plot-title'></div>
            <div class='plot-time'></div>
            <div class='chart-row'>
                <div class='y-axis'></div>
                <div class='canvas-col'>z
                    <div class='canvas-div'></div>
                    <div class='x-axis'></div>
                </div>
            </div>
            <div class='slider-row'>
                <div class='play-button-col'></div>
                <div class='slider-col'></div>
            </div>
            <div class='interpolation-row'><label class='interpolated-toggle'></label></div>
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
