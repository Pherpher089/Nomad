﻿namespace MalbersAnimations
{
    [System.Serializable]
    [UnityEngine.CreateAssetMenu(menuName = "Malbers Animations/ID/Stat", fileName = "New Stat ID", order = -1000)]
    public class StatID : IDs
    {
        #region CalculateID
#if UNITY_EDITOR
        private void Reset() => GetID();

        [UnityEngine.ContextMenu("Get ID")]
        private void GetID() => FindID<StatID>();
#endif
        #endregion
    }
}

