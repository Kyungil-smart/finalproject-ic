using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GSheetManager : IGsheetManager
{
    private const string BASE_URL = "https://docs.google.com/spreadsheets/d";
    private const int MAX_ATTEMPTS = 3;
    private const int TIMEOUT_SECONDS = 15;
    private const int BASE_RETRY_DELAY_MS = 500;
    
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
        try
        {
            for (int attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
            {
                using UnityWebRequest request = UnityWebRequest.Get(CombineURL());
                request.timeout = TIMEOUT_SECONDS;
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await UniTask.Yield();
                // 성공
                if (request.result == UnityWebRequest.Result.Success)
                {
                    ParseCsv(request.downloadHandler.text);
                    Debug.Log($"[GSheetManager] Success (attempt {attempt}) => {data.Count} rows");
                    return; // finally 에서 _isDownload = true
                }
                // 재시도 불가(4xx): 시트 공유설정/ID 오류 → 즉시 중단
                if (request.result == UnityWebRequest.Result.ProtocolError
                    && request.responseCode >= 400 && request.responseCode < 500)
                {
                    Debug.LogError($"[GSheetManager] Non-retriable HTTP {request.responseCode}: {request.error}. 시트 공유설정/ID 확인 필요.");
                    return;
                }
                // 재시도 가능(네트워크/타임아웃/5xx/429)
                Debug.LogWarning($"[GSheetManager] Attempt {attempt}/{MAX_ATTEMPTS} failed: {request.error} (code {request.responseCode})");
                if (attempt < MAX_ATTEMPTS)
                    await UniTask.Delay(BASE_RETRY_DELAY_MS * (int)Mathf.Pow(2, attempt - 1));
            }
            Debug.LogError($"[GSheetManager] All {MAX_ATTEMPTS} attempts failed. sheet={gSheetId}, gid={gid}");
        }
        finally
        {
            Debug.Log("[GSheetManager] Finish to Load");
            _isDownload = true;
        }
    }
    
    private void ParseCsv(string csvString)
    {
        data.Clear();
        string[] lines = csvString.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return;
        string[] headers = lines[0].Split(",");
        for (int i = 1; i < lines.Length; i++)
        {
            var items = lines[i].Split(",");
            var row = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
                row.Add(headers[j], j < items.Length ? items[j] : string.Empty); // 열 수 방어
            data.Add(row);
        }
    }
    
    public List<Dictionary<string, string>> GetData() => data;
    public void ToScriptableObject(string fileName)
    {
        // ToDo. Implement
    }
}