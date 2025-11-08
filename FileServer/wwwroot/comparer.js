class Comparer {
    static objectsAreIdentical(obj1, obj2, excludeProps) {
        if (!(typeof obj1 === typeof obj2 && typeof obj1 === "object"))
            throw new Error(`Non-object type '${typeof obj1}'/'${typeof obj2}' while comparing objects.`);

        if (obj1 === null && obj2 === null)
            return true;
        if (obj1 === null || obj2 === null)
            return false;

        const keys1 = Object.keys(obj1).filter(x => !excludeProps?.includes(x));
        const keys2 = Object.keys(obj2).filter(x => !excludeProps?.includes(x));
        if (keys1.length !== keys2.length)
            return false;

        for (const key of keys1) {
            if (!keys2.includes(key))
                return false;
            if (!Comparer.#valuesAreIdentical(obj1[key], obj2[key], excludeProps))
                return false;
        }

        return true;
    }

    static #valuesAreIdentical(val1, val2, excludeProps) {
        const typesAreEqual = typeof val1 === typeof val2;
        if (!typesAreEqual)
            return false;

        const bothAreObjects = typesAreEqual && typeof val1 === "object";
        if (bothAreObjects && !Comparer.objectsAreIdentical(val1, val2, excludeProps))
            return false;
        if (!bothAreObjects && val1 !== val2)
            return false;

        return true;
    }
}
