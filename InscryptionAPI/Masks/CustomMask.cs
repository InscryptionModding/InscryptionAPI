using DiskCardGame;
using InscryptionAPI.Helpers;

namespace InscryptionAPI.Masks;

/// <summary>
/// Class for adding custom boss masks for Leshy to use in-game.
/// </summary>
public class CustomMask {
    /// <summary>
    /// The ID of this mask.
    /// </summary>
    public readonly LeshyAnimationController.Mask ID;
    /// <summary>
    /// The internal name of this mask.
    /// </summary>
    public readonly string Name;
    /// <summary>
    /// The GUID of the mod adding this mask.
    /// </summary>
    public readonly string GUID;
    /// <summary>
    /// If this mask should override another mask using the same ID as this mask.
    /// </summary>
    public readonly bool Override;
    /// <summary>
    /// The material overrides that will affect this mask's model materials.
    /// </summary>
    public List<MaterialOverride> MaterialOverrides { get; private set; }
    /// <summary>
    /// The ID of the model type this mask uses.
    /// </summary>
    public MaskManager.ModelType ModelType { get; private set; }
    /// <summary>
    /// A subclass of <see cref="MaskBehaviour"/> that implements the logic for this mask.
    /// </summary>
    public Type BehaviourType { get; private set; } = typeof(MaskBehaviour);

    /// <summary>
    /// Creates a new instance of <see cref="CustomMask"/>.
    /// </summary>
    /// <param name="GUID">The GUID of the mod adding this mask.</param>
    /// <param name="Name">The internal name of this mask.</param>
    /// <param name="ID">The ID of this mask.</param>
    /// <param name="isOverride">If this mask is meant to override masks using the same ID.</param>
    public CustomMask(string GUID, string Name, LeshyAnimationController.Mask ID, bool isOverride)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.ID = ID;
        this.Override = isOverride;
    }

    /// <summary>
    /// Adds a new <see cref="MaterialOverride"/> to this mask to directly modify the material textures used for this mask.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> to add.</param>
    /// <returns>The same <see cref="CustomMask"/> so a chain can continue.</returns>
    public CustomMask AddMaterialOverride(MaterialOverride materialOverride)
    {
        if (MaterialOverrides == null)
        {
            MaterialOverrides = new List<MaterialOverride>();
        }
        MaterialOverrides.Add(materialOverride);
        return this;
    }

    /// <summary>
    /// Sets the model type ID for this mask. Use if you want to use one of the vanilla or API-added mask models.
    /// </summary>
    /// <param name="modelType">The ID of the mask model to use.</param>
    /// <returns>The same <see cref="CustomMask"/> so a chain can continue.</returns>
    public CustomMask SetModelType(MaskManager.ModelType modelType)
    {
        ModelType = modelType;
        return this;
    }

    /// <summary>
    /// Sets the mask's <see cref="MaskBehaviour"/>.
    /// </summary>
    /// <param name="type">A subclass of <see cref="MaskBehaviour"/> that adds the mask's logic.</param>
    /// <returns>The same <see cref="CustomMask"/> so a chain can continue.</returns>
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

    /// <summary>
    /// Returns the internal name of this mask.
    /// </summary>
    /// <returns>The mask's internal name in format <c>{GUID}_{Name}</c>.</returns>
    public override string ToString()
    {
        return GUID + "_" + Name;
    }
}