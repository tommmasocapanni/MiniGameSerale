using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Custom Cursor")]
    [SerializeField] private bool useCustomCursor = false;
    [SerializeField] private Image customCursorImage;
    [SerializeField] private Sprite normalCursorSprite;
    [SerializeField] private Sprite clickCursorSprite;
    [SerializeField] private Canvas cursorCanvas;

    private bool isSystemCursorVisible = false;
    private bool isInventoryOpen = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup custom cursor
        if (useCustomCursor && customCursorImage != null)
        {
            customCursorImage.sprite = normalCursorSprite;
            HideSystemCursor();
            ShowCustomCursor(true);
        }
        else
        {
            ShowCustomCursor(false);
            HideSystemCursor();
        }
    }

    private void Update()
    {
        if (useCustomCursor && customCursorImage != null && customCursorImage.gameObject.activeSelf)
        {
            // Update custom cursor position
            Vector2 cursorPos = Input.mousePosition;
            customCursorImage.rectTransform.position = cursorPos;
            
            // Change sprite on click if click sprite exists
            if (clickCursorSprite != null)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    customCursorImage.sprite = clickCursorSprite;
                }
                else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    customCursorImage.sprite = normalCursorSprite;
                }
            }
        }
    }

    public void SetInventoryOpen(bool isOpen)
    {
        isInventoryOpen = isOpen;
        UpdateCursorVisibility();
    }
    
    public void ShowSystemCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isSystemCursorVisible = true;
    }
    
    public void HideSystemCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isSystemCursorVisible = false;
    }
    
    public void ShowCustomCursor(bool show)
    {
        if (customCursorImage != null)
        {
            customCursorImage.gameObject.SetActive(show);
        }
        
        if (cursorCanvas != null)
        {
            cursorCanvas.enabled = show;
        }
    }
    
    private void UpdateCursorVisibility()
    {
        if (isInventoryOpen)
        {
            if (useCustomCursor)
            {
                ShowCustomCursor(true);
                HideSystemCursor();
            }
            else
            {
                ShowSystemCursor();
            }
        }
        else
        {
            if (useCustomCursor)
            {
                ShowCustomCursor(false);
            }
            HideSystemCursor();
        }
    }
    
    // Call this method when you want to force show the cursor (e.g., in menus)
    public void ForceShowCursor()
    {
        if (useCustomCursor)
        {
            ShowCustomCursor(true);
            HideSystemCursor();
        }
        else
        {
            ShowSystemCursor();
        }
    }
    
    // Call this method when you want to force hide the cursor (e.g., in gameplay)
    public void ForceHideCursor()
    {
        if (useCustomCursor)
        {
            ShowCustomCursor(false);
        }
        HideSystemCursor();
    }
    
    public void ToggleCursorMode()
    {
        useCustomCursor = !useCustomCursor;
        UpdateCursorVisibility();
    }
}
