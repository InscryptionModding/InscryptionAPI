# Getting Started (Modders)

So your interested in learning how to modify Inscryption? Well this section of our guide should come in really handy for you! Albeit if your a artist, a developer, or just passing by, we hope you find these guides of value.

First off let's get you to the right branch of the guides. Let's get some groundworks out of the way now. Based on the item below, you should be funneled to the level your after. *Note for the IDE Question, if you have familiarity with [IntelliJ Idea](https://www.jetbrains.com/idea/), you'll likely prefer Jetbrains Rider.*

```mermaid
flowchart TD
    A["Are you trying to make a Simple Cardpack?"]
    A --> A1["If this item applied to you, than let's head to the<br/>~~ JSONLoader Branch ~~<br/>Unless your willing to challenge yourself?<br/>If you want the challenge, follow the other tree!"]

    B["Are you after something more Complex in terms of your Project?"]
    B --> C["Are you already familiar with C#?"]

    C -->|Yes| D["What is your preferred of the two? Visual Studio, or JetBrains Rider?"]
    C -->|No| C1["If this was your case, than let's head into the<br/>~~ C# Basics Branch ~~</br>To get you familiar with the underlying concepts,<br/>and knowledge you will need in order to make your mod."]

    D -->|Visual Studio| D1["If this was your choice, let us navigate into the<br/>~~ API Project Setup Guide for Visual Studio ~~"]
    D -->|JetBrains Rider| D2["If this was your choice, let us navigate into the<br/>~~ API Project Setup Guide for JetBrains Rider ~~"]

    click A1 "jsonloader_projects/index.html" "Open JSONLoader Branch"
    click C1 "c_sharp_basics/index.html" "Open C# Basics Branch"
    click D1 "api_projects_vs_edition/index.html" "Open Visual Studio Setup Guide"
    click D2 "api_projects_rider_edition/index.html" "Open JetBrains Rider Setup Guide"
```

___

## Modding Resources
- [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm)
- [BepInEx documentation](https://docs.bepinex.dev/)
- [Harmony patching article](https://harmony.pardeike.net/articles/patching.html)
- [Example Mod using C#](https://github.com/debugman18/InscryptionExampleMod)
- [Vanilla and Modded Enumerations](https://github.com/SaxbyMod/SabyModEnums)

___