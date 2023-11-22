using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

namespace UI.Documents {
    public class SelectionHUD : MonoBehaviour {
        
       // 1. Property of active block(store active index)
       // 2. SetActiveBlock(by id)(by shift +1, -1)
       // 3. Set active index 0 by default

       private List<BlockDetails> _selectionList = new(9);

       private int _activeIndex = 0;

       public BlockDetails ActiveBlock => _selectionList[_activeIndex];

       private VisualElement _root;

       private const string SELECTED_BUTTON = "selection-button_selected";

       private void Awake() {
           _selectionList.Add(BlockDetails.GetItemByID(1));
           _selectionList.Add(BlockDetails.GetItemByID(2));
           _selectionList.Add(BlockDetails.GetItemByID(3));
           _selectionList.Add(BlockDetails.GetItemByID(5));
           _selectionList.Add(BlockDetails.GetItemByID(10));
           _selectionList.Add(BlockDetails.GetItemByID(42));
           _selectionList.Add(BlockDetails.GetItemByID(11));
           _selectionList.Add(BlockDetails.GetItemByID(6));
           _selectionList.Add(BlockDetails.GetItemByID(4));
       }
       private void OnEnable() {
           _root = GetComponent<UIDocument>().rootVisualElement;
           _root.Q<Button>(_activeIndex + "").AddToClassList(SELECTED_BUTTON);
           var i = 0;
           foreach (var button in _root.Children()) {
               button.Q<VisualElement>().style.backgroundImage = new StyleBackground(_selectionList[i].Sprite);
               i++;
           }
       }

       public void SetActiveSlot(int index) {
           if (index < 0 || index > 9) return;
           _activeIndex = index;
           UpdateBar();
       }

       public void ShiftActiveSlot(bool isBackward) {
           int shiftTo = isBackward ? -1 : 1;
           var newIndex = _activeIndex + shiftTo;
           if (newIndex < 0) newIndex = 9;
           if (newIndex > 9) newIndex = 0;
           _activeIndex = newIndex;
           UpdateBar();
       }

       private void UpdateBar() {
           _root.Q<Button>(className: SELECTED_BUTTON).RemoveFromClassList(SELECTED_BUTTON);
           _root.Q<Button>(_activeIndex + "").AddToClassList(SELECTED_BUTTON);
       }
    }
}
