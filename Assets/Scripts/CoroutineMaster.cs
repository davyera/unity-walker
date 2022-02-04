using System.Collections;
using UnityEngine;

public class CoroutineMaster : MonoBehaviour
{
    private static CoroutineMaster singleton;

    public static void StartChildCoroutine(IEnumerator coroutine)
    {
        if (singleton == null)
            singleton = new GameObject("[Coroutine Master]").AddComponent<CoroutineMaster>();

        singleton.StartCoroutine(coroutine);
    }
}
