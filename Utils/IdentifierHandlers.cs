using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
  public partial class Plugin
  {
    private void SetAbilityIdentifiers()
    {
      ImportNewAbilities();

      Log.LogDebug($"Number of ability IDs to set from NewCard.abilityIds: [{NewCard.abilityIds.Count}]");
      foreach(var item in NewCard.abilityIds)
      {
        foreach (AbilityIdentifier id in item.Value)
        {
          var newCard = NewCard.cards[item.Key];
          if (id.id != 0)
          {
            // if the card already has the ability then no point and adding it
            if (AbilityDoesNotExistOrCanStack(newCard.abilities, id))
              newCard.abilities.Add(id.id);
          }
          else
          {
            Log.LogWarning($"Ability {id} not found for card {newCard}");
          }
        }
      }

      Log.LogDebug($"Number of ability IDs to set from CustomCard.abilityIds: [{CustomCard.abilityIds.Count}]");
      foreach(var item in CustomCard.abilityIds)
      {
        foreach (AbilityIdentifier id in item.Value)
        {
          var customCard = CustomCard.cards[item.Key];
          if (id.id != 0)
          {
            if (AbilityDoesNotExistOrCanStack(customCard.abilities, id)) 
              customCard.abilities.Add(id.id);
          }
          else
          {
            Log.LogWarning($"Ability {id} not found for card {customCard}");
          }
        }
      }
    }

    private static void ImportNewAbilities()
    {
      Log.LogDebug($"Starting pre-emptive data load for AbilityInfo before adding custom abilities...");
      List<AbilityInfo> official = ScriptableObjectLoader<AbilityInfo>.AllData;
      
      foreach (NewAbility newAbility in NewAbility.abilities)
      {
        Log.LogDebug($"Added [{newAbility.info.rulebookName}] to official ability list!");
        official.Add(newAbility.info);
      }

      ScriptableObjectLoader<AbilityInfo>.allData = official;
      Log.LogInfo($"Loaded {NewAbility.abilities.Count} custom abilities into data! " +
                  $"Total of [{ScriptableObjectLoader<AbilityInfo>.allData}]");
    }

    private void SetSpecialAbilityIdentifiers()
    {
      foreach(var item in NewCard.specialAbilityIds)
      {
        foreach (SpecialAbilityIdentifier id in item.Value)
        {
          var newCard = NewCard.cards[item.Key];
          if (id.id != 0)
          {
            // Special Abilities do not stack, unlike regular Abilities
            if (!newCard.specialAbilities.Contains(id.id)) newCard.specialAbilities.Add(id.id);
          }
          else
          {
            Log.LogWarning($"Special Ability {id} not found for card {newCard}");
          }
        }
      }
    
      foreach(var item in CustomCard.specialAbilityIds)
      {
        foreach (SpecialAbilityIdentifier id in item.Value)
        {
          var customCard = CustomCard.cards[item.Key];
          if (id.id != 0)
          {
            // Special Abilities do not stack, unlike regular Abilities
            if (!customCard.specialAbilities.Contains(id.id)) customCard.specialAbilities.Add(id.id);
          }
          else
          {
            Log.LogWarning($"Special Ability {id} not found for card {customCard}");
          }
        }
      }
    }
    
    private static bool AbilityDoesNotExistOrCanStack(List<Ability> abilities, AbilityIdentifier id)
    {
      Log.LogDebug($"List of abilities [{abilities}] with id [{id.id}]");
      return !abilities.Contains(id.id) || AbilitiesUtil.GetInfo(id.id).canStack;
    }

    private void SetEvolveIdentifiers()
    {
      foreach(var item in NewCard.evolveIds)
      {
        EvolveIdentifier id = item.Value;
        if (id.Evolution != null)
        {
          NewCard.cards[item.Key].evolveParams = id.Evolution;
        }
        else
        {
          Log.LogWarning($"Evolution card {id} not found for card {NewCard.cards[item.Key]}");
        }
      }

      foreach(var item in CustomCard.evolveIds)
      {
        EvolveIdentifier id = item.Value;
        if (id.Evolution != null)
        {
          CustomCard.cards[item.Key].evolveParams = id.Evolution;
        }
        else
        {
          Log.LogWarning($"Evolution card {id} not found for card {CustomCard.cards[item.Key]}");
        }
      }
    }

    private void SetIceCubeIdentifiers()
    {
      foreach(var item in NewCard.iceCubeIds)
      {
        IceCubeIdentifier id = item.Value;
        if (id.IceCube != null)
        {
          NewCard.cards[item.Key].iceCubeParams = id.IceCube;
        }
        else
        {
          Log.LogWarning($"IceCube card {id} not found for card {NewCard.cards[item.Key]}");
        }
      }

      foreach(var item in CustomCard.iceCubeIds)
      {
        IceCubeIdentifier id = item.Value;
        if (id.IceCube != null)
        {
          CustomCard.cards[item.Key].iceCubeParams = id.IceCube;
        }
        else
        {
          Log.LogWarning($"IceCube card {id} not found for card {CustomCard.cards[item.Key]}");
        }
      }
    }

    private void SetTailIdentifiers()
    {
      foreach(var item in NewCard.tailIds)
      {
        TailIdentifier id = item.Value;
        if (id.Tail != null)
        {
          NewCard.cards[item.Key].tailParams = id.Tail;
        }
        else
        {
          Log.LogWarning($"Tail card {id} not found for card {NewCard.cards[item.Key]}");
        }
      }

      foreach(var item in CustomCard.tailIds)
      {
        TailIdentifier id = item.Value;
        if (id.Tail != null)
        {
          CustomCard.cards[item.Key].tailParams = id.Tail;
        }
        else
        {
          Log.LogWarning($"Tail card {id} not found for card {CustomCard.cards[item.Key]}");
        }
      }
    }
  }
}
