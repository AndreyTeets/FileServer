class App {
    static #menu;
    static get menu() {
        return App.#menu;
    }
    static set menu(value) {
        if (App.#menu !== value) {
            App.#menu = value;
            App.#render();
        }
    }

    static #page;
    static get page() {
        return App.#page;
    }
    static set page(value) {
        if (App.#page !== value) {
            App.#page = value;
            App.#render();
        }
    }

    static proxifyObjectPropertiesForRenderOnChange(value, renderCondition) {
        return App.#proxyfyIfObject(value, renderCondition);
    }

    static #render() {
        const div = document.createElement("div");
        div.appendChild(App.#menu.create());
        if (App.#page) {
            div.appendChild(document.createElement("br"));
            div.appendChild(App.#page.create());
        }

        const appDiv = document.querySelector("#app");
        appDiv.replaceChild(div, appDiv.firstChild);
    }

    static #proxyfyIfObject(value, renderCondition) {
        if (App.#isObject(value)) {
            App.#proxifyAllPropsObjectValues(value, renderCondition);
            return new Proxy(value, {
                get(obj, prop) {
                    return obj[prop];
                },
                set(obj, prop, val) {
                    obj[prop] = App.#proxyfyIfObject(val, renderCondition);
                    if (renderCondition())
                        App.#render();
                    return true;
                }
            });
        } else {
            return value;
        }
    }

    static #proxifyAllPropsObjectValues(value, renderCondition) {
        for (const prop of Object.getOwnPropertyNames(value)) {
            if (App.#isObject(value[prop]))
                value[prop] = App.#proxyfyIfObject(value[prop], renderCondition);
        }
    }

    static #isObject(value) {
        return value
            && typeof value === 'object'
            && !Array.isArray(value);
    }
}
