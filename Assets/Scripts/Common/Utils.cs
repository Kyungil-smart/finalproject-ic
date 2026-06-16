using System;
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
        public static bool isDevelopment = true;
    }

    /// <summary>
    /// 지정한 조건이 참이 될 때까지 대기합니다. 제한 시간을 초과하면 예외를 던집니다.
    /// </summary>
    /// <param name="condition">검사할 조건 (람다식)</param>
    /// <param name="timeoutSeconds">제한 시간 (초 단위, 기본값 3초)</param>
    /// <exception cref="TimeoutException">제한 시간 내에 조건을 만족하지 못했을 때 발생</exception>
    public static class TaskAsync
    {
        public static async UniTask WaitUntilOrThrowAsync(Func<bool> condition, float timeoutSeconds = 3f)
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
    
    
}
