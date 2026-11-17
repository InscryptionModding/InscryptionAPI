exports.postTransform = function (model) {
    var uidLookup = {};
    buildUidLookup(model, uidLookup);

    setMemberFlags(model);
    setExtensionMethodSummaries(model, uidLookup);

    return model;
};

function buildUidLookup(item, lookup) {
    if (!item || typeof item !== 'object') return;

    if (item.uid) {
        lookup[String(item.uid)] = item;
    }

    if (Array.isArray(item.children)) {
        item.children.forEach(function (child) {
            buildUidLookup(child, lookup);
        });
    }

    if (Array.isArray(item.items)) {
        item.items.forEach(function (child) {
            buildUidLookup(child, lookup);
        });
    }
}

function setExtensionMethodSummaries(item, uidLookup) {
    if (!item || typeof item !== 'object') return;

    if (Array.isArray(item.extensionMethods)) {
        item.extensionMethods.forEach(function (extensionMethod) {
            if (!extensionMethod || typeof extensionMethod !== 'object') return;

            var definitionUid = extensionMethod.definition || extensionMethod.uid;

            if (!definitionUid) return;

            var definition = uidLookup[String(definitionUid)];

            if (!definition) return;

            if (definition.summary) {
                extensionMethod.summary = definition.summary;
            }
        });
    }

    if (Array.isArray(item.children)) {
        item.children.forEach(function (child) {
            setExtensionMethodSummaries(child, uidLookup);
        });
    }

    if (Array.isArray(item.items)) {
        item.items.forEach(function (child) {
            setExtensionMethodSummaries(child, uidLookup);
        });
    }
}

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
        item.isEnum
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