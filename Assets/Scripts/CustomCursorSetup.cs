using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CustomCursorSetup : MonoBehaviour
{
    [SerializeField] private Canvas cursorCanvas;
    [SerializeField] private Sprite normalCursorSprite;
    [SerializeField] private Sprite clickCursorSprite;
    [SerializeField] private Vector2 cursorSize = new Vector2(32, 32);
    
    private void OnValidate()
    {
        SetupCursorCanvas();
    }
    
    private void Start()
    {
        SetupCursorCanvas();
    }
    
    private void SetupCursorCanvas()
    {
        if (cursorCanvas == null)
        {
            GameObject canvasObj = new GameObject("CursorCanvas");
            canvasObj.transform.SetParent(transform);
            cursorCanvas = canvasObj.AddComponent<Canvas>();
            cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cursorCanvas.sortingOrder = 32767; // Very high sorting order to be on top of everything
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        Image cursorImage = cursorCanvas.GetComponentInChildren<Image>();
        if (cursorImage == null)
        {
            GameObject cursorObj = new GameObject("CustomCursor");
            cursorObj.transform.SetParent(cursorCanvas.transform, false);
            
            RectTransform rectTransform = cursorObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = cursorSize;
            rectTransform.pivot = new Vector2(0, 1); // Set pivot to top-left corner
            
            cursorImage = cursorObj.AddComponent<Image>();
            cursorImage.sprite = normalCursorSprite;
            
            // Find or add CursorManager
            CursorManager manager = FindFirstObjectByType<CursorManager>();
            if (manager == null)
            {
                manager = gameObject.AddComponent<CursorManager>();
            }
            
            // Set references
            manager.GetType().GetField("customCursorImage", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(manager, cursorImage);
                
            manager.GetType().GetField("normalCursorSprite", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(manager, normalCursorSprite);
                
            manager.GetType().GetField("clickCursorSprite", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(manager, clickCursorSprite);
                
            manager.GetType().GetField("cursorCanvas", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(manager, cursorCanvas);
                
            manager.GetType().GetField("useCustomCursor", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(manager, true);
        }
    }
}
