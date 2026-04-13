using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI nameText;

    public void Setup(SkillData skill)
    {
        cardImage.sprite = skill.icon;
        nameText.text = skill.description;
    }
}
