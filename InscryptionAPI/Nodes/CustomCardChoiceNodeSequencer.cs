using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public abstract class CustomCardChoiceNodeSequencer : CardChoicesSequencer, INodeSequencer 
    {
        public virtual CardChoicesSequencer SequencerToInherit => EasyAccess.CardChoice;

        public virtual void Inherit()
        {
            selectableCards = new List<SelectableCard>();
            if(SequencerToInherit != null)
            {
                transform.position = SequencerToInherit.transform.position;
                selectableCardPrefab = SequencerToInherit.selectableCardPrefab;
                if (SequencerToInherit.gamepadGrid)
                {
                    gamepadGrid = Instantiate(SequencerToInherit.gamepadGrid);
                    gamepadGrid.transform.parent = SequencerToInherit.gamepadGrid.transform.parent == SequencerToInherit.transform ? transform : SequencerToInherit.gamepadGrid.transform.parent;
                    gamepadGrid.transform.position = SequencerToInherit.gamepadGrid.transform.position;
                }
                choicesView = SequencerToInherit.choicesView;
                if (SequencerToInherit.deckPile)
                {
                    deckPile = Instantiate(SequencerToInherit.deckPile);
                    deckPile.DestroyCardsImmediate();
                    deckPile.transform.parent = SequencerToInherit.deckPile.transform.parent == SequencerToInherit.transform ? transform : SequencerToInherit.deckPile.transform.parent;
                    deckPile.transform.position = SequencerToInherit.deckPile.transform.position;
                    deckPile.gameObject.SetActive(true);
                }
            }
        }

        public abstract IEnumerator DoCustomSequence();
    }
}
