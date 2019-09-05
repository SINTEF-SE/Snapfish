// const dataContainer = new DataContainer('http://localhost:5000/api/Snap/1');
//const dataContainer = new DataContainer('/snap.json');
//const dataContainer = new DataContainer('https://10.218.157.107:5003/api/snap/' + snapId);
// In current test environment, port 5001 is used locally while 5003 is used for external calls (e.g. from tablet / phone)
const dataContainer = (window.location.hostname.includes('localhost')) ?
    new DataContainer('https://' + window.location.hostname + ':5001/api/snap/' + snapId) :
    new DataContainer('https://' + window.location.hostname + ':5003/api/snap/' + snapId);

const svPlot = new Plot({
    domId: 'snap',
    title: 'Echogram Snap',
    data: dataContainer
});
