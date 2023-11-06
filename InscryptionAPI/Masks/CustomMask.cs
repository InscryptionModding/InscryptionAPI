using DiskCardGame;
using InscryptionAPI.Helpers;

namespace InscryptionAPI.Masks;

public class CustomMask
{
    public readonly LeshyAnimationController.Mask ID;
    public readonly string Name;
    public readonly string GUID;
    public readonly bool Override;
    public List<MaterialOverride> MaterialOverrides { get; private set; }
    public MaskManager.ModelType ModelType { get; private set; }
    public Type BehaviourType { get; private set; } = typeof(MaskBehaviour);

    public CustomMask(string GUID, string Name, LeshyAnimationController.Mask ID, bool isOverride)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.ID = ID;
        this.Override = isOverride;
    }

    public CustomMask AddMaterialOverride(MaterialOverride materialOverride)
    {
        if (MaterialOverrides == null)
        {
            MaterialOverrides = new List<MaterialOverride>();
        }
        MaterialOverrides.Add(materialOverride);
        return this;
    }

    public CustomMask SetModelType(MaskManager.ModelType modelType)
    {
        ModelType = modelType;
        return this;
    }

    public CustomMask SetMaskBehaviour(Type type)
    {
        if (!type.IsSubclassOf(typeof(MaskBehaviour)))
        {
            InscryptionAPIPlugin.Logger.LogError("Could not add type " + type + " to mask " + ToString() + ". It is not of type MaskBehaviour!");
            return this;
        }
        BehaviourType = type;
        return this;
    }

    public override string ToString()
    {
        return GUID + "_" + Name;
    }
}