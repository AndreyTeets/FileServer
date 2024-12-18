class AppLogic {
    static #downloadPage;
    static #uploadPage;

    static async setDownloadPage() {
        if (!AppLogic.#downloadPage) {
            const pageData = AppLogic.#createProxifiedPageData(DownloadPageComponent);
            pageData.downloadFunc = async (file) => {
                const a = document.createElement("a");
                a.href = Api.getFileLink(file.path);
                a.target = `_blank`;
                a.click();
            };
            AppLogic.#downloadPage = new DownloadPageComponent(pageData);
        }

        AppLogic.#downloadPage.data.state = { status: { text: "Loading..." } };
        App.page = AppLogic.#downloadPage;

        const [response, errorText] = await Api.getFilesList();
        if (errorText)
            AppLogic.#downloadPage.data.state.status = { error: errorText };
        else
            AppLogic.#downloadPage.data.state = { files: response.files, status: {} };
    }

    static setUploadPage() {
        if (!AppLogic.#uploadPage) {
            const pageData = AppLogic.#createProxifiedPageData(UploadPageComponent);
            pageData.uploadFunc = async (file) => {
                const fileFormData = new FormData();
                fileFormData.append("file", file);

                pageData.state.status = { text: "Uploading..." };
                const [response, errorText] = await Api.uploadFile(fileFormData);
                if (errorText)
                    pageData.state.status = { error: errorText };
                else
                    pageData.state.status = { text: `Uploaded: ${response.createdFileName}`, reset: true };
            };
            AppLogic.#uploadPage = new UploadPageComponent(pageData);
        }

        AppLogic.#uploadPage.data.state = { status: {} };
        App.page = AppLogic.#uploadPage;
    }

    static #createProxifiedPageData(pageClass) {
        return App.proxifyObjectPropertiesForRenderOnChange(
            { state: {} },
            () => {
                return App.page instanceof pageClass;
            }
        );
    }
}
