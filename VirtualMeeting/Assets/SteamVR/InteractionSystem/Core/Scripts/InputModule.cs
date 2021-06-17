//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes the hand act as an input module for Unity's event system
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class InputModule : BaseInputModule
	{
		private GameObject submitObject;
		private PointerEventData pointerEventData;
		//-------------------------------------------------
		private static InputModule _instance;
		public static InputModule instance
		{
			get
			{
				if ( _instance == null )
					_instance = GameObject.FindObjectOfType<InputModule>();

				return _instance;
			}
		}
		protected override void Awake()
		{
			base.Awake();
			pointerEventData = new PointerEventData(eventSystem);
		}

		//-------------------------------------------------
		public override bool ShouldActivateModule()
		{
			if ( !base.ShouldActivateModule() )
				return false;

			return submitObject != null;
		}
		public RaycastResult getNearestResult(List<RaycastResult> rayCastList)
        {
			return (FindFirstRaycast(rayCastList));
        }

		//-------------------------------------------------
		public void HoverBegin( GameObject gameObject )
		{
			PointerEventData pointerEventData2 = new PointerEventData(eventSystem);
			pointerEventData2.selectedObject = gameObject;
			ExecuteEvents.ExecuteHierarchy( gameObject, pointerEventData2, ExecuteEvents.pointerEnterHandler );
		}


		//-------------------------------------------------
		public void HoverEnd( GameObject gameObject )
		{
			PointerEventData pointerEventData2 = new PointerEventData(eventSystem);
			pointerEventData2.selectedObject = null;
			ExecuteEvents.ExecuteHierarchy( gameObject, pointerEventData2, ExecuteEvents.pointerExitHandler );
		}

		public void PointerDown(GameObject gameObject, RaycastResult ray)
        {
			Debug.Log("Pointer Down Called!");
			pointerEventData.Reset();
			
			pointerEventData.pointerPressRaycast = ray;
			pointerEventData.pointerCurrentRaycast = ray;
			pointerEventData.pressPosition = ray.worldPosition;
			pointerEventData.useDragThreshold = true;

			//pointerEventData.selectedObject = gameObject;
			GameObject newPointerPress =
			ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.pointerDownHandler);

			if (newPointerPress == null)
			{
				newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}

			pointerEventData.pressPosition = pointerEventData.position;
			pointerEventData.pointerPress = newPointerPress;
			pointerEventData.rawPointerPress = gameObject;
		}

		public void PointerHold( GameObject gameObject, RaycastResult ray)
        {
			Debug.Log("Pointer Hold Called!");
			pointerEventData.Reset();
			
			pointerEventData.pointerPressRaycast = ray;
			pointerEventData.pointerCurrentRaycast = ray;
			//pointerEventData.selectedObject = gameObject;
			GameObject newPointerDrag = ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.dragHandler);

			if (newPointerDrag == null)
			{
				newPointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			}

			pointerEventData.pressPosition = pointerEventData.position;
			pointerEventData.pointerDrag = newPointerDrag;
			pointerEventData.rawPointerPress = gameObject;
		}

		public void PointerUp(GameObject gameObject, RaycastResult ray)
        {
			Debug.Log("Pointer Up Called!");
			pointerEventData.Reset();
			//pointerEventData.selectedObject = gameObject;
			ExecuteEvents.Execute(pointerEventData.pointerPress, pointerEventData, ExecuteEvents.pointerUpHandler);

			GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);

			if (pointerEventData.pointerPress == pointerUpHandler)
			{
				ExecuteEvents.Execute(pointerEventData.pointerPress, pointerEventData, ExecuteEvents.pointerClickHandler);
			}

			eventSystem.SetSelectedGameObject(null);
			pointerEventData.pressPosition = Vector2.zero;
			pointerEventData.pointerPress = null;
			pointerEventData.rawPointerPress = null;
		}

		//-------------------------------------------------
		public void Submit( GameObject gameObject )
		{
			submitObject = gameObject;
		}


		//-------------------------------------------------
		public override void Process()
		{
			if ( submitObject )
			{
				Debug.Log("Submit On: " + submitObject.name);
				BaseEventData data = GetBaseEventData();
				data.selectedObject = submitObject;
				ExecuteEvents.ExecuteHierarchy( submitObject, data, ExecuteEvents.submitHandler);

				submitObject = null;
			}
		}
	}
}
