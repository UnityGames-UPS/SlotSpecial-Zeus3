using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
  [Header("Sprites")]
  [SerializeField]
  private Sprite[] myImages;  //images taken initially
  [SerializeField]
  private Sprite[] FSImages;

  [Header("Slot Images")]
  [SerializeField]
  private List<SlotImage> images;     //class to store total images
  [SerializeField]
  private List<SlotImage> FSimages;   //class to store FS total images
  [SerializeField]
  private List<SlotImage> placeHolderImages;

  [Header("Slots Transforms")]
  [SerializeField]
  private Transform[] Slot_Transform;
  [SerializeField]
  private Transform[] FSSlot_Transform;
  [SerializeField]
  private double[] stopIndices;

  private Dictionary<int, string> y_string = new Dictionary<int, string>();

  [Header("Animated Sprites")]
  [SerializeField]
  private Sprite[] Ship_Sprite;
  [SerializeField]
  private Sprite[] Coin_Sprite;
  [SerializeField]
  private Sprite[] Wild_Sprite;
  [SerializeField]
  private Sprite[] Feature_Sprite;

  [Header("FS Animated Sprites")]
  [SerializeField]
  private Sprite[] ShipFS_Sprite;
  [SerializeField]
  private Sprite[] CoinFS_Sprite;
  [SerializeField]
  private Sprite[] WildFS_Sprite;
  [SerializeField]
  private Sprite[] FeatureFS_Sprite;

  [Header("Miscellaneous UI")]
  [SerializeField]
  private TMP_Text Balance_text;
  [SerializeField]
  private TMP_Text TotalBet_text;
  [SerializeField]
  private TMP_Text LineBet_text;
  [SerializeField]
  private TMP_Text TotalWin_text;
  [SerializeField]
  private TMP_Text Info_text;

  [Header("Audio Management")]
  [SerializeField]
  private AudioController audioController;

  [SerializeField]
  private UIManager uiManager;

  [Header("Free Spin Popup")]
  [SerializeField]
  private GameObject FSPanel_Object;
  [SerializeField]
  private TMP_Text FS_Text;
  [SerializeField]
  private Button FS_Button;
  [SerializeField]
  private TMP_Text Buttontext_Object;
  private Tween FS_Tween = null;

  [Header("AutoSpin Buttons")]
  [SerializeField]
  private Button AutoSpin_Button;
  [SerializeField]
  private Button AutoSpinStop_Button;

  [Header("Wild Reel")]
  [SerializeField]
  private GameObject[] Wildreels_Object;
  [SerializeField]
  private GameObject[] Borderreels_Object;
  [SerializeField]
  private GameObject[] FSWildreels_Object;
  [SerializeField]
  private GameObject[] FSBorderreels_Object;

  [SerializeField]
  private Button Turbo_Button;
  [SerializeField]
  internal Button StopSpin_Button;

  [SerializeField]
  private Sprite[] TurboToggleSprites;

  private bool StopSpinToggle;
  private float SpinDelay = 0.2f;
  private bool IsTurboOn;
  internal bool WasAutoSpinOn;

  [SerializeField]
  int tweenHeight = 2700;  //calculate the height at which tweening is done

  [SerializeField]
  private PayoutCalculation PayCalculator;

  private List<Tweener> alltweens = new List<Tweener>();

  private Tweener WinTween = null;

  [SerializeField]
  internal List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

  [SerializeField]
  private SocketIOManager SocketManager;

  private Coroutine tweenroutine;
  private Coroutine Freespintweenroutine;
  private Coroutine AutoSpinRoutine = null;

  internal bool IsFreeSpin = false;
  private bool IsSpinning = false;
  private bool CheckSpinAudio = false;
  internal bool CheckPopups = false;
  internal bool IsAutoSpin = false;

  private int BetCounter = 0;
  private int FreeSpinCounter = 0;
  private double currentBalance = 0;
  private double currentTotalBet = 0;
  [SerializeField]
  private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
  private int numberOfSlots = 6;          //number of columns


  private void Start()
  {
    IsAutoSpin = false;
    if (FS_Button) FS_Button.onClick.RemoveAllListeners();
    if (FS_Button) FS_Button.onClick.AddListener(FlipSlot);

    if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
    if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);


    if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
    if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(() => { WasAutoSpinOn = false; StopAutoSpin(); });

    if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
    if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); audioController.PlayButtonAudio(); });

    if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
    if (Turbo_Button) Turbo_Button.onClick.AddListener(() => { TurboToggle(); audioController.PlayButtonAudio(); });
  }

  void TurboToggle()
  {
    if (IsTurboOn)
    {
      IsTurboOn = false;
      Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
      Turbo_Button.image.sprite = TurboToggleSprites[0];
      Turbo_Button.image.color = new Color(0.86f, 0.86f, 0.86f, 1);
    }
    else
    {
      IsTurboOn = true;
      Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
      Turbo_Button.image.color = new Color(1, 1, 1, 1);
    }
  }

  private void CompareBalance()
  {

    if (currentBalance < currentTotalBet)
    {
      uiManager.LowBalPopup();
    }
  }

  #region LinesCalculation
  //Fetch Lines from backend
  internal void FetchLines(string LineVal, int count)
  {
    y_string.Add(count + 1, LineVal);
  }

  #endregion




  internal void ChangeBet(bool IncDec)
  {
    if (audioController) audioController.PlayButtonAudio();
    if (IncDec)
    {
      if (BetCounter < SocketManager.initialData.betMultiplier.Count - 1)
      {
        BetCounter++;
      }
      else
      {
        BetCounter = 0;
      }
    }
    else
    {
      if (BetCounter > 0)
      {
        BetCounter--;
      }
      else
      {
        BetCounter = SocketManager.initialData.betMultiplier.Count - 1;
      }
    }
    if (LineBet_text) LineBet_text.text = SocketManager.initialData.betMultiplier[BetCounter];
    if (TotalBet_text) TotalBet_text.text = (double.Parse(SocketManager.initialData.betMultiplier[BetCounter]) * SocketManager.initialData.baseBet).ToString();
    currentTotalBet = double.Parse(SocketManager.initialData.betMultiplier[BetCounter]) * SocketManager.initialData.baseBet;
    if (Info_text) Info_text.text = SocketManager.initialData.baseBet + " base bet x " + SocketManager.initialData.betMultiplier[BetCounter] + " bet multiplier = " + currentTotalBet + " total bet";

  }

  #region InitialFunctions
  internal void shuffleInitialMatrix(bool initialShuffle)
  {
    if (initialShuffle)
    {
      for (int i = 0; i < images.Count; i++)
      {
        for (int j = 0; j < 5; j++)
        {
          int randomIndex = UnityEngine.Random.Range(0, 11);
          images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().sprite = myImages[randomIndex];
          if (randomIndex == 5)
          {
            images[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(true);
          }
          else
          {
            images[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(false);
          }
          if (randomIndex == 0 || randomIndex == 10)
          {
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-10.205f, -39.6f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(10.205f, 39.6f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = false;
            images[i].slotImages[j].transform.SetAsLastSibling();
          }
          else if (randomIndex == 11)
          {
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-47.5f, -55.555f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(73.5f, 55.555f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
            images[i].slotImages[j].transform.SetAsLastSibling();
          }
          else
          {
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
          }
        }
      }
    }
    else
    {
      for (int i = 0; i < placeHolderImages.Count; i++)
      {
        for (int j = 0; j < 2; j++)
        {
          int randomIndex = UnityEngine.Random.Range(0, 11);
          placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().sprite = myImages[randomIndex];
          if (randomIndex == 5)
          {
            placeHolderImages[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(true);
          }
          else
          {
            placeHolderImages[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(false);
          }
          if (randomIndex == 0 || randomIndex == 10)
          {
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-10.205f, -39.6f);
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(10.205f, 39.6f);
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = false;
            placeHolderImages[i].slotImages[j].transform.SetAsLastSibling();
          }
          else if (randomIndex == 11)
          {
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-47.5f, -55.555f);
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(73.5f, 55.555f);
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
            placeHolderImages[i].slotImages[j].transform.SetAsLastSibling();
          }
          else
          {
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            placeHolderImages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
          }
        }
      }
    }
  }

  internal void shuffleInitialMatrixFS()
  {
    for (int i = 0; i < FSimages.Count; i++)
    {
      for (int j = 0; j < 5; j++)
      {
        int randomIndex = UnityEngine.Random.Range(0, 11);
        FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().sprite = FSImages[randomIndex];
        if (randomIndex == 5)
        {
          FSimages[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
          FSimages[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(false);
        }
        if (randomIndex == 0 || randomIndex == 10)
        {
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-10.205f, -39.6f);
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(10.205f, 39.6f);
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = false;
          FSimages[i].slotImages[j].transform.SetAsLastSibling();
        }
        else if (randomIndex == 11)
        {
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-47.5f, -55.555f);
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(73.5f, 55.555f);
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
          FSimages[i].slotImages[j].transform.SetAsLastSibling();
        }
        else
        {
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
          FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
        }
      }
    }
  }

  internal void SetInitialUI()
  {
    BetCounter = 0;
    if (LineBet_text) LineBet_text.text = SocketManager.initialData.betMultiplier[BetCounter].ToString();
    if (TotalBet_text) TotalBet_text.text = (double.Parse(SocketManager.initialData.betMultiplier[BetCounter]) * SocketManager.initialData.baseBet).ToString();
    if (TotalWin_text) TotalWin_text.text = "0.000";
    if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
    currentBalance = SocketManager.playerdata.Balance;
    currentTotalBet = double.Parse(SocketManager.initialData.betMultiplier[BetCounter]) * SocketManager.initialData.baseBet;
    if (Info_text) Info_text.text = SocketManager.initialData.baseBet + " base bet x " + SocketManager.initialData.betMultiplier[BetCounter] + " bet multiplier = " + currentTotalBet + " total bet";
    CompareBalance();
    uiManager.InitialiseUIData(SocketManager.initUIData.paylines);
  }
  #endregion

  private void OnApplicationFocus(bool focus)
  {
    audioController.CheckFocusFunction(focus, CheckSpinAudio);
  }

  //function to populate animation sprites accordingly
  private void PopulateAnimationSprites(ImageAnimation animScript, int val)
  {
    animScript.textureArray.Clear();
    animScript.textureArray.TrimExcess();
    animScript.AnimationSpeed = 25f;
    switch (val)
    {
      case 2:
        for (int i = 0; i < Ship_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Ship_Sprite[i]);
        }
        animScript.AnimationSpeed = 110f;
        break;
      case 5:
        for (int i = 0; i < Coin_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Coin_Sprite[i]);
        }
        animScript.AnimationSpeed = 25f;
        break;
      case 10:
        for (int i = 0; i < Wild_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Wild_Sprite[i]);
        }
        animScript.AnimationSpeed = 31f;
        break;
      case 11:
        for (int i = 0; i < Feature_Sprite.Length; i++)
        {
          animScript.textureArray.Add(Feature_Sprite[i]);
        }
        animScript.AnimationSpeed = 20f;
        break;
    }
  }

  private void PopulateFSAnimationSprites(ImageAnimation animScript, int val)
  {
    animScript.textureArray.Clear();
    animScript.textureArray.TrimExcess();
    animScript.AnimationSpeed = 25f;
    switch (val)
    {
      case 2:
        for (int i = 0; i < ShipFS_Sprite.Length; i++)
        {
          animScript.textureArray.Add(ShipFS_Sprite[i]);
        }
        animScript.AnimationSpeed = 40f;
        break;
      case 5:
        for (int i = 0; i < CoinFS_Sprite.Length; i++)
        {
          animScript.textureArray.Add(CoinFS_Sprite[i]);
        }
        animScript.AnimationSpeed = 20f;
        break;
      case 10:
        for (int i = 0; i < WildFS_Sprite.Length; i++)
        {
          animScript.textureArray.Add(WildFS_Sprite[i]);
        }
        animScript.AnimationSpeed = 31f;
        break;
      case 11:
        for (int i = 0; i < FeatureFS_Sprite.Length; i++)
        {
          animScript.textureArray.Add(FeatureFS_Sprite[i]);
        }
        animScript.AnimationSpeed = 20f;
        break;
    }
  }

  #region Autospin
  private void AutoSpin()
  {
    if (!IsAutoSpin)
    {

      IsAutoSpin = true;
      if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
      if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

      if (AutoSpinRoutine != null)
      {
        StopCoroutine(AutoSpinRoutine);
        AutoSpinRoutine = null;
      }
      AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

    }
  }

  private void StopAutoSpin()
  {
    if (IsAutoSpin)
    {
      IsAutoSpin = false;

      if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
      if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
      StartCoroutine(StopAutoSpinCoroutine());
    }
  }



  private IEnumerator AutoSpinCoroutine()
  {
    while (IsAutoSpin)
    {
      StartSlots(IsAutoSpin);
      yield return tweenroutine;
      yield return new WaitForSeconds(SpinDelay);
    }
    WasAutoSpinOn = false;
  }

  private IEnumerator StopAutoSpinCoroutine()
  {
    yield return new WaitUntil(() => !IsSpinning);
    ToggleButtonGrp(true);
    if (AutoSpinRoutine != null || tweenroutine != null)
    {
      StopCoroutine(AutoSpinRoutine);
      StopCoroutine(tweenroutine);
      tweenroutine = null;
      AutoSpinRoutine = null;
      StopCoroutine(StopAutoSpinCoroutine());
    }
  }
  #endregion

  #region SlotSpin

  internal void StartFreeSpinRoutine()
  {
    Freespintweenroutine = StartCoroutine(FreeSpinRoutine());
  }

  private IEnumerator FreeSpinRoutine()
  {
    while (FreeSpinCounter > 0)
    {
      yield return new WaitUntil(() => !IsSpinning);
      StartSlots();
      //yield return new WaitForSeconds(2);
      yield return new WaitForSeconds(SpinDelay);

      yield return new WaitUntil(() => !IsSpinning);
      if (tweenroutine != null)
      {
        StopCoroutine(tweenroutine);
        tweenroutine = null;
      }
    }

    yield return new WaitForSeconds(6f);

    if (WasAutoSpinOn)
    {
      AutoSpin();
    }
    else
    {
      ToggleButtonGrp(true);
    }

    IsFreeSpin = false;
  }
  //starts the spin process
  internal void StartSlots(bool autoSpin = false)
  {

    if (audioController) audioController.PlaySpinButtonAudio();

    if (!autoSpin)
    {
      if (AutoSpinRoutine != null)
      {
        StopCoroutine(AutoSpinRoutine);
        StopCoroutine(tweenroutine);
        tweenroutine = null;
        AutoSpinRoutine = null;
      }
    }

    WinningsAnim(false);
    foreach (GameObject i in Wildreels_Object)
    {
      i.SetActive(false);
    }
    foreach (GameObject i in Borderreels_Object)
    {
      i.SetActive(false);
    }
    foreach (GameObject i in FSWildreels_Object)
    {
      i.SetActive(false);
    }
    foreach (GameObject i in FSBorderreels_Object)
    {
      i.SetActive(false);
    }
    if (TempList.Count > 0)
    {
      StopGameAnimation();
    }
    PayCalculator.ResetLines();
    tweenroutine = StartCoroutine(TweenRoutine());
  }

  //manage the Routine for spinning of the slots
  private IEnumerator TweenRoutine()
  {
    if (currentBalance < currentTotalBet && !IsFreeSpin)
    {
      CompareBalance();
      StopAutoSpin();
      yield return new WaitForSeconds(1);
      ToggleButtonGrp(true);
      yield break;
    }

    if (IsFreeSpin)
    {
      FreeSpinCounter--;
      if (uiManager) uiManager.UpdateUI(FreeSpinCounter, 0);
    }
    if (audioController) audioController.PlayWLAudio("spin");
    CheckSpinAudio = true;

    IsSpinning = true;

    ToggleButtonGrp(false);

    if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
    {
      StopSpin_Button.gameObject.SetActive(true);
    }

    for (int i = 0; i < numberOfSlots; i++)
    {
      if (!IsFreeSpin)
      {
        InitializeTweening(Slot_Transform[i], i);
        yield return new WaitForSeconds(0.1f);
      }
      else
      {
        InitializeTweening(FSSlot_Transform[i], i);
        yield return new WaitForSeconds(0.1f);
      }
    }

    if (!IsFreeSpin)
    {
      BalanceDeduction();
    }
    shuffleInitialMatrix(false);
    SocketManager.AccumulateResult(BetCounter);

    yield return new WaitUntil(() => SocketManager.isResultdone);

    for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
    {
      List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
      for (int i = 0; i < 6; i++)
      {
        if (!IsFreeSpin)
        {
          if (images[i].slotImages[j]) images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().sprite = myImages[resultnum[i]];
          if (resultnum[i] == 5)
          {
            images[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(true);
          }
          else
          {
            images[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(false);
          }
          if (resultnum[i] == 0 || resultnum[i] == 10)
          {
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-10.205f, -39.6f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(10.205f, 39.6f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = false;
            images[i].slotImages[j].transform.SetAsLastSibling();
          }
          else if (resultnum[i] == 11)
          {
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-47.5f, -55.555f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(73.5f, 55.555f);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
            images[i].slotImages[j].transform.SetAsLastSibling();
          }
          else
          {
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            images[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
          }
          PopulateAnimationSprites(images[i].slotImages[j].transform.GetChild(1).GetComponent<ImageAnimation>(), resultnum[i]);
        }
        else
        {
          if (FSimages[i].slotImages[j]) FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().sprite = FSImages[resultnum[i]];
          if (resultnum[i] == 5)
          {
            FSimages[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(true);
          }
          else
          {
            FSimages[i].slotImages[j].transform.GetChild(0).gameObject.SetActive(false);
          }
          if (resultnum[i] == 0 || resultnum[i] == 10)
          {
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-10.205f, -39.6f);
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(10.205f, 39.6f);
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = false;
            FSimages[i].slotImages[j].transform.SetAsLastSibling();
          }
          else if (resultnum[i] == 11)
          {
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(-47.5f, -55.555f);
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(73.5f, 55.555f);
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
            FSimages[i].slotImages[j].transform.SetAsLastSibling();
          }
          else
          {
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
          }
          PopulateFSAnimationSprites(FSimages[i].slotImages[j].transform.GetChild(1).GetComponent<ImageAnimation>(), resultnum[i]);
        }
      }
    }

    //yield return new WaitForSeconds(0.5f);
    if (IsTurboOn)
    {
      yield return new WaitForSeconds(0.2f);
      StopSpinToggle = true;
    }
    else
    {
      for (int i = 0; i < 10; i++)
      {
        yield return new WaitForSeconds(0.1f);
        if (StopSpinToggle)
        {
          break;
        }
      }
      StopSpin_Button.gameObject.SetActive(false);
    }

    for (int i = 0; i < numberOfSlots; i++)
    {
      if (!IsFreeSpin)
      {
        yield return StopTweening(Slot_Transform[i], i, StopSpinToggle);
      }
      else
      {
        yield return StopTweening(FSSlot_Transform[i], i, StopSpinToggle);
      }
    }

    yield return new WaitForSeconds(0.3f);
    StopSpinToggle = false;
    yield return alltweens[^1].WaitForCompletion();

    if (SocketManager.resultData.wildSymbolIndices.Count > 0)
    {
      for (int i = 0; i < SocketManager.resultData.wildSymbolIndices.Count; i++)
      {
        if (IsFreeSpin)
        {
          if (FSWildreels_Object[SocketManager.resultData.wildSymbolIndices[i]]) FSWildreels_Object[SocketManager.resultData.wildSymbolIndices[i]].SetActive(true);
          if (FSBorderreels_Object[SocketManager.resultData.wildSymbolIndices[i]]) FSBorderreels_Object[SocketManager.resultData.wildSymbolIndices[i]].SetActive(true);
        }
        else
        {
          if (Wildreels_Object[SocketManager.resultData.wildSymbolIndices[i]]) Wildreels_Object[SocketManager.resultData.wildSymbolIndices[i]].SetActive(true);
          if (Borderreels_Object[SocketManager.resultData.wildSymbolIndices[i]]) Borderreels_Object[SocketManager.resultData.wildSymbolIndices[i]].SetActive(true);
        }
      }
    }

    yield return new WaitForSeconds(0.1f);
    if (SocketManager.playerdata.currentWining > 0)
    {
      SpinDelay = 1.2f;
    }
    else
    {
      SpinDelay = 0.2f;
    }

    CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.matchCountofLines);
    KillAllTweens();

    CheckPopups = true;

    if (IsFreeSpin)
    {
      uiManager.UpdateUI(FreeSpinCounter, SocketManager.playerdata.currentWining);
    }

    if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");

    currentBalance = SocketManager.playerdata.Balance;

    CheckWinPopups();

    yield return new WaitUntil(() => !CheckPopups);
    if (SocketManager.resultData.freeSpinAdded || SocketManager.resultData.isFreeSpin)
    {
      FreeSpinCounter = SocketManager.resultData.freeSpinCount;
      if (SocketManager.resultData.freeSpinAdded)
      {
        uiManager.UpdateUI(FreeSpinCounter, 0);
        uiManager.ExtraFSPopup(FreeSpinCounter);
      }
      else if (!IsFreeSpin)
      {
        //if (SocketManager.resultData.linesToEmit.Count > 0)
        //{
        //    //yield return new WaitForSeconds(13);
        //    yield return new WaitForSeconds(4);
        //}
        //else
        //{
        //    yield return new WaitForSeconds(2);
        //}
        PayCalculator.ResetLines();
        IsFreeSpin = true;
        uiManager.FreeSpinProcessStart(SocketManager.resultData.freeSpinCount, currentTotalBet);
        EnableFreespins();
        shuffleInitialMatrixFS();
      }
      if (IsAutoSpin)
      {
        WasAutoSpinOn = true;
        StopAutoSpin();
        yield return new WaitForSeconds(0.1f);
      }
    }

    if (IsFreeSpin && FreeSpinCounter <= 0)
    {
      IsFreeSpin = false;
      uiManager.FreeSpinProcessStop();
      ToggleButtonGrp(true);
    }
    if ((IsFreeSpin || IsAutoSpin) && SocketManager.resultData.symbolsToEmit.Count > 0)
    {
      //yield return new WaitForSecondsRealtime(10);
      yield return new WaitForSecondsRealtime(2);
      IsSpinning = false;
    }
    else
    {
      yield return new WaitForSecondsRealtime(1);
      Debug.Log(IsAutoSpin + " " + IsFreeSpin + " " + WasAutoSpinOn);
      if (IsAutoSpin || IsFreeSpin || WasAutoSpinOn)
        ToggleButtonGrp(false);
      else
        ToggleButtonGrp(true);
      IsSpinning = false;
    }
  }

  private void EnableFreespins()
  {
    if (FS_Text) FS_Text.text = SocketManager.resultData.freeSpinCount + "\nfree spins awarded";
    if (FSPanel_Object) FSPanel_Object.SetActive(true);
    //if (Buttontext_Object) FS_Tween = Buttontext_Object.DOFade(0f, 0.5f).SetLoops(-1, LoopType.Yoyo);

    DOVirtual.DelayedCall(1.2f, () =>
    {
      FlipSlot();
    });
  }

  private void DisableFreeSpins()
  {
    if (FSPanel_Object) FSPanel_Object.SetActive(false);
    if (FS_Tween != null)
      FS_Tween.Kill();
    //if (WasAutoSpinOn)
    //{
    //    StopSpin_Button.GetComponent<ImageAnimation>().StartAnimation();
    //}
  }

  private void FlipSlot()
  {
    DisableFreeSpins();
    uiManager.FlipMySlot();
  }

  private void BalanceDeduction()
  {
    double bet = 0;
    double balance = 0;
    try
    {
      bet = double.Parse(TotalBet_text.text);
    }
    catch (Exception e)
    {
      Debug.Log("Error while conversion " + e.Message);
    }

    try
    {
      balance = double.Parse(Balance_text.text);
    }
    catch (Exception e)
    {
      Debug.Log("Error while conversion " + e.Message);
    }
    double initAmount = balance;

    balance = balance - bet;

    DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
    {
      if (Balance_text) Balance_text.text = initAmount.ToString("F3");
    });
  }

  internal void CheckWinPopups()
  {
    if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f3");
    if (SocketManager.playerdata.currentWining >= currentTotalBet * 10)
    {
      uiManager.BigWinSequence(SocketManager.playerdata.currentWining);
      if (audioController) audioController.PlayWLAudio("megaWin");
    }
    else
    {
      CheckPopups = false;
    }
  }

  //generate the payout lines generated 
  private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, List<List<int>> MatchCount)
  {
    List<int> y_points = null;
    List<int> points_anim = null;
    if (LineId.Count > 0 || points_AnimString.Count > 0)
    {
      if (audioController) audioController.PlayWLAudio("win");
      for (int i = 0; i < LineId.Count; i++)
      {
        y_points = y_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, MatchCount[i][1]);
      }

      for (int i = 0; i < points_AnimString.Count; i++)
      {
        points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

        for (int k = 0; k < points_anim.Count; k++)
        {
          if (points_anim[k] >= 10)
          {
            StartGameAnimation(images[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].transform.GetChild(1).gameObject);
          }
          else
          {
            StartGameAnimation(images[0].slotImages[points_anim[k]].transform.GetChild(1).gameObject);
          }
        }
      }

      WinningsAnim(true);
      if (LineId.Count > 0)
      {
        PayCalculator.StartLinesRoutine();
      }
    }
    else
    {
      if (audioController) audioController.StopWLAaudio();
    }
    CheckSpinAudio = false;
  }

  private void WinningsAnim(bool IsStart)
  {
    if (IsStart)
    {
      WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
    }
    else
    {
      WinTween.Kill();
      TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
    }
  }

  #endregion

  internal void CallCloseSocket()
  {
    SocketManager.CloseSocket();
  }


  void ToggleButtonGrp(bool toggle)
  {
    if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
    if (uiManager) uiManager.ToggleButtonGrp(toggle);
  }

  //start the icons animation
  private void StartGameAnimation(GameObject animObjects)
  {
    ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
    if (temp.textureArray.Count > 0)
    {
      temp.StartAnimation();
      TempList.Add(temp);
    }
  }

  //stop the icons animation
  private void StopGameAnimation()
  {
    for (int i = 0; i < TempList.Count; i++)
    {
      TempList[i].StopAnimation();
    }
    TempList.Clear();
    TempList.TrimExcess();
  }


  #region TweeningCode
  private void InitializeTweening(Transform slotTransform, int index)
  {
    slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, -(float)stopIndices[index]);
    Tweener tweener = slotTransform.DOLocalMoveY(-3014 + (float)stopIndices[index], 0.4f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
    tweener.Play();
    alltweens.Add(tweener);
  }



  private IEnumerator StopTweening(Transform slotTransform, int index, bool isStop)
  {
    alltweens[index].Pause();
    slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, -(float)stopIndices[index]);
    alltweens[index] = slotTransform.DOLocalMoveY(0 - (float)stopIndices[index] - 274, 0.5f).SetEase(Ease.OutSine);
    //yield return new WaitForSeconds(0.2f);
    if (!isStop)
    {
      yield return new WaitForSeconds(0.2f);
    }
    else
    {
      yield return null;
    }
  }


  private void KillAllTweens()
  {
    for (int i = 0; i < numberOfSlots; i++)
    {
      alltweens[i].Kill();
    }
    alltweens.Clear();

  }
  #endregion

}

[Serializable]
public class SlotImage
{
  public List<Image> slotImages = new List<Image>(10);
}

[Serializable]
public class BoxScript
{
  public List<BoxScripting> boxScripts = new List<BoxScripting>(10);
}

