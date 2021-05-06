using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] private Image pictureImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text functionText;

    /// <summary>
    /// Fills this gameObject with the given info, sets the text and image elements
    /// </summary>
    /// <param name="info">The info to fill in</param>
    public void SetInfo(ProfileInfo info)
    {
        pictureImage.sprite = info.picture;
        nameText.text = info.name;
        functionText.text = info.function;
    }
}
