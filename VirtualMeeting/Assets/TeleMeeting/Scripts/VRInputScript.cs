using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class VRInputScript : BaseInputModule
{
    public Camera cam;
    public SteamVR_Input_Sources lHand;
    public SteamVR_Input_Sources rHand;
    public SteamVR_Action_Boolean clickAction;

    //private GameObject rHand;
    private GameObject currentObject = null;
    private PointerEventData data = null;
    // Start is called before the first frame update
    protected override void Awake()
    {
        //rHand = transform.parent.GetChild(0).GetChild(2).gameObject;
        base.Awake();
        data = new PointerEventData(eventSystem);
    }

    public override void Process()
    {
        data.Reset();
        data.position = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);
        
        eventSystem.RaycastAll(data, m_RaycastResultCache);
        if (data.pointerPress == null)
        {
            data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            currentObject = data.pointerCurrentRaycast.gameObject;
        }

        m_RaycastResultCache.Clear();
        
        HandlePointerExitAndEnter(data, currentObject);
        
        if (clickAction.GetStateDown(lHand) || clickAction.GetStateDown(rHand))
            ProcessPress(data);
        
        if (clickAction.GetStateUp(lHand) || clickAction.GetStateUp(rHand))
            ProcessRelease(data);
        
        if (clickAction.GetState(lHand) || clickAction.GetState(rHand))
            ProcessDrag(data);
    }

    public PointerEventData GetData()
    {
        return data;
    }

    private void ProcessPress(PointerEventData data)
    {
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        GameObject newPointerPress =
            ExecuteEvents.ExecuteHierarchy(currentObject, data, ExecuteEvents.pointerDownHandler);

        if (newPointerPress == null)
        {
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);
        }

        data.pressPosition = data.position;
        data.pointerPress = newPointerPress;
        data.rawPointerPress = currentObject;
    }
    
    private void ProcessRelease(PointerEventData data)
    {
        ExecuteEvents.ExecuteHierarchy(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);

        if (data.pointerPress == pointerUpHandler)
        {
            ExecuteEvents.ExecuteHierarchy(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
        }
        
        eventSystem.SetSelectedGameObject(null);
        data.pressPosition = Vector2.zero;
        data.pointerPress = null;
        data.rawPointerPress = null;
        
    }

    private void ProcessDrag(PointerEventData data)
    {
        //Debug.Log(data.pointerCurrentRaycast);
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        GameObject newPointerDrag = ExecuteEvents.ExecuteHierarchy(currentObject, data, ExecuteEvents.dragHandler);

        if (newPointerDrag == null)
        {
            newPointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentObject);
        }   
        
        data.pressPosition = data.position;
        data.pointerDrag = newPointerDrag;
        data.rawPointerPress = currentObject;
    }
}
