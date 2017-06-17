﻿using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class GridPlacement : Singleton<GridPlacement>
    {
        public SpriteData CurrentSprite; // Initialized by the sprite menu

        [SerializeField]
        private GridObject previewObject;

        private Vector2? previousMousePosition;
        private bool? deletionLayer; // Functional if true, decorative if false

        private void Update()
        {
            // Calculate sprite coordinates for the current mouse position
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(previousMousePosition == null)
                previousMousePosition = mousePosition;
            int spriteX = 0, spriteY = 0;

            // Interpolate between previous and current mouse position
            for(float i = 0.25f; i <= 1; i += 0.25f)
            {
                spriteX = Mathf.RoundToInt(Mathf.Lerp(previousMousePosition.Value.x, mousePosition.x, i) - (float)CurrentSprite.Width / 2);
                spriteY = Mathf.RoundToInt(Mathf.Lerp(previousMousePosition.Value.y, mousePosition.y, i) - (float)CurrentSprite.Height / 2);

                if(Input.GetMouseButton(1))
                {
                    int mouseX = Mathf.RoundToInt(Mathf.Lerp(previousMousePosition.Value.x, mousePosition.x, i) - 0.5f);
                    int mouseY = Mathf.RoundToInt(Mathf.Lerp(previousMousePosition.Value.y, mousePosition.y, i) - 0.5f);

                    // Set deletion layer if not set, prioritizing the functional layer
                    if(deletionLayer == null)
                    {
                        if(GridManager.Instance.ContainsGridObject(true, mouseX, mouseY))
                            deletionLayer = true;
                        else if(GridManager.Instance.ContainsGridObject(false, mouseX, mouseY))
                            deletionLayer = false;
                    }

                    // Remove existing grid object based on deletion layer
                    if(deletionLayer != null)
                        if(GridManager.Instance.ContainsGridObject(deletionLayer.Value, mouseX, mouseY))
                            GridManager.Instance.RemoveGridObject(deletionLayer.Value, mouseX, mouseY);
                }
                else if(Input.GetMouseButton(0) && CurrentSprite.HoldToPlace && GridManager.Instance.CanAddGridObject(CurrentSprite, spriteX, spriteY))
                {
                    // Place new grid object (if hold-to-place)
                    GridManager.Instance.AddGridObject(CurrentSprite, spriteX, spriteY);
                }
            }

            // Update preview object
            if(CurrentSprite.Sprite != previewObject.Data.Sprite)
                previewObject.SetSprite(CurrentSprite);
            previewObject.SetPosition(spriteX, spriteY);
            previewObject.gameObject.SetActive(GridManager.Instance.CanAddGridObject(CurrentSprite, spriteX, spriteY));

            // Place new grid object (if not hold-to-place)
            if(Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1) && GridManager.Instance.CanAddGridObject(CurrentSprite, spriteX, spriteY))
                GridManager.Instance.AddGridObject(CurrentSprite, spriteX, spriteY);

            // Remove deletion layer
            if(Input.GetMouseButtonUp(1))
                deletionLayer = null;

            // Store mouse position
            previousMousePosition = mousePosition;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Prevents off-screen mouse interpolation
            previousMousePosition = null;
        }
    }
}