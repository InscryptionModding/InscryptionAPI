using DiskCardGame;
using UnityEngine;

namespace InscryptionAPI.Masks;

public class CustomMask
{
    public LeshyAnimationController.Mask ID;
    public string Name;
    public string GUID;
    public Texture2D TextureOverride;
    public MaskManager.ModelType ModelType;
    public Type BehaviourType;
    public bool Override = true;
}