using System;
using DiskCardGame;

namespace InscryptionAPI.Encounters;

public class CustomNodeData : SpecialNodeData
{
    public CustomNodeData(string guid)
    {
        this.guid = guid;
    }

    public virtual void Initialize() { }

    public string guid { get; private set; }

    public sealed override string PrefabPath
    {
        get
        {
            // This syntax works with our custom resource bank loaders
            return $"Prefabs/Map/MapNodesPart1/MapNode_BuyPelts@{guid}";
        }
    }
}