class AppComponent extends ComponentBase {
    #menuComponent;
    #downloadPageComponent;
    #uploadPageComponent;
    #authPageComponent;

    constructor() {
        super();
        this.#menuComponent = new MenuComponent();
        this.#downloadPageComponent = new DownloadPageComponent();
        this.#uploadPageComponent = new UploadPageComponent();
        this.#authPageComponent = new AuthPageComponent();
    }

    renderCore() {
        const div = document.createElement("div");
        div.appendChild(this.#menuComponent.render({ selectPageFunc: this.#selectPage }));

        if (this.state.currentPage)
            div.appendChild(document.createElement("br"));

        if (this.state.currentPage === "download")
            div.appendChild(this.#downloadPageComponent.render());
        if (this.state.currentPage === "upload")
            div.appendChild(this.#uploadPageComponent.render());
        if (this.state.currentPage === "auth")
            div.appendChild(this.#authPageComponent.render());

        return div;
    }

    #selectPage = (page) => {
        if (page === "download")
            this.#downloadPageComponent.loadFilesList();
        if (page === "auth")
            this.#authPageComponent.setAuthState();
        this.setState({ currentPage: page });
    }
}
