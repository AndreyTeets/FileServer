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
        const div = VDom.createElement("div");
        div.append(this.#menuComponent.render({ selectPageFunc: this.#selectPage }));

        if (this.state.currentPage)
            div.append(VDom.createElement("br"));

        if (this.state.currentPage === "download")
            div.append(this.#downloadPageComponent.render());
        if (this.state.currentPage === "upload")
            div.append(this.#uploadPageComponent.render());
        if (this.state.currentPage === "auth")
            div.append(this.#authPageComponent.render());

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
