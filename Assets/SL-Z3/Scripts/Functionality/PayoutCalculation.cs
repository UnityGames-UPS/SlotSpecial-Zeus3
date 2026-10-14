using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PayoutCalculation : MonoBehaviour
{
  [SerializeField]
  private int x_Distance;
  [SerializeField]
  private float y_Distance;
  [SerializeField]
  private Material[] LineMats;

  [SerializeField]
  private TMP_Text SlotLines_Text;
  [SerializeField]
  private TMP_Text GreenSlotLines_Text;
  [SerializeField]
  private TMP_Text YellowSlotLines_Text;
  [SerializeField]
  private TMP_Text PurpleSlotLines_Text;
  [SerializeField]
  private TMP_Text RedSlotLines_Text;

  [SerializeField]
  private Transform LineContainerEnable;
  [SerializeField]
  private Transform LineContainerDisable;
  [SerializeField]
  private GameObject Line_Prefab;
  [SerializeField]
  private GameObject Line2_Prefab;
  internal List<int> DontDestroyLines = new List<int>();

  [SerializeField]
  private List<float> yStartpoints;

  [SerializeField]
  private Vector2 InitialLinePosition = new Vector2(-315, 100);

  [SerializeField]
  private List<GameObject> GreenWinLinesEnabled;
  [SerializeField]
  private List<GameObject> GreenWinLinesDisabled;

  [SerializeField]
  private List<GameObject> YellowWinLinesEnabled;
  [SerializeField]
  private List<GameObject> YellowWinLinesDisabled;

  [SerializeField]
  private List<GameObject> PurpleWinLinesEnabled;
  [SerializeField]
  private List<GameObject> PurpleWinLinesDisabled;

  [SerializeField]
  private List<GameObject> RedWinLinesEnabled;
  [SerializeField]
  private List<GameObject> RedWinLinesDisabled;

  [SerializeField]
  private Image GreenBarImage;
  [SerializeField]
  private Image YellowBarImage;
  [SerializeField]
  private Image PurpleBarImage;
  [SerializeField]
  private Image RedBarImage;
  [SerializeField] private SocketIOManager socketManager;
  [SerializeField] private SlotBehaviour slotBehaviour;
  private Coroutine LineCoroutine = null;

  //generate lines at runtime accordingly
  internal void GeneratePayoutLinesBackend(List<int> y_index, int Count, int matCount = 0)
  {
    GameObject MyLineObjEnable = null;
    GameObject MyLineObjDisable = null;
    if (y_index[0] == 1)
    {
      MyLineObjEnable = Instantiate(Line2_Prefab, LineContainerEnable);
      MyLineObjDisable = Instantiate(Line2_Prefab, LineContainerDisable);
    }
    else
    {
      MyLineObjEnable = Instantiate(Line_Prefab, LineContainerEnable);
      MyLineObjDisable = Instantiate(Line_Prefab, LineContainerDisable);
    }
    MyLineObjDisable.SetActive(false);
    MyLineObjEnable.SetActive(false);
    MyLineObjEnable.transform.localPosition = new Vector2(InitialLinePosition.x, InitialLinePosition.y);
    UILineRenderer MyLine = MyLineObjEnable.GetComponent<UILineRenderer>();
    MyLineObjDisable.transform.localPosition = new Vector2(InitialLinePosition.x, InitialLinePosition.y);
    UILineRenderer MyLine2 = MyLineObjDisable.GetComponent<UILineRenderer>();
    switch (matCount)
    {
      case 3:
        MyLine2.material = LineMats[0];
        MyLine.material = LineMats[1];
        GreenWinLinesEnabled.Add(MyLineObjEnable);
        GreenWinLinesDisabled.Add(MyLineObjDisable);
        break;
      case 4:
        MyLine2.material = LineMats[2];
        MyLine.material = LineMats[3];
        YellowWinLinesEnabled.Add(MyLineObjEnable);
        YellowWinLinesDisabled.Add(MyLineObjDisable);
        break;
      case 5:
        MyLine2.material = LineMats[4];
        MyLine.material = LineMats[5];
        PurpleWinLinesEnabled.Add(MyLineObjEnable);
        PurpleWinLinesDisabled.Add(MyLineObjDisable);
        break;
      case 6:
        MyLine2.material = LineMats[6];
        MyLine.material = LineMats[7];
        RedWinLinesEnabled.Add(MyLineObjEnable);
        RedWinLinesDisabled.Add(MyLineObjDisable);
        break;
    }
    for (int i = 0; i < Count; i++)
    {
      var points = new Vector2() { x = i * x_Distance, y = yStartpoints[i] - (y_index[i] * 137) };
      var pointlist = new List<Vector2>(MyLine.Points);
      pointlist.Add(points);
      MyLine.Points = pointlist.ToArray();
    }
    var temppoints = new Vector2() { x = (5 * x_Distance) + 120, y = yStartpoints[5] - (y_index[5] * 137) };
    var temppointlist = new List<Vector2>(MyLine.Points);
    temppointlist.Add(temppoints);
    MyLine.Points = temppointlist.ToArray();
    var newpointlist = new List<Vector2>(MyLine.Points);
    MyLine.Points = newpointlist.ToArray();
    MyLine2.Points = MyLine.Points;
  }

  internal void StartLinesRoutine()
  {
    if (LineCoroutine != null)
    {
      StopCoroutine(LineCoroutine);
      LineCoroutine = null;
    }
    LineCoroutine = StartCoroutine(LineRoutine());
  }

  private IEnumerator LineRoutine()
  {
    float Greenval = (float)GreenWinLinesEnabled.Count / socketManager.initFeat.numberOfLines;
    float Yellowval = (float)YellowWinLinesEnabled.Count / socketManager.initFeat.numberOfLines;
    float Purpleval = (float)PurpleWinLinesEnabled.Count / socketManager.initFeat.numberOfLines;
    float Redval = (float)RedWinLinesEnabled.Count / socketManager.initFeat.numberOfLines;
    if (GreenSlotLines_Text) GreenSlotLines_Text.text = GreenWinLinesEnabled.Count + "\n3" + "\nof" + "\na" + "\nkind";
    if (YellowSlotLines_Text) YellowSlotLines_Text.text = YellowWinLinesEnabled.Count + "\n4" + "\nof" + "\na" + "\nkind";
    if (PurpleSlotLines_Text) PurpleSlotLines_Text.text = PurpleWinLinesEnabled.Count + "\n5" + "\nof" + "\na" + "\nkind";
    if (RedSlotLines_Text) RedSlotLines_Text.text = RedWinLinesEnabled.Count + "\n6" + "\nof" + "\na" + "\nkind";
    if (SlotLines_Text) SlotLines_Text.gameObject.SetActive(false);

    if (Greenval > 0)
    {
      if (GreenSlotLines_Text) GreenSlotLines_Text.gameObject.SetActive(true);
      float initAmount = 0;
      DOTween.To(() => initAmount, (val) => initAmount = val, Greenval, 0.2f).OnUpdate(() =>
      {
        if (GreenBarImage) GreenBarImage.fillAmount = initAmount;
      });
      for (int i = 0; i < GreenWinLinesEnabled.Count; i++)
      {
        GreenWinLinesDisabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(0.1f);
        GreenWinLinesEnabled[i].SetActive(true);
      }
      // if (GreenSlotLines_Text) GreenSlotLines_Text.gameObject.SetActive(false);
    }
    if (Yellowval > 0)
    {
      ResetTexts();
      if (YellowSlotLines_Text) YellowSlotLines_Text.gameObject.SetActive(true);
      float initAmount = 0;
      DOTween.To(() => initAmount, (val) => initAmount = val, Yellowval + Greenval, 0.2f).OnUpdate(() =>
      {
        if (YellowBarImage) YellowBarImage.fillAmount = initAmount;
      });
      for (int i = 0; i < YellowWinLinesEnabled.Count; i++)
      {
        YellowWinLinesDisabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(0.1f);
        YellowWinLinesEnabled[i].SetActive(true);
      }
      // if (YellowSlotLines_Text) YellowSlotLines_Text.gameObject.SetActive(false);
    }
    if (Purpleval > 0)
    {
      ResetTexts();
      if (PurpleSlotLines_Text) PurpleSlotLines_Text.gameObject.SetActive(true);
      float initAmount = 0;
      DOTween.To(() => initAmount, (val) => initAmount = val, Purpleval + Greenval + Yellowval, 0.2f).OnUpdate(() =>
      {
        if (PurpleBarImage) PurpleBarImage.fillAmount = initAmount;
      });
      for (int i = 0; i < PurpleWinLinesEnabled.Count; i++)
      {
        PurpleWinLinesDisabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(0.1f);
        PurpleWinLinesEnabled[i].SetActive(true);
      }
      // if (PurpleSlotLines_Text) PurpleSlotLines_Text.gameObject.SetActive(false);
    }
    if (Redval > 0)
    {
      ResetTexts();
      if (RedSlotLines_Text) RedSlotLines_Text.gameObject.SetActive(true);
      float initAmount = 0;
      DOTween.To(() => initAmount, (val) => initAmount = val, Redval + Purpleval + Greenval + Yellowval, 0.2f).OnUpdate(() =>
      {
        if (RedBarImage) RedBarImage.fillAmount = initAmount;
      });
      for (int i = 0; i < RedWinLinesEnabled.Count; i++)
      {
        RedWinLinesDisabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(0.1f);
        RedWinLinesEnabled[i].SetActive(true);
      }
      // if (RedSlotLines_Text) RedSlotLines_Text.gameObject.SetActive(false);
    }

    if (slotBehaviour.IsFreeSpin || socketManager.resultData.features.freeSpin.isAdded || socketManager.resultData.features.freeSpin.isTriggered || slotBehaviour.IsAutoSpin)
    {
      yield return new WaitForSecondsRealtime(2f);
    }
    Debug.Log("Called");
    slotBehaviour.lineShowed = true;

    foreach (Transform child in LineContainerEnable)
    {
      child.gameObject.SetActive(false);
    }
    yield return new WaitForSecondsRealtime(1f);
    
    if (GreenSlotLines_Text) GreenSlotLines_Text.gameObject.SetActive(true);
    for (int i = 0; i < GreenWinLinesEnabled.Count; i++)
    {
      GreenWinLinesEnabled[i].SetActive(true);
      yield return new WaitForSecondsRealtime(0.3f);
      GreenWinLinesEnabled[i].SetActive(false);
    }
    if (YellowSlotLines_Text) YellowSlotLines_Text.gameObject.SetActive(true);
    if (GreenSlotLines_Text) GreenSlotLines_Text.gameObject.SetActive(false);
    for (int i = 0; i < YellowWinLinesEnabled.Count; i++)
    {
      YellowWinLinesEnabled[i].SetActive(true);
      yield return new WaitForSecondsRealtime(0.3f);
      YellowWinLinesEnabled[i].SetActive(false);
    }
    if (PurpleSlotLines_Text) PurpleSlotLines_Text.gameObject.SetActive(true);
    if (YellowSlotLines_Text) YellowSlotLines_Text.gameObject.SetActive(false);
    for (int i = 0; i < PurpleWinLinesEnabled.Count; i++)
    {
      PurpleWinLinesEnabled[i].SetActive(true);
      yield return new WaitForSecondsRealtime(0.3f);
      PurpleWinLinesEnabled[i].SetActive(false);
    }
    if (RedSlotLines_Text) RedSlotLines_Text.gameObject.SetActive(true);
    if (PurpleSlotLines_Text) PurpleSlotLines_Text.gameObject.SetActive(false);
    for (int i = 0; i < RedWinLinesEnabled.Count; i++)
    {
      RedWinLinesEnabled[i].SetActive(true);
      yield return new WaitForSecondsRealtime(0.3f);
      RedWinLinesEnabled[i].SetActive(false);
    }
    foreach (Transform child in LineContainerEnable)
    {
      child.gameObject.SetActive(false);
    }
    foreach (Transform child in LineContainerDisable)
    {
      child.gameObject.SetActive(false);
    }
    while (true)
    {
      if (GreenSlotLines_Text) GreenSlotLines_Text.gameObject.SetActive(true);
      if (RedSlotLines_Text) RedSlotLines_Text.gameObject.SetActive(false);
      for (int i = 0; i < GreenWinLinesEnabled.Count; i++)
      {
        GreenWinLinesEnabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        GreenWinLinesEnabled[i].SetActive(false);
      }
      if (YellowSlotLines_Text) YellowSlotLines_Text.gameObject.SetActive(true);
      if (GreenSlotLines_Text) GreenSlotLines_Text.gameObject.SetActive(false);
      for (int i = 0; i < YellowWinLinesEnabled.Count; i++)
      {
        YellowWinLinesEnabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        YellowWinLinesEnabled[i].SetActive(false);
      }
      if (PurpleSlotLines_Text) PurpleSlotLines_Text.gameObject.SetActive(true);
      if (YellowSlotLines_Text) YellowSlotLines_Text.gameObject.SetActive(false);
      for (int i = 0; i < PurpleWinLinesEnabled.Count; i++)
      {
        PurpleWinLinesEnabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        PurpleWinLinesEnabled[i].SetActive(false);
      }
      if (RedSlotLines_Text) RedSlotLines_Text.gameObject.SetActive(true);
      if (PurpleSlotLines_Text) PurpleSlotLines_Text.gameObject.SetActive(false);
      for (int i = 0; i < RedWinLinesEnabled.Count; i++)
      {
        RedWinLinesEnabled[i].SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        RedWinLinesEnabled[i].SetActive(false);
      }
    }
  }

  private void ResetTexts()
  {
    YellowSlotLines_Text.gameObject.SetActive(false);
    PurpleSlotLines_Text.gameObject.SetActive(false);
    GreenSlotLines_Text.gameObject.SetActive(false);
    RedSlotLines_Text.gameObject.SetActive(false);
  }

  //delete all lines
  internal void ResetLines()
  {
    if (LineCoroutine != null)
    {
      StopCoroutine(LineCoroutine);
      LineCoroutine = null;
    }
    foreach (Transform child in LineContainerDisable)
    {
      Destroy(child.gameObject);
    }
    foreach (Transform child in LineContainerEnable)
    {
      Destroy(child.gameObject);
    }
    GreenWinLinesEnabled.Clear();
    GreenWinLinesEnabled.TrimExcess();
    GreenWinLinesDisabled.Clear();
    GreenWinLinesDisabled.TrimExcess();
    YellowWinLinesEnabled.Clear();
    YellowWinLinesEnabled.TrimExcess();
    YellowWinLinesDisabled.Clear();
    YellowWinLinesDisabled.TrimExcess();
    PurpleWinLinesEnabled.Clear();
    PurpleWinLinesEnabled.TrimExcess();
    PurpleWinLinesDisabled.Clear();
    PurpleWinLinesDisabled.TrimExcess();
    RedWinLinesEnabled.Clear();
    RedWinLinesEnabled.TrimExcess();
    RedWinLinesDisabled.Clear();
    RedWinLinesDisabled.TrimExcess();
    if (RedSlotLines_Text) RedSlotLines_Text.gameObject.SetActive(false);
    if (GreenSlotLines_Text) GreenSlotLines_Text.gameObject.SetActive(false);
    if (YellowSlotLines_Text) YellowSlotLines_Text.gameObject.SetActive(false);
    if (PurpleSlotLines_Text) PurpleSlotLines_Text.gameObject.SetActive(false);
    if (SlotLines_Text) SlotLines_Text.gameObject.SetActive(true);
    if (GreenBarImage) GreenBarImage.fillAmount = 0;
    if (YellowBarImage) YellowBarImage.fillAmount = 0;
    if (PurpleBarImage) PurpleBarImage.fillAmount = 0;
    if (RedBarImage) RedBarImage.fillAmount = 0;
  }
}
