using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public static class EasyAccess
    {
        public static SpecialNodeHandler NodeHandler => SpecialNodeHandler.Instance;
        public static CardSingleChoicesSequencer CardSingleChoices => NodeHandler.cardChoiceSequencer;
        public static CardMergeSequencer CardMerge => NodeHandler.cardMerger;
        public static DuplicateMergeSequencer DuplicateMerge => NodeHandler.duplicateMerger;
        public static CardRemoveSequencer CardRemove => NodeHandler.cardRemoveSequencer;
        public static CardStatBoostSequencer CardStatBoost => NodeHandler.cardStatBoostSequencer;
        public static GainConsumablesSequencer GainConsumables => NodeHandler.gainConsumablesSequencer;
        public static BuildTotemSequencer BuildTotem => NodeHandler.buildTotemSequencer;
        public static BuyPeltsSequencer BuyPelts => NodeHandler.buyPeltsSequencer;
        public static TradePeltsSequencer TradePelts => NodeHandler.tradePeltsSequencer;
        public static DeckTrialSequencer DeckTrial => NodeHandler.deckTrialSequencer;
        public static BoulderChoiceSequencer BoulderChoice => NodeHandler.boulderChoiceSequencer;
        public static ChooseEyeballSequencer ChooseEyeball => NodeHandler.chooseEyeballSequencer;
        public static RareCardChoicesSequencer RareCardChoices => NodeHandler.rareCardChoiceSequencer;
        public static VictoryFeastSequencer VictoryFeast => NodeHandler.victoryFeastSequencer;
        public static CopyCardSequencer CopyCard => NodeHandler.copyCardSequencer;
    }
}
