const colors = chroma.scale('Spectral');

const dataContainer = new DataContainer('data/data.csv');

const svPlot = new Plot({
    domId: 'backscatter-plot',
    title: 'Sv Plot',
    data: dataContainer,
    xAxis: 'ping_time',
    yAxis: 'range_bin',
    value: 'sv',
    splitValue: 'frequency',
    colors: colors
});
