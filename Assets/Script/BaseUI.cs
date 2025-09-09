using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    #region Properties
    private bool _isWaitingDuration;

    public bool mustWaitDuration;
    public float showDelay;
    public float hideDelay;
    public float duration;
    #endregion

    #region Base Logic
    public async Task Show(float showDelayOverride = -1f, float durationOverride = 0f, float hideDelayOverride = -1f)
    {
        float delay = showDelayOverride > -1f ? showDelayOverride : showDelay;
        float dur = durationOverride > 0f ? durationOverride : duration;

        if (delay > 0f)
            await UniTask.Delay((int)(delay * 1000));

        gameObject.SetActive(true);

        if (dur > 0f)
        {
            _isWaitingDuration = true;

            await UniTask.WaitForSeconds(dur);

            _isWaitingDuration = false;

            Hide(hideDelayOverride);
        }
    }

    public async Task Hide(float delayOverride = -1f)
    {
        if (_isWaitingDuration && mustWaitDuration) return;

        float delay = delayOverride > -1f ? delayOverride : hideDelay;

        if (delay > 0f)
            await UniTask.Delay((int)(delay * 1000));

        gameObject.SetActive(false);
    }
    #endregion
}
