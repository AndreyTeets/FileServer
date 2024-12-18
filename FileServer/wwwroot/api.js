class Api {
    static getFileLink(filePath) {
        const urlEncodedFilePath = filePath.split("/").map(x => encodeURIComponent(x)).join("/");
        return `/api/files/download/${urlEncodedFilePath}`;
    }

    static async getFilesList() {
        const url = "api/files/list";
        const params = Api.#createParams("GET");
        return await Api.#send(url, params);
    }

    static async uploadFile(fileFormData) {
        const url = "api/files/upload";
        const params = Api.#createParams("POST", fileFormData);
        return await Api.#send(url, params);
    }

    static #createParams(method, body) {
        const params = {
            method: method,
            headers: {},
            body: body
        };
        return params;
    }

    static async #send(url, params) {
        var errorText;
        const responseData = await fetch(url, params)
            .then(async response => {
                if (response.status === 200) {
                    const contentType = response.headers.get("Content-Type");
                    if (contentType && contentType.trim().startsWith("application/json"))
                        return await response.json();
                    else
                        return await response.text();
                } else {
                    const text = await response.text();
                    throw new Error(`Response status: ${response.status} ${response.statusText}. Response body: ${text}`);
                }
            })
            .catch(error => {
                errorText = `Fetch error: ${error}`;
            });
        return [responseData, errorText];
    }
}
