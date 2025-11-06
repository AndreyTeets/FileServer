class MenuComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(new HeaderComponent().render({ text: "Menu:" }));
        div.append(new ButtonComponent().render({ text: "DownloadPage", onclickFunc: () => this.props.selectPageFunc("download") }));
        div.append(new ButtonComponent().render({ text: "UploadPage", onclickFunc: () => this.props.selectPageFunc("upload") }));
        div.append(new ButtonComponent().render({ text: "AuthPage", onclickFunc: () => this.props.selectPageFunc("auth") }));
        return div;
    }
}
