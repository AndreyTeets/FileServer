class MenuComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const div = document.createElement("div");
        div.appendChild(new HeaderComponent().render({ text: "Menu:" }));
        div.appendChild(new ButtonComponent().render({ text: "DownloadPage", onclickFunc: () => this.props.selectPageFunc("download") }));
        div.appendChild(new ButtonComponent().render({ text: "UploadPage", onclickFunc: () => this.props.selectPageFunc("upload") }));
        div.appendChild(new ButtonComponent().render({ text: "AuthPage", onclickFunc: () => this.props.selectPageFunc("auth") }));
        return div;
    }
}
