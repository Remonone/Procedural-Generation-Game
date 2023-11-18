using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Documents {
    public class SelectionHUD : MonoBehaviour {
        
       // 1. Property of active block(store active index)
       // 2. SetActiveBlock(by id)(by shift +1, -1)
       // 3. Set active index 0 by default

       private BlockDetails[] _selectionList = new []{
           BlockDetails.GetItemByID(1),
           BlockDetails.GetItemByID(2),
           BlockDetails.GetItemByID(3),
           BlockDetails.GetItemByID(5),
           BlockDetails.GetItemByID(10),
           BlockDetails.GetItemByID(42), 
           BlockDetails.GetItemByID(11),
           BlockDetails.GetItemByID(6),
           BlockDetails.GetItemByID(4)
       };

       private int _activeIndex = 0;

       public BlockDetails ActiveBlock => _selectionList[_activeIndex];

       private VisualElement _root;

       private const string SELECTED_BUTTON = "selection-button_selected";


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

       public void ShiftActiveSlot(bool isForward) {
           int shiftTo = isForward ? 1 : -1;
           var newIndex = _activeIndex + shiftTo;
           if (newIndex < 0) _activeIndex = 9;
           if (newIndex > 9) _activeIndex = 0;
           UpdateBar();
       }

       private void UpdateBar() {
           _root.Q<Button>("", SELECTED_BUTTON).RemoveFromClassList(SELECTED_BUTTON);
           _root.Q<Button>(_activeIndex + "").AddToClassList(SELECTED_BUTTON);
       }
    }
}
