using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactPersoonScroll : MonoBehaviour
{
    [Header("Settings:")]
    [SerializeField] private bool useCaching = true; //Wether the random profiles are cached or not
    [SerializeField] private float uiTabSpacing = 5; //The spacing between the uiTabs

    [Header("References:")]
    [SerializeField] private GameObject uiTabPrefab; //The prefab used by the the profile list, used for getting the height of each tab
    [SerializeField] private GameObject content; //The gameObject that holds the uiTabs
    [SerializeField] private List<GameObject> uiTabs = new List<GameObject>(); //A list of all the uiTabs in the content, in order of top-bottom

    private int currentIndex; //The current scrolled position
    private float uiTabHeight; //The height of the uiTab + the spacing
    private float uiScale; //The y scale of the canvas, needed for it to work on different aspect ratios

    private Dictionary<int, ProfileInfo> profiles = new Dictionary<int, ProfileInfo>(); //The stored/visited people

    private void Start()
    {
        //Get the canvas y scale
        uiScale = 1 / gameObject.transform.root.localScale.y;
        //Gets the height from the given prefab, allows for easier prefab editing
        uiTabHeight = uiTabPrefab.GetComponent<RectTransform>().sizeDelta.y + uiTabSpacing;
        //Position all the uiTabs, so you dont have to manually adjust the height
        for (int i = 0; i < uiTabs.Count; i++)
        {
            //Fill the current tab with info
            FillInfo(uiTabs[i].gameObject, i);
            //Gets the RectTransform, calculates new height and sets it
            RectTransform toShift = uiTabs[i].GetComponent<RectTransform>();
            float newHeight = -uiTabHeight * i + uiTabHeight / 2; //+ uiTabHeight / 2 because the first one has to be above the viewport
            Vector3 newPosition = new Vector3(toShift.localPosition.x, newHeight, toShift.localPosition.z);
            toShift.localPosition = newPosition;
        }
    }

    /// <summary>
    /// Called by the scroll rect object when the user scrolls
    /// </summary>
    public void OnScroll()
    {
        CheckBounds(content.transform.position.y * uiScale - 1200); //-1200 because that is the starting height, want to start at 0
    }

    /// <summary>
    /// Checks if the user has scrolled far enough for uiTabs to reposition and get new info
    /// </summary>
    /// <param name="scrollHeight">The current scroll height</param>
    private void CheckBounds(float scrollHeight)
    {
        float currentHeight = currentIndex * uiTabHeight;
        float changedHeight = scrollHeight - currentHeight;
        //Gets the new scrolled index
        int scrolledSteps = Mathf.FloorToInt(changedHeight / uiTabHeight);


        //If the index didnt change return
        if (scrolledSteps == 0) return;

        bool scrolledDown = scrolledSteps < 0; //The scroll direction, if negative it scrolled down

        //for each index it scrolled, shift the uiTab. This is neccessary incase fps is so low it scrolls multiple per frame
        for (int i = 0; i < Mathf.Abs(scrolledSteps); i++)
        {
            //If the newIndex is bigger the list moved down, else it scrolled up
            currentIndex += (int)Mathf.Sign(scrolledSteps);
            ShiftUiTabs(scrolledDown);
        }

    }

    /// <summary>
    /// Handles the shifting of ui elements, shifts either 1 up or 1 down
    /// </summary>
    /// <param name="scrolledDown">Wether the ui should shift an element down or up</param>
    private void ShiftUiTabs(bool scrolledDown)
    {
        if (scrolledDown)
        {
            //Gets the RectTransform, calculates new height and sets it
            RectTransform toShift = uiTabs[uiTabs.Count - 1].GetComponent<RectTransform>();
            int infoIndex = -currentIndex;
            float newHeight = uiTabHeight * infoIndex + +uiTabHeight / 2;
            Vector3 newPosition = new Vector3(toShift.localPosition.x, newHeight, toShift.localPosition.z);
            toShift.localPosition = newPosition;

            //Caches gameObject, removes from bottom of list and inserts at top
            GameObject toTop = uiTabs[uiTabs.Count - 1];
            uiTabs.RemoveAt(uiTabs.Count - 1);
            uiTabs.Insert(0, toTop);

            //Update the info of this object
            FillInfo(toTop, infoIndex);
        }
        else
        {
            //Gets the RectTransform, calculates new height and sets it
            RectTransform toShift = uiTabs[0].GetComponent<RectTransform>();
            int infoIndex = -currentIndex - uiTabs.Count + 1;
            float newHeight = uiTabHeight * infoIndex + uiTabHeight / 2;
            Vector3 newPosition = new Vector3(toShift.localPosition.x, newHeight, toShift.localPosition.z);
            toShift.localPosition = newPosition;

            //Caches gameObject, removes from top of list and inserts at bottom
            GameObject toBottom = uiTabs[0];
            uiTabs.RemoveAt(0);
            uiTabs.Add(toBottom);

            //Update the info of this object
            FillInfo(toBottom, infoIndex);
        }
    }

    /// <summary>
    /// Fill the given gameObject with random info, or saved info if the given index has been saved
    /// </summary>
    /// <param name="uiObject">The object to fill with info</param>
    /// <param name="index">The index of the tab</param>
    private void FillInfo(GameObject uiObject, int index)
    {
        ProfileUI pUi = uiObject.GetComponent<ProfileUI>();

        ProfileInfo pInfo = ProfileInfo.GetDefault();
        //If uses caching try to get value from the profiles dict
        if (useCaching)
        {           
            //If no value is found make a new entry in the dict
            if (!profiles.TryGetValue(index, out pInfo))
            {
                StartCoroutine(ProfileRandomizer.Instance.GetRandomProfileAPI(pUi, index, this));
                pInfo = ProfileInfo.GetDefault();
            }
        }
        else
        {
            StartCoroutine(ProfileRandomizer.Instance.GetRandomProfileAPI(pUi, index, this));
        }

        pUi.SetInfo(pInfo);
    }
    public void AddToDict(int index, ProfileInfo pInfo)
    {
        profiles.Add(index, pInfo);
    }
}
