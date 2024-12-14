class Api {
    static getFileLink(filePath) {
        return `/api/download/${filePath}`;
    }

    static async getFilesList() {
        await this.#sleep(2000);
        const fakeResponse = {
            files: [
                { path: "file1", size: 11 },
                { path: "somedir/file2", size: 22 },
            ]
        }

        if (this.#randomIntFromInterval(1, 2) === 1)
            return [undefined, "Error get files list."];
        else
            return [fakeResponse, undefined];
    }

    static async uploadFile(file) {
        await this.#sleep(2000);
        const fakeResponse = {
            fileName: file.name
        }

        if (this.#randomIntFromInterval(1, 2) === 1)
            return [undefined, "Error upload file."];
        else
            return [fakeResponse, undefined];
    }

    static #randomIntFromInterval(min, max) {
        return Math.floor(Math.random() * (max - min + 1) + min);
    }

    static #sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}
