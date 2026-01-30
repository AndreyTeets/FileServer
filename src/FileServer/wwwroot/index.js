function onDocumentLoad() {
    const root = document.querySelector("#root");
    const vRoot = VDom.createElement("div");
    vRoot.id = "root"; // This isn't required for the app to work, it's just to keep the id prop on the new root element

    const appComponent = new AppComponent();
    const vApp = appComponent.render();

    vRoot.append(vApp);
    vRoot.setDomElem(vRoot.createDomElem());
    root.replaceWith(vRoot.getDomElem());
    VDom.rootElem = vRoot;
}
