using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;


namespace Utils
{
    public static class Layer
    {
        public static bool Compare(LayerMask myLayerMask, int targetLayer)
        {
            return ((1 << myLayerMask) & targetLayer) != 0;
        }
    }
    
    public static class JsonHandler
    {
        public static JObject ResourceFileLoader(string fileName)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(fileName);
            if (textAsset == null)
            {
                Debug.LogError($"Not found JSON file: Resources/{fileName}");
                return null;
            }
            return JObject.Parse(textAsset.text);
        }

        public static JObject LoadFromString(string jsonString) => JObject.Parse(jsonString);
    }
    
    public static class Environment
    {
        public static bool isDevelopment = false;
    }

    /// <summary>
    /// 지정한 조건이 참이 될 때까지 대기합니다. 제한 시간을 초과하면 예외를 던집니다.
    /// </summary>
    /// <param name="condition">검사할 조건 (람다식)</param>
    /// <param name="timeoutSeconds">제한 시간 (초 단위, 기본값 3초)</param>
    /// <exception cref="TimeoutException">제한 시간 내에 조건을 만족하지 못했을 때 발생</exception>
    public static class TaskAsync
    {
        public static async UniTask WaitUntilOrThrowAsync(Func<bool> condition, float timeoutSeconds = 5f)
        {
            // 1. 시간 기반 취소 토큰 소스 생성
            using (var cts = new CancellationTokenSource())
            {
                // 지정된 초 이후에 토큰이 취소되도록 설정
                cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                try
                {
                    // 2. 조건 만족 시까지 대기 (취소 토큰 연결)
                    await UniTask.WaitUntil(condition, cancellationToken: cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // 3. 시간이 초과되어 토큰이 취소되면 이 예외가 발생함 -> TimeoutException으로 전환하여 throw
                    throw new TimeoutException($"[TaskUtil] {timeoutSeconds}초 동안 조건을 만족하지 못해 대기가 중단되었습니다.");
                }
            }
        }
    }

    public static class NumberExtractor
    {
        public static int[] GetUniqueRandomNumbers(int min, int max, int count)
        {
            // 1. 1부터 100까지의 숫자가 담긴 배열 생성
            int range = max - min + 1;
            int[] numbers = new int[range];
            for (int i = 0; i < range; i++)
            {
                numbers[i] = min + i;
            }

            // 2. 필요한 개수(count)만큼만 셔플 진행
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, range);
            
                // 스왑(Swap) 진행
                int temp = numbers[i];
                numbers[i] = numbers[randomIndex];
                numbers[randomIndex] = temp;

                result[i] = numbers[i];
            }
            return result;
        }
    }

    public static class DayCheck
    {
        public static bool IsDaytime()
        {
            // 1. 현재 시스템 시간의 '시(Hour)' 정보를 가져옴
            int currentHour = DateTime.Now.Hour;

            // 2. 6시 이상 19시 미만인지 조건 검증
            return currentHour >= 6 && currentHour < 19;
        }
    }
    
    public static class SaveSerializer
    {
        private static readonly JsonSerializerSettings _settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
        };

        public static string Serialize<T>(T data) => JsonConvert.SerializeObject(data, _settings);
        public static T Deserialize<T>(string json)
            => string.IsNullOrEmpty(json) ? default : JsonConvert.DeserializeObject<T>(json, _settings);
    }
    
    public static class SaveFileIO
    {
        private static string PathOf(string fileName)
            => Path.Combine(Application.persistentDataPath, fileName);

        public static void Write(string fileName, string json)
            => File.WriteAllText(PathOf(fileName), json);

        public static string Read(string fileName)
        {
            var p = PathOf(fileName);
            return File.Exists(p) ? File.ReadAllText(p) : null;
        }

        public static bool Exists(string fileName) => File.Exists(PathOf(fileName));
        public static void Delete(string fileName) { if (Exists(fileName)) File.Delete(PathOf(fileName)); }
    }
}
