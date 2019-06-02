using Godot;
using System.Threading.Tasks;

interface Colorful {
    void SetColor(Color color);
    void ResetColor();
    Task ShortlySetColor(Color color, float seconds = 0.66F);
}