using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GSheetManager : IGsheetManager
{
    private const string BASE_URL = "https://docs.google.com/spreadsheets/d";
    private string gSheetId;
    private string gid;
    private List<Dictionary<string, string>> data = new();
    private bool _isDownload;
    public bool IsDownload { get => _isDownload; }
    
    public GSheetManager(string gSheetId, string gid)
    {
        this.gSheetId = gSheetId;
        this.gid = gid;
        UniTask.Void(async () => { await LoadDataFromGsheet(); });
    }
    
    // 1. 정상적인 URL 을 만들어줘야 한다. => 함수
    private string CombineURL() => $"{BASE_URL}/{gSheetId}/export?format=csv&gid={gid}";
    
    // 2. 네트워크를 통해서 데이터를 가져와야한다. => 함수
    // 3. 받아온 JSON string 을 JObject 화 시켜서 private JObject 에 저장한다. => 함수
    private async UniTask LoadDataFromGsheet()
    {
        _isDownload = false;
        Debug.Log($"[GSheetManager] Loading sheet: {gSheetId}");
        UnityWebRequest request = UnityWebRequest.Get(CombineURL());
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await UniTask.Yield();
        }
        
        if (request.result != UnityWebRequest.Result.Success) 
            Debug.LogError($"[GSheetManager] Failed to load sheet: {request.error}");
        else
        {
            string csvString = request.downloadHandler.text;
            string[] lines = csvString.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            
            // Header
            string[] headers = lines[0].Split(",");
            
            // dataList
            for (int i = 1; i < lines.Length; i++)
            {
                var items = lines[i].Split(",");
                Dictionary<string, string> row = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length; j++)
                    row.Add(headers[j], items[j]);
                data.Add(row);
            }
        }
        Debug.Log("[GSheetManager] Finish to Load");
        _isDownload = true;
    }
    
    public List<Dictionary<string, string>> GetData() => data;
    public void ToScriptableObject(string fileName)
    {
        // ToDo. Implement
    }
}