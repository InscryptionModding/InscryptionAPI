using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using static InscryptionAPI.Card.AbilityManager;

namespace APIPlugin;

[Obsolete("Use AbilityManager instead", true)]
public class NewAbility
{
    public Ability ability;
    public AbilityInfo info;
    public Type abilityBehaviour;
    public Texture tex;
    public AbilityIdentifier id;

    public NewAbility(AbilityInfo info, Type abilityBehaviour, Texture tex, AbilityIdentifier id = null)
    {
        string guid = id == null ? abilityBehaviour.Namespace : id.guid;
        FullAbility fab = AbilityManager.Add(guid, info, abilityBehaviour, tex);
        this.ability = fab.Id;
        this.info = fab.Info;
        this.abilityBehaviour = fab.AbilityBehavior;
        this.tex = fab.Texture;
        this.id = AbilityIdentifier.GetID(guid, fab.Info.rulebookName);
    }
}