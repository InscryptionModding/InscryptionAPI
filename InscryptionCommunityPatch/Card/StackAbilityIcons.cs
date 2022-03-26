using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class StackAbilityIcons
{
    // This patch modifies ability sigils such that multiple instances of the same sigil
    // are displayed as a single sigil with a number to indicate how many copies there are
    private static Texture2D[] NUMBER_TEXTURES = new Texture2D[]
    {
        TextureHelper.GetImageAsTexture("Stack_1.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_2.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_3.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_4.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_5.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_6.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_7.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_8.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_9.png", typeof(StackAbilityIcons).Assembly)
    };

    private static Texture2D[] MEDIUM_NUMBER_TEXTURES = new Texture2D[]
    {
        TextureHelper.GetImageAsTexture("Stack_1_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_2_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_3_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_4_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_5_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_6_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_7_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_8_med.png", typeof(StackAbilityIcons).Assembly),
        TextureHelper.GetImageAsTexture("Stack_9_med.png", typeof(StackAbilityIcons).Assembly)
    };

    private static Sprite GetGBCNumberSprite(int number)
    {
        var stackGBC = "stack_gbc.png";
        if (!PatchPlugin.act2StackIconType.Value)
        {
            stackGBC = "stack_gbc_alt.png";
        }
        Texture2D texture = TextureHelper.GetImageAsTexture(stackGBC, typeof(StackAbilityIcons).Assembly);
        return Sprite.Create(texture, new Rect(0f, 10f * (9f-number), 15f, 10f), new Vector2(0.5f, 0.5f));
    }

    private static Sprite[] GBC_NUMBER_SPRITES = new Sprite[]
    {
        GetGBCNumberSprite(1),
        GetGBCNumberSprite(2),
        GetGBCNumberSprite(3),
        GetGBCNumberSprite(4),
        GetGBCNumberSprite(5),
        GetGBCNumberSprite(6),
        GetGBCNumberSprite(7),
        GetGBCNumberSprite(8),
        GetGBCNumberSprite(9),
    };

    private static Color[] _topBorder = null;
    private static Color[] TOP_BORDER
    {
        get
        {
            if (_topBorder == null)
            {
                _topBorder = new Color[NUMBER_TEXTURES[0].width + 1];
                for (int i = 0; i < _topBorder.Length; i++)
                    _topBorder[i] = new Color(0f, 0f, 0f, 0f); // Add a completely transparent pixel
            }
            return _topBorder;
        }
    }

    private static Color[] _leftBorder = null;
    private static Color[] LEFT_BORDER
    {
        get
        {
            if (_leftBorder == null)
            {
                _leftBorder = new Color[NUMBER_TEXTURES[0].height + 1];
                for (int i = 0; i < _leftBorder.Length; i++)
                    _leftBorder[i] = new Color(0f, 0f, 0f, 0f); // Add a completely transparent pixel
            }
            return _leftBorder;
        }
    }

    private static int NORMAL = 1;
    private static int MEDIUM = 2;
    private static int FORCED = 3;

    private static Dictionary<string, Texture2D> patchedTexture = new Dictionary<string, Texture2D>();
    private static Dictionary<Ability, Tuple<Vector2Int, int>> patchLocations = new Dictionary<Ability, Tuple<Vector2Int, int>>();

    [HarmonyPatch(typeof(CardAbilityIcons), "GetDistinctShownAbilities")]
    [HarmonyPostfix]
    public static void ClearStackableIcons(ref List<Ability> __result)
    {
        // We'll start by completely removing the stackable icons from the list
        // We will be patching the AbilityIconInteractable class to display the icon
        __result = __result.Distinct().ToList<Ability>();
    }

    private static Vector2Int FindMatchingOnesDigit(Texture2D searchTex, bool normalSize = true)
    {
        Texture2D onesTexture = normalSize ? NUMBER_TEXTURES[0] : MEDIUM_NUMBER_TEXTURES[0];
        Color[] onesColor = onesTexture.GetPixels();
        return FindMatchingTexture(searchTex, onesTexture.width, onesTexture.height, onesColor);
    }

    private static Vector2Int FindMatchingTexture(Texture2D searchTex, int width, int height, Color[] matchPixels = null)
    {
        Color[] searchPixels = searchTex.GetPixels();

        int locX = -1;
        int locY = -1;
        bool failed = false;

        for (int sX = 0; sX < searchTex.width - width; sX++)
        {
            for (int sY = 0; sY < searchTex.height - height; sY++)
            {
                failed = false;

                for (int nX = 0; nX < width; nX++)
                {
                    for (int nY = 0; nY < height; nY++)
                    {
                        int j = nX + (nY * width);
                        int i = sX + nX + (sY + nY) * searchTex.width;

                        if (matchPixels != null)
                        {
                            if (searchPixels[i] != matchPixels[j])
                            {
                                failed = true;
                                break;
                            }
                        } 
                        else 
                        {
                            if (searchPixels[i].a > 0)
                            {
                                failed = true;
                                break;
                            }
                        }

                    }

                    if (failed)
                        break;
                }

                if (failed)
                    continue;

                // Success!
                locX = sX;
                locY = sY;
            }

            if (!failed)
                break;
        }

        return new Vector2Int(locX, locY);
    }

    private static Tuple<Vector2Int, int> FindNextBestLocation(Texture2D texture)
    {
        // First, we want to see if the lower-right corner is available.
        // We will call it available if it's all blank and there is a one-pixel border around it
        Color[] pixels = texture.GetPixels();

        Vector2Int lowerRight = new Vector2Int(texture.width - NUMBER_TEXTURES[0].width, 0);
        bool success = true;
        for (int sX = lowerRight.x - 1; sX < texture.width; sX++)
        {
            for (int sY = lowerRight.y; sY < texture.height + 1; sY++)
            {
                int i = sX + (texture.width * sY);

                if (pixels[i].a > 0f)
                {
                    success = false;
                    break;
                }
            }

            if (!success)
                break;
        }

        if (success)
            return new Tuple<Vector2Int, int>(lowerRight, NORMAL);

        // Okay, the lower right is not clear.
        // At this point, we're just going to look for any clear space.
        // A clear space is all blank and has a one-pixel gap around it
        Vector2Int nextBest = FindMatchingTexture(texture, NUMBER_TEXTURES[0].width + 2, NUMBER_TEXTURES[0].height + 2);
        if (nextBest.x != -1)
            return new Tuple<Vector2Int, int>(nextBest, NORMAL);

        // Ugh. Okay, we have to use the lower right now.
        // And we'll just have to deal with what that looks like.
        return new Tuple<Vector2Int, int>(lowerRight, FORCED);
    }

    private static Tuple<Vector2Int, int> GetPatchLocationForAbility(Ability ability, Texture2D abilityTexture)
    {
        if (patchLocations.ContainsKey(ability))
            return patchLocations[ability];

        // We have to calculate the location

        // First, see if the texture has the 'one' icon on it. If so, we will replace at that location
        Vector2Int oneLoc = FindMatchingOnesDigit(abilityTexture, true);
        if (oneLoc.x != -1)
        {
            patchLocations.Add(ability, new Tuple<Vector2Int, int>(oneLoc, NORMAL));
            return patchLocations[ability];
        }

        // Let's try the medium-sized digit
        oneLoc = FindMatchingOnesDigit(abilityTexture, false);
        if (oneLoc.x != -1)
        {
            patchLocations.Add(ability, new Tuple<Vector2Int, int>(oneLoc, MEDIUM));
            return patchLocations[ability];
        }

        // Now we just use the next best space
        patchLocations.Add(ability, FindNextBestLocation(abilityTexture));
        return patchLocations[ability];
    }

    private static Texture2D DuplicateTexture(Texture2D texture)
    {
        // https://support.unity.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = RenderTexture.GetTemporary( 
                            texture.width,
                            texture.height,
                            0,
                            RenderTextureFormat.Default,
                            RenderTextureReadWrite.Linear);


        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(texture, tmp);

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;

        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = new Texture2D(texture.width, texture.height);

        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = previous;

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        return myTexture2D;
    }

    private static Texture2D PatchTexture(Ability ability, int count)
    {
        if (count <= 1 || count >= 10) // We can't actually patch anything more than 9 stacks right now.
            return null;

        string textureName = $"{ability.ToString()}-icon-{count}-PATCHEDINF";
        if (patchedTexture.ContainsKey(textureName))
            return patchedTexture[textureName];

        // Copy the old texture to the new texture
        Texture2D newTexture = DuplicateTexture(AbilitiesUtil.LoadAbilityIcon(ability.ToString(), false, false) as Texture2D);
        newTexture.name = textureName;

        Tuple<Vector2Int, int> patchTuple = GetPatchLocationForAbility(ability, newTexture);
        Vector2Int patchLocation = patchTuple.Item1;
        int textureType = patchTuple.Item2; // This means that it's on the lower-right and needs a one-pixel border

        if (textureType == FORCED)
        {
            // We will set a one-pixel border around the location
            newTexture.SetPixels(patchLocation.x - 1, patchLocation.y + 1, TOP_BORDER.Length, 1, TOP_BORDER, 0);
            newTexture.SetPixels(patchLocation.x - 1, patchLocation.y + 1, 1, LEFT_BORDER.Length, LEFT_BORDER, 0);
        }
        
        // Set the new number
        Texture2D newNumber = (textureType == NORMAL) ? NUMBER_TEXTURES[count - 1] : MEDIUM_NUMBER_TEXTURES[count - 1];
        newTexture.SetPixels(patchLocation.x, patchLocation.y, newNumber.width, newNumber.height, newNumber.GetPixels(), 0);

        newTexture.filterMode = FilterMode.Point;

        // Apply all of the set pixels
        newTexture.Apply();

        // Upload to the dictionary
        patchedTexture.Add(textureName, newTexture);
        return newTexture;
    }

    [HarmonyPatch(typeof(AbilityIconInteractable), "AssignAbility")]
    [HarmonyPostfix]
    public static void AddIconNumber(Ability ability, CardInfo info, PlayableCard card, ref AbilityIconInteractable __instance)
    {
        if (info == null && card == null)
            return;

        if (!AbilitiesUtil.GetInfo(ability).canStack)
            return;

        // Here's the goal
        // Find all abilities on the card
        // Replace all of the textures where it stacks with a texture showing that it stacks
        PatchPlugin.Logger.LogDebug("Adding new icon; testing for stacks");

        // Okay, go through each ability on the card and see how many instances it has.
        List<Ability> baseAbilities = info.Abilities;
        if (card != null)
            baseAbilities.AddRange(AbilitiesUtil.GetAbilitiesFromMods(card.TemporaryMods));
        
        int count = baseAbilities.Where(ab => ab == ability).Count();

        if (count > 1)
        {
            PatchPlugin.Logger.LogDebug($"Ability {ability.ToString()} stacks {count} times");
            // We need to add an override
            __instance.SetIcon(PatchTexture(ability, count));
        }
    }

    [HarmonyPatch(typeof(PixelCardAbilityIcons), "DisplayAbilities", new Type[] { typeof(List<Ability>), typeof(PlayableCard)})]
    [HarmonyPrefix]
    public static bool PatchPixelCardStacks(ref PixelCardAbilityIcons __instance, List<Ability> abilities, PlayableCard card)
    {
        List<Tuple<Ability, int>> grps = abilities.Distinct().Select(a => new Tuple<Ability, int>(a, abilities.Where(ab => ab == a).Count())).ToList();
        List<GameObject> abilityIconGroups = __instance.abilityIconGroups;

        //InfiniscryptionStackableSigilsPlugin.Log.LogInfo($"abilityIconGroups {abilityIconGroups}");

        if (abilityIconGroups.Count > 0)
        {
            foreach (GameObject gameObject in abilityIconGroups)
                gameObject.gameObject.SetActive(false);
            
            if (grps.Count > 0 && grps.Count - 1 < abilityIconGroups.Count)
            {
                GameObject iconGroup = abilityIconGroups[grps.Count - 1];
                iconGroup.gameObject.SetActive(true);

                List<SpriteRenderer> componentsInChildren = new();
                foreach (Transform child in iconGroup.transform)
                    componentsInChildren.Add(child.gameObject.GetComponent<SpriteRenderer>());

                componentsInChildren.RemoveAll(sr => sr == null);

                for (int i = 0; i < componentsInChildren.Count; i++)
                {
                    SpriteRenderer abilityRenderer = componentsInChildren[i];
                    AbilityInfo info = AbilitiesUtil.GetInfo(grps[i].Item1);
                    abilityRenderer.sprite = info.pixelIcon;
                    if (info.flipYIfOpponent && card != null && card.OpponentCard)
                    {
                        if (info.customFlippedPixelIcon)
                            abilityRenderer.sprite = info.customFlippedPixelIcon;
                        else
                            abilityRenderer.flipY = true;
                    }
                    else
                    {
                        abilityRenderer.flipY = false;
                    }

                    // And now my custom code to add the ability counter
                    // But only if we need to
                    Transform countTransform = abilityRenderer.transform.Find("Count");

                    if (countTransform == null && grps[i].Item2 <= 1)
                        continue;

//InfiniscryptionStackableSigilsPlugin.Log.LogInfo($"countTransform {countTransform}");
                    if (countTransform == null)
                    {
                        GameObject counter = new GameObject();
                        counter.transform.SetParent(abilityRenderer.transform);
                        counter.layer = LayerMask.NameToLayer("GBCPauseMenu");
                        SpriteRenderer renderer = counter.AddComponent<SpriteRenderer>();
                        renderer.size = new Vector2(0.14f, 0.08f);
                        renderer.color = new Color(1f, 1f, 1f, 1f);
                        renderer.adaptiveModeThreshold = 0.5f;
                        renderer.enabled = true;
                        renderer.sortingLayerName = "PauseMenuUI";
                        renderer.sortingOrder = 200;

                        counter.name = "Count";
                        counter.transform.localPosition = new Vector3(.03f, -.05f, 0f);
                        countTransform = counter.transform;
                    }
                    //InfiniscryptionStackableSigilsPlugin.Log.LogInfo($"countTransform.gameObject {countTransform.gameObject}");
                    //InfiniscryptionStackableSigilsPlugin.Log.LogInfo($"countTransform.gameObject.spriterenderer {countTransform.gameObject.GetComponent<SpriteRenderer>()}");

                    if (grps[i].Item2 <= 1)
                        countTransform.gameObject.SetActive(false);
                    else
                    {
                        countTransform.gameObject.SetActive(true);
                        countTransform.gameObject.GetComponent<SpriteRenderer>().sprite = GBC_NUMBER_SPRITES[grps[i].Item2 - 1];
                    }
                }
            }
            __instance.conduitIcon.SetActive(abilities.Exists((Ability x) => AbilitiesUtil.GetInfo(x).conduit));
            Ability ability = abilities.Find((Ability x) => AbilitiesUtil.GetInfo(x).activated);

            PixelActivatedAbilityButton button = __instance.activatedAbilityButton;
            if (ability > Ability.None)
            {
                button.gameObject.SetActive(true);
                button.SetAbility(ability);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
        return false;
    }

    public static string StackDescription(string input)
    {
        string[] lines = input.Split('\n');
        string retval = lines[0];
        int duplicateCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i] == lines[i-1])
            {
                duplicateCount += 1;
            }
            else
            {
                if (duplicateCount > 0)
                    retval = retval + $" (x{duplicateCount + 1})";
                retval = retval + "\n" + lines[i];
                duplicateCount = 0;
            }
        }
        if (duplicateCount > 0 )
        {
            retval = retval + $" (x{duplicateCount + 1})";
        } 
        return retval;
    }

    [HarmonyPatch(typeof(CardInfo), "GetGBCDescriptionLocalized")]
    [HarmonyPostfix]
    public static void GetStackedGBCDescriptionLocalized(ref string __result)
    {
        __result = StackDescription(__result);        
    }
}