using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] private Image pictureImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text functionText;

    public void SetInfo(ProfileInfo info)
    {
        pictureImage.sprite = info.picture;
        nameText.text = info.name;
        functionText.text = info.function;
    }
}
