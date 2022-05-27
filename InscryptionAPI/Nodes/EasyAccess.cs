using DiskCardGame;

namespace InscryptionAPI.Nodes;

/// <summary>
/// Provides easy access to act 1 node sequencers.
/// </summary>
public static class EasyAccess
{
    /// <summary>
    /// Returns the instance of SpecialNodeHandler.
    /// </summary>
    public static SpecialNodeHandler NodeHandler => SpecialNodeHandler.Instance;
    /// <summary>
    /// Returns the sequencer for normal card choices.
    /// </summary>
    public static CardSingleChoicesSequencer CardSingleChoices => NodeHandler.cardChoiceSequencer;
    /// <summary>
    /// Returns the sequencer for the sacrifice stone card merge node.
    /// </summary>
    public static CardMergeSequencer CardMerge => NodeHandler.cardMerger;
    /// <summary>
    /// Returns the sequencer for the Mycologists duplicate card merge node.
    /// </summary>
    public static DuplicateMergeSequencer DuplicateMerge => NodeHandler.duplicateMerger;
    /// <summary>
    /// Returns the sequencer for the Bone Lord card remove node.
    /// </summary>
    public static CardRemoveSequencer CardRemove => NodeHandler.cardRemoveSequencer;
    /// <summary>
    /// Returns the sequencer for the campfire card stat boost node.
    /// </summary>
    public static CardStatBoostSequencer CardStatBoost => NodeHandler.cardStatBoostSequencer;
    /// <summary>
    /// Returns the sequencer for the backpack gain items node.
    /// </summary>
    public static GainConsumablesSequencer GainConsumables => NodeHandler.gainConsumablesSequencer;
    /// <summary>
    /// Returns the sequencer for the Woodcarver build totem node.
    /// </summary>
    public static BuildTotemSequencer BuildTotem => NodeHandler.buildTotemSequencer;
    /// <summary>
    /// Returns the sequencer for the Trapper buy pelts node.
    /// </summary>
    public static BuyPeltsSequencer BuyPelts => NodeHandler.buyPeltsSequencer;
    /// <summary>
    /// Returns the sequencer for the Trader trade pelts node.
    /// </summary>
    public static TradePeltsSequencer TradePelts => NodeHandler.tradePeltsSequencer;
    /// <summary>
    /// Returns the sequencer for the cave deck trial node.
    /// </summary>
    public static DeckTrialSequencer DeckTrial => NodeHandler.deckTrialSequencer;
    /// <summary>
    /// Returns the sequencer for the Prospector boulder strike node.
    /// </summary>
    public static BoulderChoiceSequencer BoulderChoice => NodeHandler.boulderChoiceSequencer;
    /// <summary>
    /// Returns the sequencer for the Special Dagger choose eyeball special event.
    /// </summary>
    public static ChooseEyeballSequencer ChooseEyeball => NodeHandler.chooseEyeballSequencer;
    /// <summary>
    /// Returns the sequencer for the post-boss rare card choice special event.
    /// </summary>
    public static RareCardChoicesSequencer RareCardChoices => NodeHandler.rareCardChoiceSequencer;
    /// <summary>
    /// Returns the sequencer for post-Leshy victory feast special event.
    /// </summary>
    public static VictoryFeastSequencer VictoryFeast => NodeHandler.victoryFeastSequencer;
    /// <summary>
    /// Returns the sequencer for the Kaycee's Mod-specific Goobert copy card node.
    /// </summary>
    public static CopyCardSequencer CopyCard => NodeHandler.copyCardSequencer;
}