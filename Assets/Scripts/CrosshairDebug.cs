using UnityEngine;

public class CrosshairDebug : MonoBehaviour
{
    void OnGUI()
    {
        float size = 10f;
        float x = Screen.width / 2 - (size / 2);
        float y = Screen.height / 2 - (size / 2);
        GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
    }
}
