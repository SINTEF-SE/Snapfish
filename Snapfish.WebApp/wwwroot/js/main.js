const dataContainer = new DataContainer('data.csv');
const colors = chroma.scale('Spectral');
const plot = new Plot('sv-plot', 'SV Plot', dataContainer, colors);

const backscatterDataContainer = new DataContainer('backscatter.csv');
const backscatterPlot = new Plot('backscatter-plot', 'Backscatter Plot', backscatterDataContainer, colors);

const timeControl = new TimeControl('time-controller', [plot, backscatterPlot]);
timeControl.initialize();