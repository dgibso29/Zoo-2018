using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Zoo.UI
{
    public class ModifiableWindow : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        RectTransform window;

        /// <summary>
        /// Minimum width of window. Based on widght as set in editor.
        /// </summary>
        float minWidth;

        /// <summary>
        /// Minimum height of window. Based on height as set in editor.
        /// </summary>
        float minHeight;

        /// <summary>
        /// Offset from corner edge within which the cursor will still be considered at that corner.
        /// </summary>
        float cornerOffset = 20;

        /// <summary>
        /// X & Y coordinates of the top right corner of the window.
        /// </summary>
        Vector2 topRightCorner;

        /// <summary>
        /// X & Y coordinates of the bottom right corner of the window.
        /// </summary>
        Vector2 bottomRightCorner;

        /// <summary>
        /// X & Y coordinates of the bottom left corner of the window.
        /// </summary>
        Vector2 bottomLeftCorner;

        /// <summary>
        /// X & Y coordinates of the top left corner of the window.
        /// </summary>
        Vector2 topLeftCorner;

        /// <summary>
        /// Position of mouse when clicked.
        /// </summary>
        Vector3 mousePositionWhenClicked;

        bool mouseIsOverWindow = false;

        bool isResizing = false;

        bool isMoving = false;

        public bool canBeResized = true;

        public bool canBeMoved = true;

        /// <summary>
        /// Current edge of window mouse is over. Ex. TopRight, Right, BottomRight, Bottom, None, etc
        /// </summary>
        string currentMouseEdge = "None";

        void Start()
        {
            InitialiseWindow();
        }

        // Update is called once per frame
        void Update()
        {
            // If the mouse enters the window
            if (mouseIsOverWindow && !isResizing && !isMoving)
            {
                SetMouseEdge();
                if (Input.GetMouseButtonDown(0))
                {
                    if (currentMouseEdge != "None" && !isResizing && canBeResized && !isMoving)
                    {
                        mousePositionWhenClicked = Input.mousePosition;
                        StartCoroutine(ResizeWindow());
                    }
                    if(currentMouseEdge != "None" && !isMoving && canBeMoved && !isMoving)
                    {
                        mousePositionWhenClicked = Input.mousePosition;
                        StartCoroutine(MoveWindow());
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseIsOverWindow = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseIsOverWindow = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }

        void InitialiseWindow()
        {
            window = GetComponent<RectTransform>();
            minWidth = window.rect.width;
            minHeight = window.rect.height;
            GetCorners();
        }

        void GetCorners()
        {
            Vector3[] corners = new Vector3[4];
            window.GetWorldCorners(corners);
            topRightCorner = new Vector2(corners[2].x, corners[2].y);
            bottomRightCorner = new Vector2(corners[3].x, corners[3].y);
            bottomLeftCorner = new Vector2(corners[0].x, corners[0].y);
            topLeftCorner = new Vector2(corners[1].x, corners[1].y);
        }

        /// <summary>
        /// Set mouseEdge string based on mouse position over window.
        /// </summary>
        void SetMouseEdge()
        {
            Vector3 mousePosition = Input.mousePosition;
            // Check mouse position against window parameters            
            // Check Bottom Left/ Left/ Top Left First
            if (mousePosition.x >= bottomLeftCorner.x && mousePosition.x <= bottomLeftCorner.x + cornerOffset)
            {
                // Check Y
                // If bottom left
                if(mousePosition.y >= bottomLeftCorner.y && mousePosition.y <= bottomLeftCorner.y + cornerOffset)
                {
                    currentMouseEdge = "BottomLeft";
                }
                // If top left
                else if (mousePosition.y <= topLeftCorner.y && mousePosition.y >= topLeftCorner.y - cornerOffset)
                {
                    currentMouseEdge = "TopLeft";
                }
                // Otherwise, it's the left edge
                else
                {
                    currentMouseEdge = "Left";
                }
            }
            // Check Bottom Right/ Right/ Top Right
            else if (mousePosition.x <= topRightCorner.x && mousePosition.x >= topRightCorner.x - cornerOffset)
            {
                // Check Y
                // If bottom right
                if (mousePosition.y >= bottomRightCorner.y && mousePosition.y <= bottomRightCorner.y + cornerOffset)
                {
                    currentMouseEdge = "BottomRight";
                }
                // If top right
                else if (mousePosition.y <= topRightCorner.y && mousePosition.y >= topRightCorner.y - cornerOffset)
                {
                    currentMouseEdge = "TopRight";
                }
                // Otherwise, it's the right edge
                else
                {
                    currentMouseEdge = "Right";
                }
            }
            // Check Top/Bottom/None
            else
            {
                // If top
                if(mousePosition.y <= topRightCorner.y && mousePosition.y >= topRightCorner.y - cornerOffset)
                {
                    currentMouseEdge = "Top";
                }
                // If Bottom
                else if (mousePosition.y >= bottomLeftCorner.y && mousePosition.y <= bottomLeftCorner.y + cornerOffset)
                {
                    currentMouseEdge = "Bottom";
                }
            }
        }

        ///// <summary>
        ///// Change the mouse cursor when at edges of window
        ///// </summary>
        ///// <returns></returns>
        //IEnumerator ChangeCursor()
        //{
        //    Cursor.SetCursor()
        //}

        IEnumerator MoveWindow()
        {
            isMoving = true;
            float startX = window.position.x;
            float startY = window.position.y;
            Vector3 mousePosition;
            while (Input.GetMouseButton(0))
            {
                mousePosition = Input.mousePosition;

                // TO DO: FIGURE OUT HOW TO STOP AT THE EDGES OF THE SCREEN

                switch (currentMouseEdge)
                {
                    case "Top":
                        {
                            float newX = Mathf.Clamp((startX + (mousePosition.x - mousePositionWhenClicked.x)), -50, 1600);
                            float newY = Mathf.Clamp((startY + (mousePosition.y - mousePositionWhenClicked.y)), 50, 950);
                            window.position = new Vector3(newX, newY);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            isMoving = false;
        }

        IEnumerator ResizeWindow()
        {
            isResizing = true;
            float startHeight = window.rect.height;
            float startWidth = window.rect.width;
            Vector3 mousePosition;
            while (Input.GetMouseButton(0))
            {
                mousePosition = Input.mousePosition;
                // Resize window based on current corner and current mouse position.

                // TO DO: FIGURE OUT HOW TO STOP AT THE EDGES OF THE SCREEN

                switch (currentMouseEdge)
                {
                    case "BottomRight":
                        {
                            float newHeight = Mathf.Clamp((startHeight - (mousePosition.y - mousePositionWhenClicked.y)), minHeight, 1000);
                            float newWidth = Mathf.Clamp((startWidth + (mousePosition.x - mousePositionWhenClicked.x)), minWidth, 1400);
                            window.sizeDelta = new Vector2(newWidth, newHeight);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            GetCorners();
            isResizing = false;
        }

    }
}
