using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow_State : MonoBehaviour
{
    public Sprite sussce_img;
    public Sprite failed_img;
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        //image=gameObject.AddComponent<Image>();
    }

    public void Arrow_Image_Chage(bool s_or_f)
    {
        image = gameObject.GetComponent<Image>();
        if (s_or_f) image.sprite = sussce_img;
        else image.sprite = failed_img;
    }
}
