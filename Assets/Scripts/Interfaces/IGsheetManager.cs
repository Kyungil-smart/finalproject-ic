using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public interface IGsheetManager
{
    public List<Dictionary<string, string>> GetData(); // 병희가 이야기 한거
    public void ToScriptableObject(string fileName);
    public bool IsDownload { get; }
}