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
          var newCard = NewCard.cards[item.Key];
          if (id.id != 0)
          {
            // if the card already has the ability then no point and adding it
            if (!newCard.abilities.Contains(id.id)) newCard.abilities.Add(id.id);
          }
          else
          {
            Log.LogWarning($"Ability {id} not found for card {newCard}");
          }
        }
      }

      foreach(var item in CustomCard.abilityIds)
      {
        foreach (AbilityIdentifier id in item.Value)
        {
          var customCard = CustomCard.cards[item.Key];
          if (id.id != 0)
          {
            // if the card already has the ability then no point and adding it
            if (!customCard.abilities.Contains(id.id)) customCard.abilities.Add(id.id);
          }
          else
          {
            Log.LogWarning($"Ability {id} not found for card {customCard}");
          }
        }
      }
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
            // if the card already has the ability then no point and adding it
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
            // if the card already has the ability then no point and adding it
            if (!customCard.specialAbilities.Contains(id.id)) customCard.specialAbilities.Add(id.id);
          }
          else
          {
            Log.LogWarning($"Special Ability {id} not found for card {customCard}");
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
