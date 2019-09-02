// const dataContainer = new DataContainer('http://localhost:5000/api/Snap/1');
const dataContainer = new DataContainer('/snap.json');


const svPlot = new Plot({
    domId: 'snap',
    title: 'Echogram Snap',
    data: dataContainer
});
