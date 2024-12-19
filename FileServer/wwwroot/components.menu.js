class MenuComponent {
    create() {
        const div = document.createElement("div");
        div.appendChild(new HeaderComponent("Menu:").create());
        div.appendChild(new ButtonComponent("DownloadPage", AppLogic.setDownloadPage).create());
        div.appendChild(new ButtonComponent("UploadPage", AppLogic.setUploadPage).create());
        div.appendChild(new ButtonComponent("AuthPage", AppLogic.setAuthPage).create());
        return div;
    }
}
