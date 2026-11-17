exports.postTransform = function (model) {
    setMemberFlags(model);
    return model;
};

function setMemberFlags(item, inheritedType) {
    if (!item || typeof item !== 'object') return;

    var t = item.type
        ? String(item.type).toLowerCase()
        : inheritedType;

    // Member-group containers carry their kind in typePropertyName (inMethod, ...)
    if (item.typePropertyName) {
        var groupType = String(item.typePropertyName)
            .replace(/^in/, '')
            .toLowerCase();

        if (groupType) {
            t = groupType;
        }
    }

    // ---- Declaration kinds -------------------------------------------------
    // postTransform runs AFTER ManagedReference.common.js's transform, which
    // already set isClass = true for class/struct/interface/delegate because
    // ManagedReference.html.primary.tmpl dispatches on isClass. Setting
    // isClass = (t === 'class') here would clobber that and blank every
    // interface, struct and delegate page. Mirror common.js instead.
    item.isNamespace = (t === 'namespace');
    item.isClass = (
        t === 'class' ||
        t === 'struct' ||
        t === 'interface' ||
        t === 'delegate'
    );
    item.isStruct = (t === 'struct');
    item.isInterface = (t === 'interface');
    item.isEnum = (t === 'enum');
    item.isDelegate = (t === 'delegate');

    // ---- Member kinds ------------------------------------------------------
    item.isConstructor = (t === 'constructor');
    item.isMethod = (t === 'method');
    item.isProperty = (t === 'property');
    item.isField = (t === 'field');
    item.isEvent = (t === 'event');
    item.isOperator = (t === 'operator');

    // ---- in* flags, as consumed by the partials ({{#inClass}}, {{#inEnum}}) --
    // common.js sets these only on the root and on group containers. Emit them
    // here too so the partials never depend on transform order.
    item.inNamespace = (t === 'namespace');
    item.inClass = (t === 'class');
    item.inStruct = (t === 'struct');
    item.inInterface = (t === 'interface');
    item.inEnum = (t === 'enum');
    item.inDelegate = (t === 'delegate');

    item.inConstructor = (t === 'constructor');
    item.inMethod = (t === 'method');
    item.inProperty = (t === 'property');
    item.inField = (t === 'field');
    item.inEvent = (t === 'event');
    item.inOperator = (t === 'operator');

    // ---- Aggregates --------------------------------------------------------
    item.isType = (
        item.isClass ||
        item.isEnum ||
        item.isNamespace
    );

    item.isMember = (
        item.isConstructor ||
        item.isMethod ||
        item.isProperty ||
        item.isField ||
        item.isEvent ||
        item.isOperator
    );

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