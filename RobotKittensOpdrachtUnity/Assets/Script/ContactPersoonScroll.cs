using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles all scrolling behaviour
/// </summary>
public class ContactPersoonScroll : MonoBehaviour
{
    [Header("Settings:")]
    [SerializeField] private bool useCaching = true; //Wether the random profiles are cached or not
    [SerializeField] private float uiTabSpacing = 5; //The spacing between the uiTabs
    [SerializeField] private float parallaxSpeed = 0.00036f; //The speed at which the background scrolls
    [SerializeField] private float autoScrollSpeed = 250; //The speed at which the autoScroll scrolls

    [Header("References:")]
    [SerializeField] private GameObject uiTabPrefab; //The prefab used by the the profile list, used for getting the height of each tab
    [SerializeField] private GameObject content; //The gameObject that holds the uiTabs
    [SerializeField] private List<GameObject> uiTabs = new List<GameObject>(); //A list of all the uiTabs in the content, in order of top-bottom
    [SerializeField] private GameObject[] parallaxBackgroundObjects; //An array of the backgrounds, 0 is background, 1 is in front of that etc

    private int currentIndex; //The current scrolled position
    private float uiTabHeight; //The height of the uiTab + the spacing
    private float uiScale; //The y scale of the canvas, needed for it to work on different aspect ratios
    private float viewportHeight; //The height of the viewport, needed to know where top and bottom are
    private bool isAutoScrolling; //Wether it is auto scrolling or not

    private Dictionary<int, ProfileInfo> profiles = new Dictionary<int, ProfileInfo>(); //The stored/visited people

    private void Start()
    {
        //Get the canvas y scale, used by scrolling to scale the scroll amount by, needed because canvas is different scales at different aspect ratios
        uiScale = 1 / gameObject.transform.root.localScale.y;
        //Gets the height from the given prefab, allows for easier prefab editing
        uiTabHeight = uiTabPrefab.GetComponent<RectTransform>().sizeDelta.y + uiTabSpacing;
        //Gets the height of the current viewport, has to be different on mobile
        if (Application.platform == RuntimePlatform.Android) viewportHeight = content.GetComponent<RectTransform>().sizeDelta.y;
        else viewportHeight = content.GetComponent<RectTransform>().sizeDelta.y / 2 + uiTabHeight;

        //Reset position to start off
        content.transform.position = new Vector3(content.transform.position.x, viewportHeight - uiTabHeight, content.transform.position.z);
        //Position all the uiTabs, so you dont have to manually adjust the height
        for (int i = 0; i < uiTabs.Count; i++)
        {
            //Fill the current tab with info
            FillInfo(uiTabs[i].gameObject, i);
            //Gets the RectTransform, calculates new height and sets it
            RectTransform toShift = uiTabs[i].GetComponent<RectTransform>();
            float newHeight = -uiTabHeight * i;
            Vector3 newPosition = new Vector3(toShift.localPosition.x, newHeight, toShift.localPosition.z);
            toShift.localPosition = newPosition;
        }        
    }

    /// <summary>
    /// Called by the scroll rect object when the user scrolls
    /// </summary>
    public void OnScroll()
    {
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == gameObject) isAutoScrolling = false;
        float currentHeight = (content.transform.position.y - viewportHeight) * uiScale;
        CheckBounds(currentHeight);
        ScrollBackground(currentHeight);
    }

    /// <summary>
    /// Checks if the user has scrolled far enough for uiTabs to reposition and get new info
    /// </summary>
    /// <param name="scrollHeight">The current scroll height</param>
    private void CheckBounds(float scrollHeight)
    {
        //Calculates the difference in height between the current position and the scrolled position
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
            float newHeight = uiTabHeight * infoIndex;
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
            float newHeight = uiTabHeight * infoIndex;
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
    /// <summary>
    /// Scrolls the background objects at different speeds based on their layer
    /// </summary>
    /// <param name="height">The current scrolled height</param>
    private void ScrollBackground(float height)
    {
        for (int i = 0; i < parallaxBackgroundObjects.Length; i++)
        {
            parallaxBackgroundObjects[i].GetComponent<Image>().material.mainTextureOffset = new Vector2(0, -height * i * parallaxSpeed);
        }
    }
    /// <summary>
    /// When called will automaticly scroll either up or down
    /// </summary>
    /// <param name="scrollingUp"></param>
    public void AutoScroll(bool scrollingUp)
    {
        if (isAutoScrolling)
        {
            isAutoScrolling = false; 
            return;
        }

        isAutoScrolling = true;
        StartCoroutine(AutoScrollRoutine(scrollingUp));
    }
    private IEnumerator AutoScrollRoutine(bool scrollingUp)
    {
        float moveVector = scrollingUp ? autoScrollSpeed : -autoScrollSpeed;
        while (isAutoScrolling)
        {
            content.transform.position = new Vector3(content.transform.position.x, content.transform.position.y + moveVector * Time.deltaTime, content.transform.position.z);
            yield return null;
        }
    }
    public void AddToDict(int index, ProfileInfo pInfo)
    {
        profiles.Add(index, pInfo);
    }
}
