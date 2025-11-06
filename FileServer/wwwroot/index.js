function onDocumentLoad() {
    const appComponent = new AppComponent();
    const div = appComponent.render();
    const root = document.querySelector("#root");
    root.replaceChild(div, root.firstChild);
}
