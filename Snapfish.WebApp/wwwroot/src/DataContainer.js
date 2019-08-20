class DataContainer {
    
    constructor(path) {
        this.dataPromise = this.parseJson(path);
    }

    getDataContainer() {
        return this.dataPromise;
    }

    async parseJson(path) {
        const dataPromise = new Promise(function(resolve, reject) {
            fetch(path)
                .then(response => {
                    return response.json()
                })
                .then(data => {
                    // console.log(data);
                    resolve(data);
                })
                .catch(e => {
                    console.log('fetching json data failed:', e);
                });
        });

        return dataPromise;
    }

}