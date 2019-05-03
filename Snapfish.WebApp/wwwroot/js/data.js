
// TODO: Make more general?

class DataContainer {
    constructor(csvPath) {
        this.dataPromise = this.parseCsv(csvPath);
    }

    getData() {
        return this.dataPromise;
    }

    async parseCsv(path) {
        const dataPromise = new Promise(function(resolve, reject) {
            Papa.parse(path, {
                download: true,
                skipEmptyLines: true,
                complete: function(results) {
                    const data = DataContainer.createDataJson(results.data);
                    resolve(data);
                }
            });
        });

        return dataPromise;
    }

    static createDataJson(papaArray) {
        const data = {};
        const headerJson = {};
        const headerArray = papaArray.shift();

        for (let i in headerArray)
            headerJson[headerArray[i]] = i;
        
            data.headers = headerJson;

        const jsonData = DataContainer.dataArrayToJson(papaArray, headerJson);
        data.records = jsonData.data;
        data.timestamps = jsonData.metadata.timestamps;
        data.width = jsonData.metadata.width;
        data.height = jsonData.metadata.height;
        data.min = jsonData.metadata.min;
        data.max = jsonData.metadata.max;

        return data;
    }

    static dataArrayToJson(dataArray, headers) {
        const t0 = performance.now();
        let json = {}
        let min = Infinity;
        let max = -Infinity;

        for (let i = 0; i < dataArray.length; i++) {
            const sv = dataArray[i][headers.sv];
            const range = dataArray[i][headers.range_bin];
            const frequency = dataArray[i][headers.frequency];
            const time = dataArray[i][headers.ping_time];

            if (!json[time]) json[time] = {};
            if (!json[time][frequency]) json[time][frequency] = [];

            // js is having trouble with the high precision floats from the data..
            if (Math.floor(sv) > max) max = Math.floor(sv);
            if (Math.floor(sv) < min) min = Math.floor(sv);
    
            json[time][frequency].push([range, sv]);
        }

        const timeKeys = Object.keys(json);
        const frequencyKeys = Object.keys(json[timeKeys[0]]);
        const height = Object.keys(json[timeKeys[0]]).length;
        const width = json[timeKeys[0]][frequencyKeys[0]].length;

        const metadata = {min: min, max: max, width: width, height: height, timestamps: timeKeys};

        console.log('making data json:', performance.now() - t0);
    
        return {data: json, metadata: metadata};
    }
}