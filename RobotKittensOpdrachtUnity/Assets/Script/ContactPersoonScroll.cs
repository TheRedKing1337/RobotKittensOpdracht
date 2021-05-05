using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactPersoonScroll : MonoBehaviour
{
    [SerializeField] private GameObject content;
    private Dictionary<int, ContactPersoon> contactPersonen;

    public void OnScroll(Vector2 position)
    {
        Debug.Log(content.transform.position.y);
    }
    public struct ContactPersoon
    {
        public string pictureUrl;
        public string name;
        public string function;
    }
}
