using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace CardLoaderPlugin.lib
{
    public class CardUtils
    {
        
        public static readonly Vector2 DefaultVector2 = new Vector2(0.5f, 0.5f);
        public static readonly Rect DefaultCardArtRect = new Rect(0.0f, 0.0f, 114.0f, 94.0f);
        public static readonly Rect DefaultCardPixelArtRect = new Rect(0.0f, 0.0f, 41.0f, 28.0f);
        
        public static List<CardAppearanceBehaviour.Appearance> getRareAppearance =
            new List<CardAppearanceBehaviour.Appearance>() { CardAppearanceBehaviour.Appearance.RareCardBackground };

        public static List<CardMetaCategory> getNormalCardMetadata = new List<CardMetaCategory>()
            { CardMetaCategory.ChoiceNode };
        
        public static List<CardMetaCategory> getRareCardMetadata = new List<CardMetaCategory>()
            { CardMetaCategory.ChoiceNode, CardMetaCategory.Rare };

        public static Texture2D loadImage(string pathCardArt)
        {
            if (!System.IO.File.Exists(pathCardArt))
            {
                throw new Exception($"Unable to find image for {pathCardArt}");
            }
            else
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] imgBytes = System.IO.File.ReadAllBytes(pathCardArt);
                bool isLoaded = texture.LoadImage(imgBytes);
                return texture;
            }
        }
    }
}