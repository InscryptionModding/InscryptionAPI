namespace APIPlugin
{
  public partial class Plugin
  {
    private void SetAbilityIdentifiers()
    {
      foreach(var item in NewCard.abilityIds)
      {
        foreach (AbilityIdentifier id in item.Value)
        {
          if (id.id != 0)
          {
            NewCard.cards[item.Key].abilities.Add(id.id);
          }
          else
          {
            Plugin.Log.LogWarning($"Ability {id} not found for card {NewCard.cards[item.Key]}");
          }
        }
      }

      foreach(var item in CustomCard.abilityIds)
      {
        foreach (AbilityIdentifier id in item.Value)
        {
          if (id.id != 0)
          {
            CustomCard.cards[item.Key].abilities.Add(id.id);
          }
          else
          {
            Plugin.Log.LogWarning($"Ability {id} not found for card {CustomCard.cards[item.Key]}");
          }
        }
      }
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
          Plugin.Log.LogWarning($"Evolution card {id} not found for card {NewCard.cards[item.Key]}");
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
          Plugin.Log.LogWarning($"Evolution card {id} not found for card {CustomCard.cards[item.Key]}");
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
          Plugin.Log.LogWarning($"IceCube card {id} not found for card {NewCard.cards[item.Key]}");
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
          Plugin.Log.LogWarning($"IceCube card {id} not found for card {CustomCard.cards[item.Key]}");
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
          Plugin.Log.LogWarning($"Tail card {id} not found for card {NewCard.cards[item.Key]}");
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
          Plugin.Log.LogWarning($"Tail card {id} not found for card {CustomCard.cards[item.Key]}");
        }
      }
    }
  }
}
