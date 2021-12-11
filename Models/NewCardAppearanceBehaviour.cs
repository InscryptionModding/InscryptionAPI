using System;
using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
    public class NewCardAppearanceBehaviour
    {
        public static Dictionary<CardAppearanceBehaviour.Appearance, NewCardAppearanceBehaviour> behaviours = new();
        public static List<NewCardAppearanceBehaviour> allBehaviours = new();

        public CardAppearanceBehaviour.Appearance Appearance;
        public string Name;
        public Type Behaviour;

        public static NewCardAppearanceBehaviour AddNewBackground(Type type, string name)
        {
            NewCardAppearanceBehaviour backgroundBehaviour = new NewCardAppearanceBehaviour();
            backgroundBehaviour.Appearance = (CardAppearanceBehaviour.Appearance)((int)(CardAppearanceBehaviour.Appearance.SexyGoat) + allBehaviours.Count + 1);
            backgroundBehaviour.Name = name;
            backgroundBehaviour.Behaviour = type;
            
            behaviours[backgroundBehaviour.Appearance] = backgroundBehaviour;
            allBehaviours.Add(backgroundBehaviour);
            Plugin.Log.LogInfo($"Loaded custom card appearance behaviour {name}!");

            return backgroundBehaviour;
        }
    }
}