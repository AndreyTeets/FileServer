class DownloadPageComponent {
    data;

    constructor(data) {
        this.data = data
    }

    create() {
        const div = document.createElement("div");
        div.appendChild(new HeaderComponent("DownloadPage:").create());

        if (this.data.state.files)
            div.appendChild(new FilesListComponent(this.data.state.files, this.data.downloadFunc).create());
        if (this.data.state.status.text)
            div.appendChild(new TextComponent(this.data.state.status.text).create());
        if (this.data.state.status.error)
            div.appendChild(new ErrorComponent(this.data.state.status.error).create());

        return div;
    }
}

class UploadPageComponent {
    #fileUploadComponent;
    data;

    constructor(data) {
        this.data = data
    }

    create() {
        const div = document.createElement("div");
        div.appendChild(new HeaderComponent("UploadPage:").create());

        if (!this.#fileUploadComponent || this.data.state.status.reset)
            this.#fileUploadComponent = new FileUploadComponent(this.data.uploadFunc).create();
        div.appendChild(this.#fileUploadComponent);

        if (this.data.state.status.text)
            div.appendChild(new TextComponent(this.data.state.status.text).create());
        if (this.data.state.status.error)
            div.appendChild(new ErrorComponent(this.data.state.status.error).create());

        return div;
    }
}
