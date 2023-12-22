using System;
using System.Collections;

namespace OTA.Coroutine
{
    public interface ICoroutine
    {
        void RunCoroutine(IEnumerator enumerator, Action onCoroutineComplete);
    }
}
