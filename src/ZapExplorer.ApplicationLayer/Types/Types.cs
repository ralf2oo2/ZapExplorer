using Avalonia.Platform.Storage;

namespace ZapExplorer.ApplicationLayer.Types;

public class Types
{
    public static FilePickerFileType Zap { get; } = new("Zap archives")
    {
        Patterns = ["*.zap"],
        MimeTypes = ["application/octet-stream"],
    };
}