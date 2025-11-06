class MenuComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(new HeaderComponent().render({ text: "Menu:" }));
        div.append(new ButtonComponent().render({ text: "DownloadPage", onclickFunc: this.#onDownloadPageClick }));
        div.append(new ButtonComponent().render({ text: "UploadPage", onclickFunc: this.#onUploadPageClick }));
        div.append(new ButtonComponent().render({ text: "AuthPage", onclickFunc: this.#onAuthPageClick }));
        return div;
    }

    #onDownloadPageClick = () => this.props.selectPageFunc("download");
    #onUploadPageClick = () => this.props.selectPageFunc("upload");
    #onAuthPageClick = () => this.props.selectPageFunc("auth");
}
