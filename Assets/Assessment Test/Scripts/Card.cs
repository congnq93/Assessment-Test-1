using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image bgImage;
    [SerializeField] Image iconImage;
    [SerializeField] Image highlightImage;
    [SerializeField] AnimationCurve highlightMatchCurve;
    [SerializeField] AnimationCurve fadeOutMatchCurve;
    [SerializeField] AnimationCurve highlightMismatchCurve;

    bool isFlipped;
    bool interactable = true;

    public Action CallbackOnFlip { get; set; }
    public Sprite IconSprite => iconImage.sprite;

    public void SetIcon(Sprite icon)
    {
        iconImage.sprite = icon;
    }

    public void SetFlip(bool flip)
    {
        isFlipped = flip;
        bgImage.color = flip ? Color.white : Color.gray;
        iconImage.enabled = flip;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable || isFlipped)
            return;

        SetFlip(true);
        CallbackOnFlip.Invoke();
    }

    public IEnumerator HighlightMatchCoroutine()
    {
        interactable = false;
        float duration = 1f;
        float time = 0;
        while (time < duration)
        {
            float t = time / duration;
            float curveValue = highlightMatchCurve.Evaluate(t);
            highlightImage.color = Color.Lerp(Color.clear, Color.green, curveValue);

            float alpha = Mathf.Lerp(0, 1, fadeOutMatchCurve.Evaluate(t));
            bgImage.color = new Color(1, 1, 1, alpha);
            iconImage.color = new Color(1, 1, 1, alpha);

            time += Time.deltaTime;
            yield return null;
        }
        highlightImage.color = Color.clear;
        bgImage.color = Color.clear;
        iconImage.color = Color.clear;
    }

    public IEnumerator HighlightMismatchCoroutine()
    {
        interactable = false;
        float duration = .5f;
        float time = 0;
        while (time < duration)
        {
            float t = time / duration;
            float curveValue = highlightMismatchCurve.Evaluate(t);
            highlightImage.color = Color.Lerp(Color.clear, Color.red, curveValue);

            time += Time.deltaTime;
            yield return null;
        }
        highlightImage.color = Color.clear;

        SetFlip(false);
        interactable = true;
    }
}
