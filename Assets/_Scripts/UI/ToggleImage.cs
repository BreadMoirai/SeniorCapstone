using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleImage : MonoBehaviour
{
    [SerializeField] Sprite onImage;
    [SerializeField] Sprite offImage;

    private bool isOn = true;
    private Image image;

    private void Awake()
    {
        if (gameObject.GetComponent<Image>() != null)
            image = gameObject.GetComponent<Image>();
    }

    public void ToggleSprite()
    {
        isOn = !isOn;
        image.sprite = isOn
            ? onImage
            : offImage;
    }
}
