using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxScripting : MonoBehaviour
{
  internal bool isAnim = false;
  internal bool isBonus = false;
  internal bool isScatter = false;
  [SerializeField]
  private Image BoxBg_Image;
  [SerializeField]
  private GameObject BoxBg_Object;

  internal void SetBG(Sprite bg)
  {
    if (isAnim)
    {
      if (BoxBg_Image) BoxBg_Image.sprite = bg;
      if (BoxBg_Object) BoxBg_Object.SetActive(true);
    }
  }

  internal void ResetBG()
  {
    if (BoxBg_Object) BoxBg_Object.SetActive(false);
  }
}
