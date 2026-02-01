using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootManager : MonoBehaviour
{
    public List<CanvasGroup> logInImages;
    public GameObject bg;
    public GameObject eventSystem;
    private bool isClickToStart = false;



    private IEnumerator Start()
    {
        // 淡入、展示、淡出时长
        float fadeInDuration = 0.6f;
        float displayDuration = 1.2f;
        float fadeOutDuration = 0.6f;
        // 顺序展示每个Logo
        foreach (var c in logInImages)
        {
            c.alpha = 0f;
            c.interactable = false;
        }
        CanvasGroup cg;
        Sequence seq;
        for (int i = 0; i < logInImages.Count - 1; i++)
        {
            cg = logInImages[i];
            seq = DOTween.Sequence()
                .AppendCallback(() => cg.gameObject.SetActive(true))
                .Append(cg.DOFade(1f, fadeInDuration))
                .AppendInterval(displayDuration)
                .Append(cg.DOFade(0f, fadeOutDuration))
                .AppendCallback(() => cg.gameObject.SetActive(false));
            yield return seq.WaitForCompletion();
        }
        // 最后一个Logo，等待玩家点击进入游戏
        cg = logInImages[^1];
        seq = DOTween.Sequence()
            .AppendCallback(() => cg.gameObject.SetActive(true))
            .Append(cg.DOFade(1f, fadeInDuration))
            .AppendCallback(() => cg.interactable = true);
        yield return seq.WaitForCompletion();
        while (!isClickToStart)
            yield return null;
        yield return SceneManager.LoadSceneAsync("City", LoadSceneMode.Additive);
        cg.gameObject.SetActive(false);
        bg.SetActive(false);
    }
    public void OnClickToStart()
    {
        Destroy(eventSystem); // 防止多个eventSystem
        isClickToStart = true;
    }
}
