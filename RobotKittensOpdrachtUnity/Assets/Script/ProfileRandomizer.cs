using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TRKGeneric;
using System;

public class ProfileRandomizer : MonoSingleton<ProfileRandomizer>
{
    private readonly string[] functionPrefixes = new string[]{"Intern","Junior","Senior"};
    private readonly string[] functionTitles = new string[]{"Programmer","Manager","Artist"};

    public IEnumerator GetRandomProfile(bool useApi, ProfileUI pUI, int index = 0, ContactPersoonScroll scroller = null)
    {
        if (useApi) StartCoroutine(GetRandomProfileAPI(pUI, index, scroller));
        else StartCoroutine(GetRandomProfileLocal(pUI));
        yield break;
    }

    private IEnumerator GetRandomProfileAPI(ProfileUI pUI, int index, ContactPersoonScroll scroller)
    {
        //Init default struct
        ProfileInfo pInfo = ProfileInfo.GetDefault();

        //Get a random person json from the api
        UnityWebRequest request = UnityWebRequest.Get("https://randomuser.me/api/?inc=name,picture&nat=nl&noinfo");
        yield return request.SendWebRequest();
        RandomUserJson randomUserJson = JsonUtility.FromJson<RandomUserJson>(request.downloadHandler.text);

        //Set name from api
        pInfo.name = randomUserJson.results[0].name.first + " " + randomUserJson.results[0].name.last;

        //Get the image from the given url
        request = UnityWebRequestTexture.GetTexture(randomUserJson.results[0].picture.medium);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
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
    private IEnumerator GetRandomProfileLocal(ProfileUI pUI)
    {
        throw new NotImplementedException();
    }

    private string GetRandomJobFunction()
    {
        string prefix = functionPrefixes[UnityEngine.Random.Range(0, functionPrefixes.Length)];
        string title = functionTitles[UnityEngine.Random.Range(0, functionTitles.Length)];

        return prefix + " " + title;
    }



    //The json classes below
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