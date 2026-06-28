using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private Sprite[] bgImgs;
    [SerializeField][Range(0.1f, 3f)] private float interval;

    private bool _isLoop;
    private SpriteRenderer _spriteRenderer;

    private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();
    
    private void OnEnable() => _isLoop = true;
    
    private void OnDisable() => _isLoop = false;
    
    private void Start()
    {
        UniTask.Void(async () =>
        {
            await UniTask.WaitUntil(() => ServiceLocater.Get<IMinigameManager>() != null);
            while (_isLoop)
            {
                if (!ServiceLocater.Get<IMinigameManager>().GameStart)
                {
                    await UniTask.WaitForSeconds(interval);
                    continue;
                }
                _spriteRenderer.sprite = bgImgs[0];
                await UniTask.WaitForSeconds(interval);
                _spriteRenderer.sprite = bgImgs[1];
                await UniTask.WaitForSeconds(interval);
                _spriteRenderer.sprite = bgImgs[0];
                await UniTask.WaitForSeconds(interval);
                _spriteRenderer.sprite = bgImgs[1];
                await UniTask.WaitForSeconds(interval);
                _spriteRenderer.sprite = bgImgs[2];
                await UniTask.WaitForSeconds(interval);
                _spriteRenderer.sprite = bgImgs[1];
                await UniTask.WaitForSeconds(interval);
            }
        });
    }
}