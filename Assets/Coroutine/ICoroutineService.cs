using System;
using System.Collections;

namespace App.Game.Services
{
    public interface ICoroutineService
    {
        void RunCoroutine(IEnumerator enumerator, Action onCoroutineComplete);
        void RemoveCoroutine(string coroutineID);
    }
}
