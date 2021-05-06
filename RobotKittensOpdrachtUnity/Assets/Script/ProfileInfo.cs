using UnityEngine;
using System.Collections;

public struct ProfileInfo
{
    public Sprite picture;
    public string name;
    public string function;

    public static ProfileInfo GetDefault()
    {
        return new ProfileInfo
        {
            picture = Resources.Load("default") as Sprite,
            name = "loading",
            function = ""
        };
    }    
}