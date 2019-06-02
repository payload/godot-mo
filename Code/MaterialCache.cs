using Godot;
using System.Collections.Generic;

static class MaterialCache {
    static Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public static Material FromColor(Color color) {
        var key = color.ToHtml();

        if (Materials.ContainsKey(key)) {
            return Materials[key];
        } else {
            var mat = new SpatialMaterial();
            mat.SetAlbedo(color);
            Materials[key] = mat;
            return mat;
        }
    }
}