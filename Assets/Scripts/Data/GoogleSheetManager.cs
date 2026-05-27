using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;


//   <사용 시 유의사항>
//   구글 스프레드 시트
// 구글 스프레드 시트에 관련 확장 프로그램(JsonParser.gs) 이 있어야 함
// 확장 프로그램의 권한을 `액세스 권한이 있는 사용자를 모든 사용자`로 변경

//   유니티
// Newtonsoft Json 패키지 설치 되어있어야 함
// Edit - Project Settings - Player - Other Settings - Api Compatibillity Level을 `.NET Framework`로 바꿔야 함 (에디터 상에서만 직렬화되는 버그를 제거)
// 이 스크립트(GoogleSheetManager) 포함 된 게임 오브젝트이 씬에 존재해야 함 -> 우크릭으로 FetchGoogleSheet 실행 가능, 구글 시트에서 데이터를 불러와서 JSON으로 저장, C# 클래스 코드 생성, SO 생성까지 한 번에 진행

// https://goranitv.tistory.com/29 에 세팅, 사용법, 예시 등 포함됨


//   <현재 게임 오브젝트 세팅>
// 임시 Google Sheet Url : https://script.google.com/macros/s/AKfycbznpYMVTubag55WoXohdYq7jiSkawdorLsDIzoZ0bCDZPL6hkWjVaE2uEdMaZEW0o-PMQ/exec
// 임시 Use Sheets To SO : CharacterData, ItemData
// 임시 Use Sheets To Dic : (없음, 현재 기능 구현 안됨)
// 임시 Generate Folder Path : (자유롭게)
// 임시 Sheet SO : (Use Sheets To SO 만큼 빈칸 생성 필요, SO는 자동으로 생성되어 들어감)


//   <TODO>
//   필수 기능 TODO
// TODO : 구글 시트에서 데이터를 불러오는 부분(런타임 사용)과, JSON 파일로 저장하는 부분을 분리
// TODO : SO를 사용할지 딕셔너리를 사용할지 모르니, 둘 중에 선택할 수 있게 하기
// V TODO : SO 1개가 아니라 여러 개가 필요할 수도 있으니, SO도 시트마다 생성하는 것으로 바꾸기

//   편의 기능 TODO
// TODO : 현재는 인스펙터에서 우클릭으로 생성하는데, 이를 윈도우에서 생성하도록 바꾸기
// V TODO : 구글 시트에서 타입을 판별해서 가져오기
// TODO : EnumType이 생길 수 있으니 JTokenType 대신 자체적으로 타입 판별하는 Enum 만들기


//   <제안>
// SO 내 데이터를 List 화 시킬지, 여러 SO 를 만들지 등에 대해 각자 다르게 진행될 것으로 예상되서, 라이브러리 형태로 부탁 드리긴 한 것 입니다.
// 먼저 예제로 공유 주신 내용을 기본으로 깔고 필요한 사람은 어떤 함수를 이용해서 하면 된다 라는 가이드 형태도 좋을 것 같습니다 :)

// 1. 순수 C# 스크립트로?? 사용할 때는 특정 모노에서 불러서 URL과 시트 이름 부르면 JSON 형태로 받아오기??
// 2. 싱글톤 되도록 없애기 -> 서비스 로케이터로 대체



// 작성자 : 한성우
// https://goranitv.tistory.com/29 기반 수정
public class GoogleSheetManager : MonoBehaviour
{
    [Tooltip("True 면 Google Sheet에서 데이터를 불러옴, False면 로컬 JSON 파일에서 불러옴")]
    [SerializeField] private bool isUseGoogleSheet = true;

    // 외부에서 불러올 수도 있을 것 같은 것은 프로퍼티화
    [Tooltip("구글 스프레드 시트 URL")]
    [field: SerializeField] public string GoogleSheetUrl { get; private set; }

    [Tooltip("SO(Scriptable Object) 파일로 생성할 시트 이름")]
    [SerializeField] private List<string> useSheetsToSO = new List<string>();

    [Tooltip("런타임 딕셔너리로 사용할 시트 이름 (Addressables로 로드)")]
    [SerializeField] private List<string> useSheetsToDic = new List<string>();
    /*
    [Tooltip("사용하는 시트 리스트로 추가")]
    [SerializeField] private List<string> useSheets = new List<string>();
    public IReadOnlyList<string> UseSheets => useSheets;
    */

    // [Space]
    [Tooltip("예시: `/GenerateGoogleSheet`")]
    [field: SerializeField] public string GenerateFolderPath { get; private set; }

    // 자동으로 연결되기에 유저가 넣어줄 필요가 없음
    [Tooltip("생성된 개별 시트 SO들을 자동으로 관리하는 리스트")]
    public List<ScriptableObject> sheetSO = new List<ScriptableObject>();


    // 경로 관련 프로퍼티, 람다식으로 호출 시 매번 최신값으로 계산하여 결합
    private string ClassPath => $"{Application.dataPath}{GenerateFolderPath}/GoogleSheetClass.cs";
    private string GetJsonPath(string sheetName) => $"{Application.dataPath}{GenerateFolderPath}/{sheetName}.json";
    private string GetSOPath(string sheetName) => $"Assets{GenerateFolderPath}/{sheetName}SO.asset";


    // private string[] availSheetArray; // useSheets가 한 줄의 string 일 경우 필요, 현재는 기능이 변경되어 사용 안함
    private string json;
    // private bool refeshTrigger;

    static GoogleSheetManager instance;

    // 메모리에 한 번 로드한 딕셔너리를 재사용하기 위한 캐싱 보관함
    private static Dictionary<Type, object> cachedDictionaries = new Dictionary<Type, object>();


    // 싱글톤 패턴을 응용한 SO 접근 메서드, 제네릭으로 반환 타입을 지정하여 원하는 클래스의 리스트에 접근 가능
    public static T SO<T>() where T : ScriptableObject
    {
        var manager = GetInstance();
        if (manager == null) return null;

        foreach (var so in manager.sheetSO)
        {
            if (so is T)
                return so as T;
        }

        Debug.LogWarning($"{typeof(T).Name} 타입의 SO를 찾을 수 없습니다. 시트 이름과 일치하는지 확인해주세요.");
        return null;
    }


    // 유니티 에디터에서 해당 오브젝트 있다면, 이 오브젝트의 하이라키 창에서 우클릭으로 FetchGoogleSheet 함수 실행
    // 구글 시트에서 데이터를 불러와서 JSON으로 저장, C# 클래스 코드 생성, SO 생성까지 한 번에 진행
#if UNITY_EDITOR
    [ContextMenu("FetchGoogleSheet")]
    private async void FetchGoogleSheet()
    {
        //Init
        // availSheetArray = UseSheets.Split('/');

        // 설정에 따라 구글 시트에서 데이터를 불러오거나, 로컬 JSON 파일에서 데이터를 불러옴
        if (isUseGoogleSheet)
        {
            // Debug.Log($"[GoogleSheetManager] Loading from google sheet..");
            json = await LoadDataGoogleSheet(GoogleSheetUrl);
        }
        else
        {
            // Debug.Log($"[GoogleSheetManager] Loading from local json..");
            json = LoadDataLocalJson();
        }

        // 데이터가 null 이면 중단
        if (json == null) return;


        bool isAnyFileSaved = false;
        JObject jsonObject = JObject.Parse(json);

        // 통짜 JSON을 시트별로 분리하여 별도의 파일로 저장
        foreach (var sheet in jsonObject)
        {
            string sheetName = sheet.Key;
            if (!IsExistAvailSheets(sheetName)) continue;

            // 해당 시트의 데이터 만 별도의 문자열로 분리
            string sheetJson = sheet.Value.ToString();

            // 시트 이름으로 JSON 파일 개별 저장 (예: CharacterData.json)
            bool isSaved = SaveFileOrSkip(GetJsonPath(sheetName), sheetJson);
            if (isSaved) isAnyFileSaved = true;
        }

        // C# 클래스 생성
        string allClassCode = GenerateCSharpClass(json);
        bool isClassSaved = SaveFileOrSkip(ClassPath, allClassCode);

        // 파일에 변화 생길 시 유니티 에디터 새로고침
        if (isAnyFileSaved || isClassSaved)
        {
            UnityEditor.EditorPrefs.SetBool("NeedCreateSO", true);
            UnityEditor.AssetDatabase.Refresh();
        }
        // 변화가 없다면 SO 생성 / 갱신
        else
        {
            CreateGoogleSheetSO();
            // Debug.Log($"[GoogleSheetManager] Fetch done.");
        }
    }


    /*
    // 가져온 json 데이터와 생성할 C# 클래스 코드를 파일로 저장
    bool isJsonSaved = SaveFileOrSkip(JsonPath, json);
        string allClassCode = GenerateCSharpClass(json);
        bool isClassSaved = SaveFileOrSkip(ClassPath, allClassCode);

        // 파일에 변화 생길 시 유니티 에디터 새로고침
        if (isJsonSaved || isClassSaved)
        {
            refeshTrigger = true;
            UnityEditor.AssetDatabase.Refresh();
        }
        // 변화가 없다면 SO 생성 / 갱신
        else
        {
            CreateGoogleSheetSO();
            Debug.Log($"Fetch done.");
        }
    }
    */


    // 구글에서 비동기 방식으로 데이터 받아옴 -> UniTask로 변환 가능할 것으로 보임
    private async Task<string> LoadDataGoogleSheet(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                byte[] dataBytes = await client.GetByteArrayAsync(url);
                return Encoding.UTF8.GetString(dataBytes);
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"[GoogleSheetManager] Request error: {e.Message}");
                return null;
            }
        }
    }


    // 분할된 로컬 Json 파일들을 읽어서, 구글 시트에서 받아왔을 때와 동일한 형태의 거대한 JObject로 재조립하여 반환
    private string LoadDataLocalJson()
    {
        JObject masterJson = new JObject();
        bool hasAnyData = false;

        foreach (var sheetName in useSheetsToSO)
        {
            string path = GetJsonPath(sheetName);
            if (File.Exists(path))
            {
                // 개별 저장된 파일을 읽어서 masterJson의 키(시트 이름)에 다시 연결
                masterJson[sheetName] = JObject.Parse(File.ReadAllText(path));
                hasAnyData = true;
            }
            else
            {
                // Debug.Log($"[GoogleSheetManager] LoadDataLocalJson : File not exist.\n{path}");
            }
        }

        return hasAnyData ? masterJson.ToString() : null;
    }
    /*
    // 로컬 환경의 Json 파일에서 데이터 받아옴, LoadDataGoogleSheet 사용시 사용 안함
    private string LoadDataLocalJson()
    {
        if (File.Exists(JsonPath))
        {
            return File.ReadAllText(JsonPath);
        }

        Debug.Log($"File not exist.\n{JsonPath}");
        return null;
    }
    */


    // 파일 저장, 이미 존재하는 파일이지만 내용이 동일하면 저장하지 않음(최적화 용도)
    private bool SaveFileOrSkip(string path, string contents)
    {
        string directoryPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (File.Exists(path) && File.ReadAllText(path).Equals(contents))
        {
            return false;
        }
            

        File.WriteAllText(path, contents);
        return true;
    }

    // 호출 시 useSheetsToSO 리스트의 시트 이름과 일치하는지 확인, 일치하면 클래스에 포함, 일치하지 않으면 무시
    private bool IsExistAvailSheets(string sheetName)
    {
        return useSheetsToSO.Contains(sheetName);
    }

    private string GenerateCSharpClass(string jsonInput)
    {

        // Debug.Log("[GoogleSheetManager] GenerateCSharpClass 받은 데이터: " + jsonInput);
        JObject jsonObject = JObject.Parse(jsonInput);
        StringBuilder classCode = new();


        classCode.AppendLine("using System;\nusing System.Collections.Generic;\nusing UnityEngine;\n");

        // 개별 데이터 클래스와 해당 데이터를 담을 전용 SO 클래스를 세트로 생성
        foreach (var sheet in jsonObject)
        {
            string className = sheet.Key; // 예: CharacterData
            if (!IsExistAvailSheets(className)) continue;

            JObject sheetContent = (JObject)sheet.Value;
            JObject typesObj = (JObject)sheetContent["types"]; // 타입 정보 데이터

            // 해당 시트 전용 ScriptableObject 뼈대 생성 (예: CharacterDataSO)
            classCode.AppendLine($"/// <summary>You must approach through `GoogleSheetManager.SO<{className}SO>()`</summary>");
            classCode.AppendLine($"public class {className}SO : ScriptableObject\n{{");
            // 기존 {className}List 라는 이름 대신 일관성 있게 'dataList' 라는 이름으로 통일
            classCode.AppendLine($"\tpublic List<{className}> dataList;\n}}\n");

            // 실제 데이터를 담는 클래스 뼈대 생성 (예: CharacterData)
            classCode.AppendLine($"[Serializable]\npublic class {className}\n{{");



            /*
            // SO의 뼈대 생성, 각 시트마다 리스트 형태로 클래스의 멤버로 추가
            classCode.AppendLine("using System;\nusing System.Collections.Generic;\nusing UnityEngine;\n");
            classCode.AppendLine("/// <summary>You must approach through `GoogleSheetManager.SO<GoogleSheetSO>()`</summary>");
            classCode.AppendLine("public class GoogleSheetSO : ScriptableObject\n{");

            foreach (var sheet in jsonObject)
            {
                // 인스펙터의 useSheets 리스트에 포함된 시트만 클래스에 포함, 포함되지 않은 시트는 무시
                string className = sheet.Key;
                if (!IsExistAvailSheets(className))
                {
                    continue;
                }


                classCode.AppendLine($"\tpublic List<{className}> {className}List;");
            }
            classCode.AppendLine("}\n");

            // 개별 데이터 클래스 뼈대 생성, 각 시트마다 클래스 하나씩 생성
            foreach (var sheet in jsonObject)
            {
                string className = sheet.Key;
                if (!IsExistAvailSheets(className)) continue;


                JObject sheetContent = (JObject)sheet.Value;
                JObject typesObj = (JObject)sheetContent["types"]; // 6번째 줄 데이터 (타입 정보)


                classCode.AppendLine($"[Serializable]\npublic class {className}\n{{");
                */


            // 구글 시트에 명시된 타입들을 그대로 C# 변수로 선언
            foreach (var typeProperty in typesObj.Properties())
            {
                string propertyName = typeProperty.Name;
                string propertyType = typeProperty.Value.ToString();

                classCode.AppendLine($"\tpublic {propertyType} {propertyName};");
            }

            classCode.AppendLine("}\n");
        }

        return classCode.ToString();
    }

    // JSON의 타입을 C#의 타입 문자열로 매핑해주는 헬퍼 함수
    private string GetCSharpType(JTokenType jsonType)
    {
        switch (jsonType)
        {
            case JTokenType.Integer:
                return "int";
            case JTokenType.Float:
                return "float";
            case JTokenType.Boolean:
                return "bool";
            default:
                return "string";
        }
    }

    // Reflection을 활용하여 런타임에 SO 인스턴스 생성 및 JSON 데이터로 필드 값 설정
    // 클래스 코드 생성 시 useSheets 리스트에 포함된 시트만 클래스에 포함되도록 했으므로, 해당 시트만 SO의 멤버로 추가됨
    private bool CreateGoogleSheetSO()
    {
        /*
        if (Type.GetType("GoogleSheetSO") == null)
            return false;

        googleSheetSO = ScriptableObject.CreateInstance("GoogleSheetSO");
        */

        JObject jsonObject = JObject.Parse(json);

        // 새로 SO들을 생성할 것이므로 기존 매니저의 리스트를 비움
        sheetSO.Clear();


        try
        {
            foreach (var sheet in jsonObject)
            {
                string className = sheet.Key;
                if (!IsExistAvailSheets(className))
                {
                    continue;
                }


                // 생성해야 할 SO의 클래스 이름 (예: CharacterDataSO)
                string soClassName = $"{className}SO";
                Type soType = Type.GetType(soClassName); // 1. 타입을 미리 변수에 담아줍니다.

                if (soType == null)
                {
                    Debug.LogWarning($"[CreateGoogleSheetSO] {soClassName} 타입을 찾을 수 없음 (컴파일 중일 수 있음)");
                    continue;
                }

                // Type 객체를 직접 넘겨주어 개별 SO 인스턴스 생성
                ScriptableObject sheetSO = ScriptableObject.CreateInstance(soType);


                // 클래스 타입과 리스트 타입을 Reflection으로 생성, JSON 데이터의 각 아이템을 클래스 인스턴스로 만들어 리스트에 추가, 완성된 리스트를 SO의 해당 멤버 필드에 설정
                Type classType = Type.GetType(className);
                Type listType = typeof(List<>).MakeGenericType(classType);
                IList listInst = (IList)Activator.CreateInstance(listType);

                JObject sheetContent = (JObject)sheet.Value;
                JArray items = (JArray)sheetContent["data"];


                foreach (var item in items)
                {
                    object classInst = Activator.CreateInstance(classType);

                    foreach (var property in ((JObject)item).Properties())
                    {
                        FieldInfo fieldInfo = classType.GetField(property.Name);

                        if (fieldInfo != null)
                        {
                            //Newtonsoft.Json의 ToObject 활용 직렬화
                            object value = property.Value.ToObject(fieldInfo.FieldType);
                            fieldInfo.SetValue(classInst, value);
                        }

                    }

                    listInst.Add(classInst);
                }

                // 생성된 리스트를 개별 SO의 'dataList' 필드에 할당
                sheetSO.GetType().GetField("dataList").SetValue(sheetSO, listInst);

                // 유니티 에셋으로 개별 저장 (예: Assets/.../CharacterDataSO.asset)
                string soPath = GetSOPath(className);
                UnityEditor.AssetDatabase.CreateAsset(sheetSO, soPath);

                // 런타임에서 SO<T>() 함수로 찾을 수 있게 매니저에 등록
                this.sheetSO.Add(sheetSO);

                /*
                // 만들어진 리스트를 SO의 해당 필드에 할당
                googleSheetSO.GetType().GetField($"{className}List").SetValue(googleSheetSO, listInst);
                */
            }
        }

        catch (Exception e)
        {
            Debug.LogError($"[CreateGoogleSheetSO] error: {e.Message}");
            return false;
        }

        // Debug.Log("[CreateGoogleSheetSO] 시트별 분리 완료");
        UnityEditor.AssetDatabase.SaveAssets();
        return true;
    }


    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // 우리가 구글 시트 갱신을 눌러서 컴파일이 일어난 상황인지 확인
        if (UnityEditor.EditorPrefs.GetBool("NeedCreateSO", false))
        {
            // 작업이 끝났으므로 플래그를 다시 꺼주기 (중복 실행 방지)
            UnityEditor.EditorPrefs.SetBool("NeedCreateSO", false);

            // 씬에 존재하는 매니저를 찾아 SO 생성을 마저 진행
            GoogleSheetManager manager = GetInstance();
            if (manager != null)
            {
                bool isCompleted = manager.CreateGoogleSheetSO();
                if (isCompleted)
                {
                    // Debug.Log("[GoogleSheetManager] 구글 시트 파싱 및 SO 생성 완료");
                }
            }
        }
    }

    /*
    // 인스펙터나 스크립트가 리로드 될 때 호출되는 유니티 콜백 함수
    private void OnValidate()
    {
        if (refeshTrigger)
        {
            bool isCompleted = CreateGoogleSheetSO();
            if (isCompleted)
            {
                refeshTrigger = false;
                Debug.Log($"Fetch done.");
            }
        }
    }
    */
#endif

    // 씬에 존재하는 매니저 객체를 찾아 반환하는 싱글톤 접근 함수
    public static GoogleSheetManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<GoogleSheetManager>();
        }
        return instance;
    }
}