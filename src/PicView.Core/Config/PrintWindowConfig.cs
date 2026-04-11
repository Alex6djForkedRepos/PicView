using System.Text.Json;
using System.Text.Json.Serialization;
using PicView.Core.Config.ConfigFileManagement;

namespace PicView.Core.Config;

[JsonSourceGenerationOptions(AllowTrailingCommas = true, WriteIndented = true)]
[JsonSerializable(typeof(PrintWindowConfig.PrintWindowProperties))]
internal partial class PrintWindowGenerationContext : JsonSerializerContext;

public class PrintWindowConfig() : ConfigFile("PrintWindow.json")
{
    public PrintWindowProperties? PrintProperties { get; private set; }

    public async Task LoadAsync()
    {
        CorrectPath ??= ConfigFileManager.ResolveDefaultConfigPath(this);
        try
        {
            if (File.Exists(CorrectPath))
            {
                var jsonString = await File.ReadAllTextAsync(CorrectPath).ConfigureAwait(false);
                if (JsonSerializer.Deserialize(
                        jsonString, typeof(PrintWindowProperties), PrintWindowGenerationContext.Default) is PrintWindowProperties settings)
                {
                    PrintProperties = settings;
                }
                else
                {
                    PrintProperties = new PrintWindowProperties();
                }
            }
            else
            {
                PrintProperties = new PrintWindowProperties();
            }
        }
        catch
        {
            PrintProperties = new PrintWindowProperties();
        }
    }

    public async Task SaveAsync()
    {
        CorrectPath = await ConfigFileManager.SaveConfigFileAndReturnPathAsync(this,
            CorrectPath, PrintProperties, typeof(PrintWindowProperties), PrintWindowGenerationContext.Default);
    }

    public class PrintWindowProperties
    {
        public string? PrinterName { get; set; }
        public string? PaperSize { get; set; }
        public int? Orientation { get; set; }
        public int? ScaleMode { get; set; }
        public int? ColorMode { get; set; }
        public int? Copies { get; set; }
        public int? MarginTop { get; set; }
        public int? MarginBottom { get; set; }
        public int? MarginLeft { get; set; }
        public int? MarginRight { get; set; }
    }
}
