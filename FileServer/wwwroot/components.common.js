class TextComponent {
    text;
    cssClass;

    constructor(text) {
        this.text = text;
        this.cssClass = "text";
    }

    create() {
        const p = document.createElement("p");
        p.append(this.text);
        p.classList.add(this.cssClass);
        return p;
    }
}

class ErrorComponent extends TextComponent {
    cssClass = "error";
}

class HeaderComponent extends TextComponent {
    cssClass = "header";
}

class ButtonComponent {
    text;
    onclickFunc;

    constructor(text, onclickFunc) {
        this.text = text;
        this.onclickFunc = onclickFunc;
    }

    create() {
        const input = document.createElement("input");
        input.type = "submit";
        input.value = this.text;
        input.onclick = this.onclickFunc;
        return input;
    }
}
