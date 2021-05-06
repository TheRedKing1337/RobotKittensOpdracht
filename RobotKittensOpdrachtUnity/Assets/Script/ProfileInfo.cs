using UnityEngine;

public struct ProfileInfo
{
    public Sprite picture;
    public string name;
    public string function;

    /// <summary>
    /// Returns a default loading state info struct
    /// </summary>
    public static ProfileInfo GetDefault()
    {
        return new ProfileInfo
        {
            picture = Resources.Load<Sprite>("loading"),
            name = "loading",
            function = ""
        };
    }    
}