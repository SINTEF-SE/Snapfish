
class TimeControl {
    constructor(containerId, plots) {
        this.container = document.getElementById(containerId);
        this.plots = plots;
        this.playing = false;
    }

    async initialize() {
        this.plots[0].ready.then(() => {
            this.currentTimestamp = this.plots[0].data.timestamps[0];
        
            this.addBaseHtml();
            this.addTimeSlider();
        });
    }

    getPlayButton() {
        const button = document.createElement('button');
        button.innerHTML = 'Play';
        button.onclick = () => this.togglePlay();
        return button;
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

    play(index) {
        const i = index ? index : 0;

        for (let plot of this.plots) {
            this.slider.value = i;
            plot.slider.value = i;
            plot.setTimestamp(i, true, true);
        }
        const playFunction = this.play.bind(this);
        if (i+1 < this.plots[0].data.timestamps[0].length) {
            // uncomment for realtime playback
            //const delta = this.data.timestamps[i+1] - this.data.timestamps[i];
            const delta = 650;
            this.playTimeout = setTimeout(function(){ playFunction(i+1) }, delta);
        }
    }

    addTimeSlider() {
        const container = this.container.querySelector('.slider-col');
        const slider = document.createElement('input');
        slider.type = 'range';
        slider.setAttribute('type', 'range');
        slider.setAttribute('min', '0');
        slider.setAttribute('max', this.plots[0].data.timestamps.length - 1);
        slider.setAttribute('value', '0');
        slider.style.width = '100%';
        slider.oninput = (e) => {
            for (let plot of this.plots)
                plot.setTimestamp(e.target.value, true, true);
        }

        container.appendChild(slider);

        this.slider = slider;
        this.playButton = this.getPlayButton();
        this.container.querySelector('.play-button-col').appendChild(this.playButton);
    }

    addBaseHtml() {
        this.container.innerHTML = `
            <div class='slider-row'>
                <div class='play-button-col'></div>
                <div class='slider-col'></div>
            </div>
        `;
    }
}