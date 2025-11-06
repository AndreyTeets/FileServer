class Comparer {
    static objectsAreEqual(obj1, obj2) {
        if (!(typeof obj1 === typeof obj2 && typeof obj1 === "object"))
            throw new Error(`Non-object type '${typeof obj1}'/'${typeof obj2}' while comparing objects.`);

        if (obj1 === obj2) // Reference equal or both null
            return true;
        if (obj1 === null || obj2 === null)
            return false; // One is null and the other isn't

        const keys1 = Object.keys(obj1);
        const keys2 = Object.keys(obj2);
        if (keys1.length !== keys2.length)
            return false;

        for (const key of keys1) {
            if (!keys2.includes(key))
                return false;

            const val1 = obj1[key];
            const val2 = obj2[key];

            const typesAreEqual = typeof val1 === typeof val2;
            if (!typesAreEqual)
                return false;

            const bothAreObjects = typesAreEqual && typeof val1 === "object";
            if (bothAreObjects && !Comparer.objectsAreEqual(val1, val2))
                return false;
            if (!bothAreObjects && val1 !== val2)
                return false;
        }

        return true;
    }
}
