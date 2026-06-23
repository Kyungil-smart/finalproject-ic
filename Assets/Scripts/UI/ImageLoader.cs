using Cysharp.Threading.Tasks;
using DataDispatcher;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(Image))]
public class ImageLoader : MonoBehaviour
{
     [SerializeField] private int imageId = -1;         

    private Image _image;
    private AsyncOperationHandle<Sprite> _handle;
    private bool _hasHandle;

    public int ImageId
    {
        set { imageId = value; LoadImage(); }     
    }

    private void Awake() => _image = GetComponent<Image>();

    private void OnEnable() => LoadImage();

    private void OnDisable() => ReleaseHandle();
    private void OnDestroy()  => ReleaseHandle();

    private void LoadImage() => UniTask.Void(async () => await ChangeImageAsync());

    private async UniTask ChangeImageAsync()
    {
        if (imageId < 0) return;                                   // -1: 아무것도 안 함
        if (_image == null) _image = GetComponent<Image>();
        if (imageId == 0)                                          // 0: 비우기
        {
            ReleaseHandle();
            _image.sprite = null;
            return;
        }
        
        ReleaseHandle();                                           // 이전 이미지 핸들 정리
        var handle = Addressables.LoadAssetAsync<Sprite>(imageId.ToString());
        _handle = handle;
        _hasHandle = true;

        try
        {
            var sprite = await handle.ToUniTask(
                cancellationToken: this.GetCancellationTokenOnDestroy());
            if (this == null || _image == null)
            {
                Debug.LogWarning($"[ImageLoader] Loading ... Failed ; Not Image");
                return; // 로드 중 파괴 가드
            }
            _image.sprite = sprite;
        }
        catch (System.OperationCanceledException) { /* 파괴/취소 시 무시 */ }
    }

    private void ReleaseHandle()
    {
        if (!_hasHandle) return;
        Addressables.Release(_handle);
        _hasHandle = false;
    }
}