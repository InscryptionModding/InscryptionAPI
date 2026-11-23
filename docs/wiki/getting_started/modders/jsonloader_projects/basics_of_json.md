# Basics of JSON

So, knowledge of `JSON` is a core requirement of using JSONLoader. Thus, let's get you familiarized with what `JSON` is. First off `JSON` stands for `JavaScript Object Notation`, if the name didn't imply it already, it is a thing that originated from JavaScript. Not to be confused with Java, they are vastly different beasts. Anyways `JSON` is a Scripting Language, meant for expressing objects, such as what JSONLoader uses for its Cards. But before we can dig into that, first we need to introduce you to `JSON Syntax`.

___

## Your First JSON

To get started, let's create a new `JSON File`. Now open it in your text editor of preference. Next up, enter the following `{}`. Why? Well without them, the `JSON` won't actually read as a `JSON`, the curly braces (`{}`) act as the piece that tells the reader that the contents within the `{}` are a `Object`. To be precise the opening `{` opens the `Object`, and the closing `}` thus closes the object. 

What goes within these `{}` are called `Properties` and they define what the `Object` holds, the `Properties` themselves you can think of as defining what a piece of the `Object` is meant to do. A `Property` consists of a `Key` and a `Value`. The `Key` will always have the same syntax. The `Value` will change syntax based on the `Type` in which the `Property` pertains of. For example, a `String` and a `Boolean` will have different ways of defining its `Value`'s syntax, and by extension, what the `Value` of the `Property Key` can be.

You can open a `Key` the same way you would an `Object`. Except with a `Key` you would use a pair of quotation marks (`""`). What goes within the quotation marks would be the `Property Key` in plaintext. When working with `JSON`, the `Schema` associated with the `JSON` will define what goes in the `Key`. A `Schema` is basically a set of rules that extend the rules of how a given `Property` can be defined, and can also be used to see what kind of `Value` is expected to go alongside a given `Property Key`. *Note: `Propety Key`'s are case-sensitive, when defining one, ensure the casing matches the original `Key` found with the `Schema`.*

Now there's a plethora of things you can format into this newly created `Object`. Open up your braces to look like the following code block, make sure there's a tab's worth of spacing in the middle. You won't be penalized if you don't but it will look better for the person who will end up reading the `JSON`. Within these braces is where we will be putting the `Properties`, as is expected by `JSON` syntax.

```json
{

}
```

The following subsections will go over each `Property Type` that we can use when making a `JSON`. It will go over both what the rules of the `Property Type` are, a what the `Property Type` could be used for. 

### String

### Integer

### Boolean

### Array

### Object

### Null
___