using DiskCardGame;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Nodes
{
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

        public NewNodeManager.FullNode Node => NewNodeManager.NewNodes.Find(x => x != null && x.guid == guid && x.name == name);

        public void OnPreGeneration() => Node?.onPreNodeGeneration?.Invoke(this);

        public void OnPostGeneration(MapNode2D node)
        {
            if(Node != null && Node.nodeAnimation != null && Node.nodeAnimation.Count > 0)
            {
                AnimatingSprite sprite = node.GetComponentInChildren<AnimatingSprite>();
                if (sprite != null)
                {
                    sprite.textureFrames = new(Node.nodeAnimation);
                    sprite.SetTexture(sprite.textureFrames[0]);
                }
            }
            else if(Node == null)
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

        public override List<SelectionCondition> GenerationPrerequisiteConditions => Node?.generationPrerequisites ?? new();

        public override List<SelectionCondition> ForceGenerationConditions => Node?.forceGenerationConditions ?? new();

        public readonly Dictionary<string, object> runtimeData = new();
        public readonly string name;
        public readonly string guid;
    }
}
