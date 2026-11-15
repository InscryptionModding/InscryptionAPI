
exports.preTransform = function (model) {
    setMemberFlags(model);
    return model;
};

function setMemberFlags(item) {
    if (!item) return;

    if (item.type) {
        var t = String(item.type).toLowerCase();

        // Container / Type declarations
        item.isNamespace = (t === 'namespace');
        item.isClass = (t === 'class');
        item.isStruct = (t === 'struct');
        item.isInterface = (t === 'interface');
        item.isEnum = (t === 'enum');
        item.isDelegate = (t === 'delegate');

        // Member declarations
        item.isConstructor = (t === 'constructor');
        item.isMethod = (t === 'method');
        item.isProperty = (t === 'property');
        item.isField = (t === 'field');
        item.isEvent = (t === 'event');
        item.isOperator = (t === 'operator');

        // Useful grouped helpers
        item.isType = (
            item.isClass ||
            item.isStruct ||
            item.isInterface ||
            item.isEnum ||
            item.isDelegate
        );

        item.isMember = (
            item.isConstructor ||
            item.isMethod ||
            item.isProperty ||
            item.isField ||
            item.isEvent ||
            item.isOperator
        );
    }

    // Recursively process nested models
    if (Array.isArray(item.children)) {
        item.children.forEach(setMemberFlags);
    }

    if (Array.isArray(item.items)) {
        item.items.forEach(setMemberFlags);
    }
}
