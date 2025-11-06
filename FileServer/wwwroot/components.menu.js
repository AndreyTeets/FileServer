class MenuComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(new HeaderComponent().render({ text: "Menu:" }));
        div.append(new ButtonComponent().render({ text: "DownloadPage", onclick: this.#onDownloadPageClick }));
        div.append(new ButtonComponent().render({ text: "UploadPage", onclick: this.#onUploadPageClick }));
        div.append(new ButtonComponent().render({ text: "AuthPage", onclick: this.#onAuthPageClick }));
        return div;
    }

    #onDownloadPageClick = () => this.props.selectPageFunc("download");
    #onUploadPageClick = () => this.props.selectPageFunc("upload");
    #onAuthPageClick = () => this.props.selectPageFunc("auth");
}
