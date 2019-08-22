const dataContainer = new DataContainer('http://localhost:5000/api/Snap/' + snapId);

const svPlot = new Plot({
    domId: 'backscatter-plot',
    title: 'Echogram Snap',
    data: dataContainer
});
