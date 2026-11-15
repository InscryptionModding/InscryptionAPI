
exports.postTransform = function (model) {
    console.log("ManagedReference.extension.js EXECUTED");
    console.log("Root model: " + JSON.stringify(model));

    setMemberFlags(model);

    return model;
};

function setMemberFlags(item, inheritedType) {
    if (!item || typeof item !== 'object') return;

    // Determine the current item's declaration type.
    var t = item.type
        ? String(item.type).toLowerCase()
        : inheritedType;

    // DocFX member-group containers identify their children
    // through properties such as inMethod, inProperty, etc.
    if (item.typePropertyName) {
        var groupType = String(item.typePropertyName)
            .replace(/^in/, '')
            .toLowerCase();

        if (groupType) {
            t = groupType;
        }
    }

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

    // Recursively process nested models.
    // Pass the current group type down to its children.
    if (Array.isArray(item.children)) {
        item.children.forEach(function (child) {
            setMemberFlags(child, t);
        });
    }

    if (Array.isArray(item.items)) {
        item.items.forEach(function (child) {
            setMemberFlags(child, t);
        });
    }
}
