using UnityEngine;
using UnityEngine.UI;

public class ImageColorWatcher : MonoBehaviour
{
    private Image img;
    private Color last;

    void Awake()
    {
        img = GetComponent<Image>();
        last = img.color;
    }

    void LateUpdate()
    {
        if (img.color != last)
        {
            //Debug.Log($"[COLOR CHANGED] {gameObject.name} : {last} -> {img.color}");
            last = img.color;
        }
    }
}
