using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TRKGeneric;
using System;

/// <summary>
/// A singleton to get random profiles, is a singleton so it can use coroutines
/// </summary>
public class ProfileRandomizer : MonoSingleton<ProfileRandomizer>
{
    private readonly string[] functionPrefixes = new string[]{"Intern","Junior","Senior","Lead"};
    private readonly string[] functionTitles = new string[]{"Programmer","Manager","Artist","Recruiter","Designer","HR"};

    /// <summary>
    /// Gets a random profile from the api.
    /// </summary>
    /// <param name="pUI">The ProfileUI to fill with the random info</param>
    /// <param name="index">The index to save the info at</param>
    /// <param name="scroller">A reference to the scroll script, used for saving the info</param>
    /// <returns></returns>
    public IEnumerator GetRandomProfileAPI(ProfileUI pUI, int index, ContactPersoonScroll scroller)
    {
        //Init default struct
        ProfileInfo pInfo = ProfileInfo.GetDefault();

        //Get a random person json from the api
        UnityWebRequest request = UnityWebRequest.Get("https://randomuser.me/api/?inc=name,picture&nat=nl&noinfo");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            Debug.Log(request.error);

        RandomUserJson randomUserJson = null;
        //Try to parse the json, can contain errors from the api if its overloaded
        try
        {
            randomUserJson = JsonUtility.FromJson<RandomUserJson>(request.downloadHandler.text);
        }
        catch
        {
            //If returned bad data from api retry
            StartCoroutine(GetRandomProfileAPI(pUI, index, scroller));
            yield break;
        }

        //Set name from api
        pInfo.name = randomUserJson.results[0].name.first + " " + randomUserJson.results[0].name.last;

        //Get the image from the given url
        request = UnityWebRequestTexture.GetTexture(randomUserJson.results[0].picture.medium);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            Debug.Log(request.error);
        else
        {
            Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
            pInfo.picture = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        //Get a random job title
        pInfo.function = GetRandomJobFunction();

        //actually update the UI and save it to the dict
        pUI.SetInfo(pInfo);
        scroller.AddToDict(index, pInfo);
        yield break;
    }

    /// <summary>
    /// Returns a random job function string, made up from the variables in the ProfileRandomizer script
    /// </summary>
    private string GetRandomJobFunction()
    {
        string prefix = functionPrefixes[UnityEngine.Random.Range(0, functionPrefixes.Length)];
        string title = functionTitles[UnityEngine.Random.Range(0, functionTitles.Length)];

        return prefix + " " + title;
    }



    //The json classes below, used for parsing the JSON from the api
    [Serializable]
    public class RandomUserJson
    {
        public Results[] results;
    }
    [Serializable]
    public class Results
    {
        public Name name;
        public Picture picture;
    }
    [Serializable]
    public class Name
    {
        public string title;
        public string first;
        public string last;
    }
    [Serializable]
    public class Picture
    {
        public string large;
        public string medium;
        public string thumbnail;
    }
}