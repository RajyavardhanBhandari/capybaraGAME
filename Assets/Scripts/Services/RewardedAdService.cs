using System;
using UnityEngine;

namespace CapybaraGame.Services
{
    /// <summary>
    /// Rewarded-ad boundary. Phase 12 replaces the provider implementation with the
    /// selected mobile ad SDK. No production code should grant a reward before the
    /// provider confirms completion.
    /// </summary>
    public sealed class RewardedAdService
    {
        public bool IsReady
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public void ShowRewarded(Action<bool> completed)
        {
#if UNITY_EDITOR
            Debug.Log("RewardedAdService: editor simulation completed.");
            completed?.Invoke(true);
#else
            Debug.Log("RewardedAdService: no production ad provider configured yet.");
            completed?.Invoke(false);
#endif
        }
    }
}
