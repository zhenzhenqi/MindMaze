using UnityEngine;
using UnityEngine.UI;

public class DrawingScript : MonoBehaviour
{
    public RawImage drawingArea;  // UI element displaying the render texture
    public RenderTexture renderTexture;  // The render texture to draw on

    public Material paperMaterial;
    public Material brushMaterial;

    public int brushSize = 5; // Initial brush size in pixels

    private bool isDrawing = false;
    private RectTransform drawingAreaRectTransform;

    void Start()
    {
        drawingAreaRectTransform = drawingArea.GetComponent<RectTransform>();

        // Initialize RenderTexture
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(1024, 1024, 24);  // Adjust size if necessary
        }
        renderTexture.Create();
        drawingArea.texture = renderTexture;  // Assign the render texture to the RawImage
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (isDrawing && Input.GetMouseButton(0))
        {

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingAreaRectTransform, Input.mousePosition, null, out localPoint);

            // Map local point to RenderTexture coordinates
            Vector2 textureCoord = new Vector2(
                (localPoint.x + drawingAreaRectTransform.rect.width * 0.5f) / drawingAreaRectTransform.rect.width * renderTexture.width,
                (localPoint.y + drawingAreaRectTransform.rect.height * 0.5f) / drawingAreaRectTransform.rect.height * renderTexture.height
            );

            // Set RenderTexture as active so we can draw on it
            RenderTexture.active = renderTexture;

            // Make sure to use the correct matrix to draw in the RenderTexture
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);

            // Apply brush material for drawing without texture
            if (brushMaterial != null)
            {
                brushMaterial.SetPass(0);
                GL.Begin(GL.QUADS);

                // Calculate brush quad vertices based on converted texture coordinates and brush size
                float halfBrushSize = brushSize / 2f;
                Vector3 topLeft = new Vector3(textureCoord.x - halfBrushSize, textureCoord.y - halfBrushSize, 0);
                Vector3 topRight = new Vector3(textureCoord.x + halfBrushSize, textureCoord.y - halfBrushSize, 0);
                Vector3 bottomRight = new Vector3(textureCoord.x + halfBrushSize, textureCoord.y + halfBrushSize, 0);
                Vector3 bottomLeft = new Vector3(textureCoord.x - halfBrushSize, textureCoord.y + halfBrushSize, 0);

                // Draw the quad for the brush stroke
                GL.Vertex3(topLeft.x, topLeft.y, topLeft.z);
                GL.Vertex3(bottomLeft.x, bottomLeft.y, bottomLeft.z);
                GL.Vertex3(bottomRight.x, bottomRight.y, bottomRight.z);
                GL.Vertex3(topRight.x, topRight.y, topRight.z);

                GL.End();
            }

            GL.PopMatrix();

            // Clear the active RenderTexture when done
            RenderTexture.active = null;
        }
    }

    // Debugging function to check if the RenderTexture is actually being used and updated
    void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, 256, 256), renderTexture, ScaleMode.ScaleToFit, false, 1);
    }
}