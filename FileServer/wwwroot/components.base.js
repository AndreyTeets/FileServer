class ComponentBase {
    props;
    state;
    #lastRenderedElem;
    #lastRenderedProps;
    #lastRenderedState;

    constructor() {
        this.props = {};
        this.state = {};
    }

    render(props) {
        this.props = props || {};

        if (!this.#lastRenderedPropsOrStateDifferFromCurrent())
            return this.#lastRenderedElem;
        return this.#renderCoreAndSaveLastRenderInfo();
    }

    setState(state) {
        this.state = { ...this.state, ...state };
        if (this.#lastRenderedElem && this.#lastRenderedElem.isConnected && this.#lastRenderedPropsOrStateDifferFromCurrent())
            this.#rerender();
    }

    #rerender() {
        const oldElem = this.#lastRenderedElem;
        const newElem = this.#renderCoreAndSaveLastRenderInfo();
        oldElem.parentNode.replaceChild(newElem, oldElem);
    }

    #renderCoreAndSaveLastRenderInfo() {
        const elem = this.renderCore();
        this.#lastRenderedElem = elem;
        this.#setCurrentPropsAndStateAsLastRendered();
        return elem;
    }

    #lastRenderedPropsOrStateDifferFromCurrent() {
        if (!(this.#lastRenderedProps && Comparer.objectsAreEqual(this.#lastRenderedProps, this.props)))
            return true;
        if (!(this.#lastRenderedState && Comparer.objectsAreEqual(this.#lastRenderedState, this.state)))
            return true;
        return false;
    }

    #setCurrentPropsAndStateAsLastRendered() {
        this.#lastRenderedProps = this.props;
        this.#lastRenderedState = this.state;
    }
}
