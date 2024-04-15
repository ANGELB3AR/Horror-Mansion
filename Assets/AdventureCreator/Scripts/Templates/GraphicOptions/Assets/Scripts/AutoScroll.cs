/** Code adapted from Razputin's comment on https://answers.unity.com/questions/1836381/auto-scroll-to-selected-button-in-grid-when-outsid.html */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AC.Templates.GraphicOptions
{

	public class AutoScroll : MonoBehaviour, ISubmitHandler
	{

		#region Variables

		private Dropdown dropdown;
		private Toggle[] toggleArray;
		private RectTransform inventoryScrollRectTransform;
		private RectTransform inventoryContentPanel;
		private RectTransform oldRect;

		#endregion


		#region PrivateFunctions

		public void OnSubmit (BaseEventData eventData)
		{
			dropdown = GetComponent<Dropdown> ();
			inventoryContentPanel = FindChild (gameObject, "Content");
			inventoryScrollRectTransform = FindChild (gameObject, "Scrollbar");
			if (inventoryContentPanel == null || inventoryScrollRectTransform == null) return;

			toggleArray = inventoryContentPanel.GetComponentsInChildren<Toggle> ();
			ApplyEvents ();

			if (dropdown != null && dropdown.value < toggleArray.Length)
			{
				SnapTo (toggleArray[dropdown.value].GetComponent<RectTransform> ());
			}
		}

		#endregion


		#region PublicFunctions

		private void SnapTo (RectTransform target)
		{
			int index = -1;
			for (int i = 0; i < toggleArray.Length; i++)
			{
				if (toggleArray[i].gameObject == target.gameObject)
				{
					index = i;
					break;
				}
			}

			if (index < 0)
			{
				return;
			}

			RectTransform rect = toggleArray[index].GetComponent<RectTransform> ();
			Vector2 v = rect.position;
			bool inView = RectTransformUtility.RectangleContainsScreenPoint (inventoryScrollRectTransform, v);
			float incrementSize = rect.rect.height;

			if (!inView && oldRect == null)
			{
				inventoryContentPanel.anchoredPosition = new Vector2 (0, incrementSize) * index;
			}

			if (!inView && oldRect)
			{
				if (oldRect.localPosition.y < rect.localPosition.y)
				{
					inventoryContentPanel.anchoredPosition -= new Vector2 (0, incrementSize);
				}
				else if (oldRect.localPosition.y > rect.localPosition.y)
				{
					inventoryContentPanel.anchoredPosition += new Vector2 (0, incrementSize);
				}
			}

			oldRect = rect;
		}


		private void ApplyEvents ()
		{
			foreach (Toggle toggle in toggleArray)
			{
				EventTrigger.Entry entry = new EventTrigger.Entry ();
				entry.eventID = EventTriggerType.Select;
				entry.callback.AddListener ((eventData) => { SnapTo (toggle.GetComponent<RectTransform> ()); });

				EventTrigger eventTrigger = toggle.GetComponent<EventTrigger> ();
				if (eventTrigger == null) eventTrigger = toggle.gameObject.AddComponent<EventTrigger> ();
				eventTrigger.triggers.Add (entry);
			}
		}


		private RectTransform FindChild (GameObject parent, string objectName)
		{
			RectTransform[] childTransforms = parent.GetComponentsInChildren<RectTransform> ();
			foreach (RectTransform childTransform in childTransforms)
			{
				if (childTransform.gameObject.name == objectName && childTransform.gameObject.activeInHierarchy)
				{
					return childTransform;
				}
			}
			return null;
		}

		#endregion

	}

}