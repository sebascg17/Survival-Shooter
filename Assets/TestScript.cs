using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    public Image imageToFill; // ссылка на компонент Image, который нужно залить цветом
    public float nValue; // переменная N, значение которой будет определять заполнение картинки

    void Update()
    {
        // ограничиваем значение переменной N от 0 до 100
        nValue = Mathf.Clamp(nValue, 0f, 100f);

        // вычисляем процент заполнения картинки
        float fillPercent = nValue / 100f;

        // устанавливаем заполнение картинки в соответствии с процентом
        imageToFill.fillAmount = fillPercent;
    }
}
