using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace InscryptionAPI.Nodes
{
    public static class NodeTools
    {
        public static Type GetTypeForNodeType(MapNodeType type)
        {
            switch (type)
            {
                case MapNodeType.SpecialCardBattle:
                case MapNodeType.CardBattle:
                    return typeof(CardBattleNodeData);
                case MapNodeType.CardChoice:
                case MapNodeType.SpecialEvent:
                case MapNodeType.Other:
                    return typeof(SpecialNodeData);
            }
            return typeof(NodeData);
        }

        public static void StartSequence(this INodeSequencer self, SpecialNodeHandler instance = null)
        {
            self.Inherit();
            (instance ?? SpecialNodeHandler.Instance).StartCoroutine(self.Sequence());
        }

        public static IEnumerator Sequence(this INodeSequencer self)
        {
            yield return self.DoCustomSequence();
            Singleton<ViewManager>.Instance.SwitchToView(View.MapDefault, false, false);
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
            if(self is not DoNotReturnToMapOnEnd)
            {
                Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.Map, null);
            }
            if(self is Component && self is DestroyOnEnd)
            {
                UnityEngine.Object.Destroy((self as Component).gameObject);
            }
            yield break;
        }

        public static bool SequencerTypeValid(Type type)
        {
            return type != null && type.IsSubclassOf(typeof(Component)) && type.GetInterfaces() != null && type.GetInterfaces().Contains(typeof(INodeSequencer));
        }
    }
}
