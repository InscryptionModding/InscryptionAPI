using DiskCardGame;
using UnityEngine;

namespace InscryptionAPI.Masks;

public class CustomMask
{
    public LeshyAnimationController.Mask ID;
    public string Name;
    public string GUID;
    public List<Texture2D> TextureOverrides;
    public MaskManager.ModelType ModelType;
    public Type BehaviourType;
    public bool Override = true;

    public override string ToString()
    {
        return GUID + "_" + Name;
    }
}