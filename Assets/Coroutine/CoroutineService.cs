using App.Game.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace App.System.Utils
{
    public class CoroutineService : MonoBehaviour, ICoroutineService
    {
        readonly Dictionary<string, IEnumerator> coroutineDict = new Dictionary<string, IEnumerator>();

        public void Init()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void RunCoroutine(IEnumerator enumerator, Action onCoroutineComplete)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(enumerator.GetHashCode());
            stringBuilder.Append(Guid.NewGuid().ToString());

            var coroutineID = stringBuilder.ToString();

            var coroutine = DoCoroutine(enumerator, coroutineID, onCoroutineComplete);
            coroutineDict.Add(coroutineID, coroutine);

            StartCoroutine(coroutine);
        }

        public void RemoveCoroutine(string coroutineID)
        {
            StopCoroutine(coroutineDict[coroutineID]);
            coroutineDict.Remove(coroutineID);
        }

        IEnumerator DoCoroutine(IEnumerator coroutine, string coroutineID, Action onCoroutineComplete)
        {
            yield return coroutine;
            onCoroutineComplete?.Invoke();
        }
    }
}