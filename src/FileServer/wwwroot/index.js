function onDocumentLoad() {
    const root = document.querySelector("#root");
    const vRoot = VDom.createElement("div");
    vRoot.id = "root";

    const appComponent = new AppComponent();
    const vApp = appComponent.render();

    vRoot.append(vApp);
    vRoot.setDomElem(vRoot.createDomElem());
    root.replaceWith(vRoot.getDomElem());
    VDom.rootElem = vRoot;
}
