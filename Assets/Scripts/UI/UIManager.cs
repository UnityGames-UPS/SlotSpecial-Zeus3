using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
  [Header("Popus UI")]
  [SerializeField]
  private GameObject MainPopup_Object;

  [Header("Main Screen Buttons")]
  [SerializeField]
  private Button SlotStart_Button;
  [SerializeField]
  private Button BetPlus_Button;
  [SerializeField]
  private Button BetMinus_Button;

  [Header("Paytable Popup")]
  [SerializeField]
  private Button Info_button;
  [SerializeField]
  private GameObject PaytablePopup_Object;
  [SerializeField]
  private RectTransform PaytablePopup_Transform;
  [SerializeField]
  private Button PaytableExit_Button;
  [SerializeField]
  private TMP_Text[] SymbolsText;
  [SerializeField]
  private TMP_Text FreeSpin_Text;
  [SerializeField]
  private Button Right_Button;
  [SerializeField]
  private Button Left_Button;
  [SerializeField]
  private GameObject[] Info_Screens;
  int screenCounter = 0;

  [Header("Settings Popup")]
  [SerializeField]
  private GameObject SettingsPopup_Object;
  [SerializeField]
  private Button SettingsExit_Button;
  [SerializeField]
  private Button Sound_Button;
  [SerializeField]
  private Button Music_Button;
  [SerializeField]
  private Button Settings_Button;

  [SerializeField]
  private GameObject MusicOn_Object;
  [SerializeField]
  private GameObject MusicOff_Object;
  [SerializeField]
  private GameObject SoundOn_Object;
  [SerializeField]
  private GameObject SoundOff_Object;

  [Header("Win Popup")]
  [SerializeField]
  private GameObject BigWinPopup_Object;
  [SerializeField]
  private RectTransform BigWin_Transform;
  [SerializeField]
  private TMP_Text BigWin_Text;
  [SerializeField]
  private Image BigWinFlash_Image;
  [SerializeField]
  private GameObject Star_Object;

  [Header("Free Spin Texts")]
  [SerializeField]
  private TMP_Text FSTotalBet_text;
  [SerializeField]
  private TMP_Text FSRemaining_text;
  [SerializeField]
  private TMP_Text FSTotalwin_text;
  [SerializeField]
  private TMP_Text FSInfo_text;
  private double freespinWin = 0;

  [Header("Extra free spin popup")]
  [SerializeField]
  private GameObject ExtraFSPanel_Object;
  [SerializeField]
  private RectTransform ExtraFSPopup_Transform;
  [SerializeField]
  private TMP_Text ExtraFS_Text;
  private Tween ExtraFS_Tween;

  [Header("Free Spin Complete Popup")]
  [SerializeField]
  private GameObject FSWinPopup_Object;
  [SerializeField]
  private TMP_Text FSWin_Text;

  [Header("Disconnection Popup")]
  [SerializeField]
  private Button CloseDisconnect_Button;
  [SerializeField]
  private GameObject DisconnectPopup_Object;
  [SerializeField] private GameObject ReconnectPopup_Object;

  [Header("LowBalance Popup")]
  [SerializeField]
  private Button LBExit_Button;
  [SerializeField]
  private GameObject LBPopup_Object;

  [Header("Flip Animation")]
  [SerializeField]
  private Sprite Flip_Sprite;
  [SerializeField]
  private Sprite Normal_Sprite;
  [SerializeField]
  private Image Slot_Image;
  [SerializeField]
  private RectTransform Slot_Transform;
  [SerializeField]
  private RectTransform BG_Transform;
  [SerializeField]
  private GameObject MainSlot_Object;
  [SerializeField]
  private GameObject MainFlipObject;
  [SerializeField]
  private RectTransform LinesText_Transform;
  [SerializeField]
  private RectTransform GreenLinesText_Transform;
  [SerializeField]
  private RectTransform YellowLinesText_Transform;
  [SerializeField]
  private RectTransform PurpleLinesText_Transform;
  [SerializeField]
  private RectTransform RedLinesText_Transform;
  [SerializeField]
  private RectTransform WinLinesEnable_Transform;
  [SerializeField]
  private RectTransform WinLinesDisable_Transform;
  [SerializeField]
  private RectTransform ButtonBG_Transform;
  [SerializeField]
  private RectTransform FlipButtonBG_Transform;
  [SerializeField]
  private Image NormalBG_Image;
  [SerializeField]
  private Image ZeusTitle_Image;
  [SerializeField]
  private Image ZeusFlip_Image;
  [SerializeField]
  private Button SkipWinAnimation;
  private bool isFlip = false;

  [Header("Quit Popup")]
  [SerializeField]
  private GameObject QuitPopup_Object;
  [SerializeField]
  private Button YesQuit_Button;
  [SerializeField]
  private Button NoQuit_Button;
  [SerializeField]
  private Button CrossQuit_Button;

  [SerializeField]
  private AudioController audioController;

  [SerializeField]
  private Button GameExit_Button;
  [SerializeField]
  private TMP_Text PayLines_text;
  [SerializeField]
  private TMP_Text PayLines_textTwo;


  [SerializeField]
  private SlotBehaviour slotManager;

  [SerializeField]
  private SocketIOManager socketManager;

  private bool isMusic = true;
  private bool isSound = true;
  private bool isExit = false;

  private Tween WinPopupTextTween;
  private Tween ClosePopupTween;

  private void Start()
  {
    if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
    if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

    if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
    if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });

    if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
    if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

    if (Info_button) Info_button.onClick.RemoveAllListeners();
    if (Info_button) Info_button.onClick.AddListener(delegate { screenCounter = 1; ChangePage(false); OpenPaytable(); });

    if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
    if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(ClosePaytable);

    if (Right_Button) Right_Button.onClick.RemoveAllListeners();
    if (Right_Button) Right_Button.onClick.AddListener(delegate { ChangePage(true); });

    if (Left_Button) Left_Button.onClick.RemoveAllListeners();
    if (Left_Button) Left_Button.onClick.AddListener(delegate { ChangePage(false); });

    if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
    if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

    if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
    if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

    if (MusicOn_Object) MusicOn_Object.SetActive(true);
    if (MusicOff_Object) MusicOff_Object.SetActive(false);

    if (SoundOn_Object) SoundOn_Object.SetActive(true);
    if (SoundOff_Object) SoundOff_Object.SetActive(false);

    if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
    if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

    if (NoQuit_Button) NoQuit_Button.onClick.RemoveAllListeners();
    if (NoQuit_Button) NoQuit_Button.onClick.AddListener(delegate { if (!isExit) { ClosePopup(QuitPopup_Object); } });

    if (CrossQuit_Button) CrossQuit_Button.onClick.RemoveAllListeners();
    if (CrossQuit_Button) CrossQuit_Button.onClick.AddListener(delegate { if (!isExit) { ClosePopup(QuitPopup_Object); } });

    if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
    if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

    if (YesQuit_Button) YesQuit_Button.onClick.RemoveAllListeners();
    if (YesQuit_Button) YesQuit_Button.onClick.AddListener(CallOnExitFunction);

    if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
    if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(delegate { CallOnExitFunction(); });

    if (audioController) audioController.ToggleMute(false);

    isMusic = true;
    isSound = true;

    if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
    if (Sound_Button) Sound_Button.onClick.AddListener(ToggleSound);

    if (Music_Button) Music_Button.onClick.RemoveAllListeners();
    if (Music_Button) Music_Button.onClick.AddListener(ToggleMusic);

    if (SkipWinAnimation) SkipWinAnimation.onClick.RemoveAllListeners();
    if (SkipWinAnimation) SkipWinAnimation.onClick.AddListener(SkipWin);
  }

  void SkipWin()
  {
    if (ClosePopupTween != null)
    {
      ClosePopupTween.Kill();
      ClosePopupTween = null;
    }
    if (WinPopupTextTween != null)
    {
      WinPopupTextTween.Kill();
      WinPopupTextTween = null;
    }
    if (BigWinPopup_Object) BigWinPopup_Object.SetActive(false);
    if (BigWin_Transform) BigWin_Transform.localPosition = new Vector3(BigWin_Transform.localPosition.x, 890, BigWin_Transform.localPosition.z);
    if (Star_Object) Star_Object.SetActive(false);
    if (BG_Transform) BG_Transform.DOScale(Vector3.one, 0.3f);
    if (Slot_Transform) Slot_Transform.DOScale(Vector3.one, 0.3f);
    if (WinLinesEnable_Transform) WinLinesEnable_Transform.DOScale(Vector3.one, 0.3f);
    if (WinLinesDisable_Transform) WinLinesDisable_Transform.DOScale(Vector3.one, 0.3f);
    if (slotManager) slotManager.CheckPopups = false;
    BigWinPopup_Object.SetActive(false);
    slotManager.CheckPopups = false;
  }

  private void StartSlots()
  {
    if (slotManager) slotManager.StartSlots();
  }

  private void ChangeBet(bool IncDec)
  {
    if (slotManager) slotManager.ChangeBet(IncDec);
  }

  internal void ToggleButtonGrp(bool toggle)
  {
    if (SlotStart_Button) SlotStart_Button.interactable = toggle;
    if (BetMinus_Button) BetMinus_Button.interactable = toggle;
    if (BetPlus_Button) BetPlus_Button.interactable = toggle;
    if (Info_button) Info_button.interactable = toggle;
  }

  internal void ToggleStartButton(bool toggy)
  {
    if (SlotStart_Button) SlotStart_Button.interactable = toggy;
  }

  private void ChangePage(bool Increment)
  {
    if (audioController) audioController.PlayButtonAudio();
    foreach (GameObject t in Info_Screens)
    {
      t.SetActive(false);
    }

    if (Increment)
    {
      if (screenCounter == Info_Screens.Length - 1)
      {
        screenCounter = 0;
      }
      else
      {
        screenCounter++;
      }
    }
    else
    {
      if (screenCounter == 0)
      {
        screenCounter = Info_Screens.Length - 1;
      }
      else
      {
        screenCounter--;
      }
    }
    Info_Screens[screenCounter].SetActive(true);
  }

  internal void LowBalPopup()
  {
    OpenPopup(LBPopup_Object);
  }

  internal void CheckAndClosePopups()
  {
    if (ReconnectPopup_Object.activeInHierarchy)
    {
      ClosePopup(ReconnectPopup_Object);
    }
    if (DisconnectPopup_Object.activeInHierarchy)
    {
      ClosePopup(DisconnectPopup_Object);
    }
  }

  internal void ReconnectionPopup()
  {
    OpenPopup(ReconnectPopup_Object);
  }

  internal void DisconnectionPopup()
  {
    if (!isExit)
    {
      OpenPopup(DisconnectPopup_Object);
    }
  }

  internal void BigWinSequence(double amount)
  {
    double initAmount = 0;
    if (BigWinPopup_Object) BigWinPopup_Object.SetActive(true);
    if (BigWinFlash_Image) BigWinFlash_Image.DOFade(1f, 0.3f).OnComplete(delegate { BigWinFlash_Image.DOFade(0f, 0.3f); });
    if (BigWin_Transform) BigWin_Transform.DOLocalMoveY(190, 0.3f).OnComplete(delegate
    {
      if (Star_Object) Star_Object.SetActive(true);
      WinPopupTextTween?.Kill();
      WinPopupTextTween = DOTween.To(() => initAmount, (val) => initAmount = val, amount, 2f).OnUpdate(() =>
          {
            if (BigWin_Text) BigWin_Text.text = initAmount.ToString("f3");
          }).OnComplete(delegate
          {
            ClosePopupTween?.Kill();
            ClosePopupTween = DOVirtual.DelayedCall(2f, () =>
                {
                  if (BigWinPopup_Object) BigWinPopup_Object.SetActive(false);
                  if (BigWin_Transform) BigWin_Transform.localPosition = new Vector3(BigWin_Transform.localPosition.x, 890, BigWin_Transform.localPosition.z);
                  if (Star_Object) Star_Object.SetActive(false);
                  if (BG_Transform) BG_Transform.DOScale(Vector3.one, 0.3f);
                  if (Slot_Transform) Slot_Transform.DOScale(Vector3.one, 0.3f);
                  if (WinLinesEnable_Transform) WinLinesEnable_Transform.DOScale(Vector3.one, 0.3f);
                  if (WinLinesDisable_Transform) WinLinesDisable_Transform.DOScale(Vector3.one, 0.3f);
                  if (slotManager) slotManager.CheckPopups = false;
                });
          });
    });
    if (BG_Transform) BG_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f);
    if (Slot_Transform) Slot_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f);
    if (WinLinesEnable_Transform) WinLinesEnable_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f);
    if (WinLinesDisable_Transform) WinLinesDisable_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f);
  }

  internal void InitialiseUIData(Paylines symbolsText)
  {
    PopulateSymbolsPayout(symbolsText);
  }

  internal void PopulateSymbolsPayout(Paylines paylines, bool init = true)
  {
    PayLines_text.text = socketManager.initFeat.numberOfLines.ToString() + "\nLines";
    PayLines_textTwo.text = socketManager.initFeat.numberOfLines.ToString();
    for (int i = 0; i < SymbolsText.Length; i++)
    {
      string text = null;
      if (paylines.symbols[i].multiplier[0] != 0)
      {
        text += (paylines.symbols[i].multiplier[0] * socketManager.initialData.bets[slotManager.BetCounter]).ToString();
      }
      if (paylines.symbols[i].multiplier[1] != 0)
      {
        text += "\n" + (paylines.symbols[i].multiplier[1] * socketManager.initialData.bets[slotManager.BetCounter]).ToString();
      }
      if (paylines.symbols[i].multiplier[2] != 0)
      {
        text += "\n" + (paylines.symbols[i].multiplier[2] * socketManager.initialData.bets[slotManager.BetCounter]).ToString();
      }
      if (paylines.symbols[i].multiplier[3] != 0)
      {
        text += "\n" + (paylines.symbols[i].multiplier[3] * socketManager.initialData.bets[slotManager.BetCounter]).ToString();
      }
      if (SymbolsText[i]) SymbolsText[i].text = text;
    }

    if (!init)
      return;

    FreeSpin fs = socketManager.initFeat.freeSpin;
    string fsText = "";
    int j = 5;
    for (int i = 0; i < fs.mults.Count; i++)
    {
      if (i != 0)
      {
        fsText += "\n";
      }
      if (fs.mults[i] != 0)
      {
        fsText += j.ToString() + " = " + "<color=yellow>" + fs.counts[i] + " Free Spins + " + fs.mults[i] + "x Bet Per Line" + "</color>";
      }
      else
      {
        fsText += j.ToString() + " = " + "<color=yellow>" + fs.counts[i] + " Free Spins" + "</color>";
      }
      j--;
    }
    FreeSpin_Text.text = fsText;
  }

  private void CallOnExitFunction()
  {
    isExit = true;
    audioController.PlayButtonAudio();
    slotManager.CallCloseSocket();
  }

  private void OpenPaytable()
  {
    if (PaytablePopup_Transform) PaytablePopup_Transform.localPosition = new Vector2(PaytablePopup_Transform.localPosition.x, -1085);
    OpenPopup(PaytablePopup_Object);
    if (PaytablePopup_Transform) PaytablePopup_Transform.DOLocalMoveY(-21, 0.5f);
  }

  private void ClosePaytable()
  {
    if (PaytablePopup_Transform) PaytablePopup_Transform.DOLocalMoveY(-1085, 0.5f).OnComplete(delegate
    {
      ClosePopup(PaytablePopup_Object);
    });
  }

  private void OpenPopup(GameObject Popup)
  {
    if (audioController) audioController.PlayButtonAudio();
    if (Popup) Popup.SetActive(true);
    if (MainPopup_Object) MainPopup_Object.SetActive(true);
  }

  private void ClosePopup(GameObject Popup)
  {
    if (audioController) audioController.PlayButtonAudio();
    if (Popup) Popup.SetActive(false);
    if (!DisconnectPopup_Object.activeSelf)
    {
      if (MainPopup_Object) MainPopup_Object.SetActive(false);
    }
  }

  private void ToggleMusic()
  {
    if (audioController) audioController.PlaySpinButtonAudio();
    isMusic = !isMusic;
    if (isMusic)
    {
      if (MusicOn_Object) MusicOn_Object.SetActive(true);
      if (MusicOff_Object) MusicOff_Object.SetActive(false);
      audioController.ToggleMute(false, "bg");
    }
    else
    {
      if (MusicOn_Object) MusicOn_Object.SetActive(false);
      if (MusicOff_Object) MusicOff_Object.SetActive(true);
      audioController.ToggleMute(true, "bg");
    }
  }

  internal void ExtraFSPopup(int freespincount)
  {
    if (ExtraFS_Text) ExtraFS_Text.text = "you have been awarded with extra free spins. total free spins left <color=yellow>" + freespincount + "</color>.";
    if (ExtraFSPanel_Object) ExtraFSPanel_Object.SetActive(true);
    if (ExtraFSPopup_Transform) ExtraFS_Tween = ExtraFSPopup_Transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.5f).SetLoops(-1, LoopType.Yoyo);
    DOVirtual.DelayedCall(2f, () =>
    {
      ExtraFS_Tween.Kill();
      if (ExtraFSPopup_Transform) ExtraFSPopup_Transform.localScale = Vector3.one;
      if (ExtraFSPanel_Object) ExtraFSPanel_Object.SetActive(false);
    });
  }

  internal void FreeSpinProcessStart(int count, double totalBet)
  {
    if (FSRemaining_text) FSRemaining_text.text = count.ToString();
    if (FSTotalBet_text) FSTotalBet_text.text = totalBet.ToString();
    if (FSInfo_text) FSInfo_text.text = count + " Free spins remaining";
    freespinWin = 0;
    if (BG_Transform) BG_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.4f);
    if (Slot_Transform) Slot_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.4f);
  }

  internal void FlipMySlot()
  {
    FlipSlot();
    if (audioController) audioController.PlayWLAudio("Flip");
    DOVirtual.DelayedCall(2f, () =>
    {
      slotManager.StartFreeSpinRoutine();
    });
  }

  internal void UpdateUI(int count, double winning)
  {
    if (FSRemaining_text) FSRemaining_text.text = count.ToString();
    freespinWin += winning;
    if (FSTotalwin_text) FSTotalwin_text.text = freespinWin.ToString("f3");
    if (FSInfo_text) FSInfo_text.text = count + " Free spins remaining";
  }

  internal void FreeSpinProcessStop()
  {
    if (FSWin_Text) FSWin_Text.text = freespinWin.ToString();
    if (FSWinPopup_Object) FSWinPopup_Object.SetActive(true);
    DOVirtual.DelayedCall(3f, () =>
    {
      if (FSWinPopup_Object) FSWinPopup_Object.SetActive(false);
      FlipSlot();
      FSTotalwin_text.text = "0.000";
    });
  }

  private void FlipSlot()
  {
    if (!isFlip)
    {
      isFlip = true;
      if (BG_Transform) BG_Transform.DOLocalRotate(new Vector3(0, 90, 0), 0.3f).SetEase(Ease.Linear).OnComplete(delegate
      {
        if (Slot_Image) Slot_Image.sprite = Flip_Sprite;
        if (MainSlot_Object) MainSlot_Object.SetActive(false);
        if (MainFlipObject) MainFlipObject.SetActive(true);
        if (LinesText_Transform) LinesText_Transform.localEulerAngles = new Vector3(0, 180, 0);
        if (GreenLinesText_Transform) GreenLinesText_Transform.localEulerAngles = new Vector3(0, 180, 0);
        if (YellowLinesText_Transform) YellowLinesText_Transform.localEulerAngles = new Vector3(0, 180, 0);
        if (PurpleLinesText_Transform) PurpleLinesText_Transform.localEulerAngles = new Vector3(0, 180, 0);
        if (RedLinesText_Transform) RedLinesText_Transform.localEulerAngles = new Vector3(0, 180, 0);
        if (WinLinesEnable_Transform) WinLinesEnable_Transform.localEulerAngles = new Vector3(0, 180, 0);
        if (WinLinesDisable_Transform) WinLinesDisable_Transform.localEulerAngles = new Vector3(0, 180, 0);
        if (BG_Transform) BG_Transform.DOLocalRotate(new Vector3(0, 180, 0), 0.5f).OnComplete(delegate
              {
                if (BG_Transform) BG_Transform.DOScale(Vector3.one, 0.4f);
              });

      });
      if (Slot_Transform) Slot_Transform.DOLocalRotate(new Vector3(0, 90, 0), 0.3f).SetEase(Ease.Linear).OnComplete(delegate
      {
        if (FlipButtonBG_Transform) FlipButtonBG_Transform.gameObject.SetActive(true);
        if (FlipButtonBG_Transform) FlipButtonBG_Transform.DOLocalMoveX(669, 0.5f);
        if (ButtonBG_Transform) ButtonBG_Transform.DOLocalMoveX(-1900, 0.5f).OnComplete(delegate { if (ButtonBG_Transform) ButtonBG_Transform.gameObject.SetActive(false); });
        if (Slot_Transform) Slot_Transform.DOLocalRotate(new Vector3(0, 180, 0), 0.5f).OnComplete(delegate
              {
                if (Slot_Transform) Slot_Transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.Linear);
              });

      });
      if (NormalBG_Image) NormalBG_Image.DOFade(0f, 1f).OnComplete(delegate { NormalBG_Image.gameObject.SetActive(false); });
      if (ZeusTitle_Image) ZeusTitle_Image.DOFade(0f, 1f);
      if (ZeusFlip_Image) ZeusFlip_Image.DOFade(1f, 1f);

      //slotManager.StopSpin_Button.GetComponent<ImageAnimation>().StopAnimation();
    }
    else
    {
      isFlip = false;
      if (BG_Transform) BG_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f).SetEase(Ease.Linear).OnComplete(delegate
      {
        if (BG_Transform) BG_Transform.DOLocalRotate(new Vector3(0, 90, 0), 0.3f).SetEase(Ease.Linear).OnComplete(delegate
              {
                if (Slot_Image) Slot_Image.sprite = Normal_Sprite;
                if (MainSlot_Object) MainSlot_Object.SetActive(true);
                if (MainFlipObject) MainFlipObject.SetActive(false);
                if (LinesText_Transform) LinesText_Transform.localEulerAngles = new Vector3(0, 0, 0);
                if (GreenLinesText_Transform) GreenLinesText_Transform.localEulerAngles = new Vector3(0, 0, 0);
                if (YellowLinesText_Transform) YellowLinesText_Transform.localEulerAngles = new Vector3(0, 0, 0);
                if (PurpleLinesText_Transform) PurpleLinesText_Transform.localEulerAngles = new Vector3(0, 0, 0);
                if (RedLinesText_Transform) RedLinesText_Transform.localEulerAngles = new Vector3(0, 0, 0);
                if (WinLinesEnable_Transform) WinLinesEnable_Transform.localEulerAngles = new Vector3(0, 0, 0);
                if (WinLinesDisable_Transform) WinLinesDisable_Transform.localEulerAngles = new Vector3(0, 0, 0);
                if (BG_Transform) BG_Transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f).OnComplete(delegate
                      {
                        if (BG_Transform) BG_Transform.DOScale(Vector3.one, 0.4f);
                      });

              });
      });
      if (Slot_Transform) Slot_Transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f).SetEase(Ease.Linear).OnComplete(delegate
      {
        if (Slot_Transform) Slot_Transform.DOLocalRotate(new Vector3(0, 90, 0), 0.3f).SetEase(Ease.Linear).OnComplete(delegate
              {
                if (ButtonBG_Transform) ButtonBG_Transform.gameObject.SetActive(true);


                if (ButtonBG_Transform) ButtonBG_Transform.DOLocalMoveX(-489, 0.5f);
                if (FlipButtonBG_Transform) FlipButtonBG_Transform.DOLocalMoveX(1900, 0.5f).OnComplete(delegate { if (FlipButtonBG_Transform) FlipButtonBG_Transform.gameObject.SetActive(false); });
                if (Slot_Transform) Slot_Transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f).OnComplete(delegate
                      {
                        if (Slot_Transform) Slot_Transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.Linear);
                      });

              });
      });

      if (NormalBG_Image) NormalBG_Image.gameObject.SetActive(true);
      if (NormalBG_Image) NormalBG_Image.DOFade(1f, 1f);
      if (ZeusTitle_Image) ZeusTitle_Image.DOFade(1f, 1f);
      if (ZeusFlip_Image) ZeusFlip_Image.DOFade(0f, 1f);

      //if (slotManager.WasAutoSpinOn)
      //{
      //    slotManager.StopSpin_Button.GetComponent<ImageAnimation>().StartAnimation();
      //}
    }
  }

  private void ToggleSound()
  {
    isSound = !isSound;
    if (audioController) audioController.PlaySpinButtonAudio();
    if (isSound)
    {
      if (SoundOn_Object) SoundOn_Object.SetActive(true);
      if (SoundOff_Object) SoundOff_Object.SetActive(false);
      if (audioController) audioController.ToggleMute(false, "button");
      if (audioController) audioController.ToggleMute(false, "wl");
    }
    else
    {
      if (SoundOn_Object) SoundOn_Object.SetActive(false);
      if (SoundOff_Object) SoundOff_Object.SetActive(true);
      if (audioController) audioController.ToggleMute(true, "button");
      if (audioController) audioController.ToggleMute(true, "wl");
    }
  }
}
