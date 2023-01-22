using DiskCardGame;
using InscryptionAPI.Helpers;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Nodes;

public class CustomSpecialNodeData : SpecialNodeData
{
    internal static Texture2D mapeventmissing;

    public CustomSpecialNodeData(NewNodeManager.FullNode node)
    {
        name = node.name;
        guid = node.guid;
    }

    public override string PrefabPath
    {
        get
        {
            return "Prefabs/Map/MapNodesPart1/MapNode_TradePelts";
        }
    }

    /// <summary>
    /// FullNode this NodeData originated from.
    /// </summary>
    public NewNodeManager.FullNode Node => NewNodeManager.addedNodes.Find(x => x != null && x.guid == guid && x.name == name);

    internal void OnPreGeneration() => Node?.onPreNodeGeneration?.Invoke(this);

    internal void OnPostGeneration(MapNode2D node)
    {
        if (Node != null && Node.nodeAnimation != null && Node.nodeAnimation.Count > 0)
        {
            AnimatingSprite sprite = node.GetComponentInChildren<AnimatingSprite>();
            if (sprite != null)
            {
                sprite.textureFrames = new(Node.nodeAnimation);
                sprite.SetTexture(sprite.textureFrames[0]);
            }
        }
        else if (Node == null)
        {
            try
            {
                AnimatingSprite sprite = node.GetComponentInChildren<AnimatingSprite>();
                if (sprite != null)
                {
                    mapeventmissing ??= TextureHelper.GetImageAsTexture("mapevent_missing.png", Assembly.GetExecutingAssembly());
                    sprite.textureFrames = new() { mapeventmissing };
                    sprite.SetTexture(mapeventmissing);
                }
            }
            catch { }
        }
        Node?.onPostNodeGeneration?.Invoke(this, node);
    }

    /// <summary>
    /// Saves value in a dictionary of runtime data using key.
    /// </summary>
    /// <param name="key">The key to the value.</param>
    /// <param name="value">The value to save.</param>
    public void Set(string key, object value)
    {
        if (runtimeData.ContainsKey(key))
        {
            runtimeData[key] = value;
        }
        else
        {
            runtimeData.Add(key, value);
        }
    }

    /// <summary>
    /// Gets value in runtime data using key.
    /// </summary>
    /// <param name="key">The key to the value.</param>
    /// <returns>If runtime data contains valuem, key. If it does not, returns the default.</returns>
    public object Get(string key)
    {
        if (runtimeData.TryGetValue(key, out var val))
        {
            return val;
        }
        return default;
    }

    /// <summary>
    /// Gets value in runtime data using key.
    /// </summary>
    /// <param name="key">The key to the value.</param>
    /// <returns>If runtime data contains value, key. If it does not, returns the default.</returns>
    public T Get<T>(string key)
    {
        if (runtimeData.TryGetValue(key, out var val))
        {
            if (val is T t)
            {
                return t;
            }
        }
        return default;
    }

    public override List<SelectionCondition> GenerationPrerequisiteConditions => Node?.generationPrerequisites ?? new();

    public override List<SelectionCondition> ForceGenerationConditions => Node?.forceGenerationConditions ?? new();

    private readonly Dictionary<string, object> runtimeData = new();
    public readonly string name;
    public readonly string guid;
}