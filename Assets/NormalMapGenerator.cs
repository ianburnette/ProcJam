using UnityEngine;

public static class NormalMapGenerator
{
    public static void CreateNormalMap(ref Texture2D source, float strength) {
        strength = Mathf.Clamp(strength, 0.0F, 1.0F);
        var normalTexture = new Texture2D (source.width, source.height, TextureFormat.ARGB32, true);

        for (var y=0; y<normalTexture.height; y++) 
        {
            for (var x=0; x<normalTexture.width; x++) 
            {
                var xLeft = source.GetPixel(x-1,y).grayscale*strength;
                var xRight = source.GetPixel(x+1,y).grayscale*strength;
                var yUp = source.GetPixel(x,y-1).grayscale*strength;
                var yDown = source.GetPixel(x,y+1).grayscale*strength;
                var xDelta = ((xLeft-xRight)+1)*0.5f;
                var yDelta = ((yUp-yDown)+1)*0.5f;
                normalTexture.SetPixel(x,y,new Color(xDelta,yDelta,1.0f,yDelta));
            }
        }
        source = normalTexture;
    }
    
    
}
