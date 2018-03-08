using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KMHelper;

public class PressX : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMSelectable[] Buttons;

    private static int _moduleIDCounter = 1;
    private int _moduleID;
    private bool Active;
    private bool _isSolved;

    // Loading Screen
    void Start()
    {
        _moduleID = _moduleIDCounter++;
        Module.OnActivate += delegate () { Active = true; };
    }
    //Room shown, lights off
    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            //Button handling 
            int b = i;
            Buttons[i].OnInteract += delegate ()
            {
                Buttons[b].AddInteractionPunch();
                Answer(b);
                return false;
            };
        }
        //Logging
        {
            if (Info.IsIndicatorOn(Indicator.CAR))
            {
                Debug.LogFormat("[Press X #{0}] Lit Car.", _moduleID);
            }
            else if (Info.IsIndicatorOff(Indicator.BOB))
            {
                Debug.LogFormat("[Press X #{0}] Unlit BOB.", _moduleID);
            }
            else if (Info.GetBatteryCount() > 2 && Info.IsIndicatorOff(Indicator.FRQ))
            {
                Debug.LogFormat("[Press X #{0}] Unlit FRQ and 3+ batteries.", _moduleID);
            }
            else if (Info.GetSerialNumberLetters().Count() == 3 && Info.GetBatteryCount() == 3 && Info.IsIndicatorOn(Indicator.NSA))
            {
                Debug.LogFormat("[Press X #{0}] Lit NSA, 3 batteries and equal number of numbers and letters in serial.", _moduleID);
            }
            else
            {
                Debug.LogFormat("[Press X #{0}] None applied.", _moduleID);
            }
            if (_isSolved)
            {
                Debug.LogFormat("[Press X #{0}] The module has been solved!", _moduleID);
            }
        }
    }
    // The module answer
    void Answer(int i)
    {
        float timeRemaining = Info.GetTime();
        int timeRemainingSeconds = Mathf.FloorToInt(timeRemaining);

        if (i == 2)
        {
            Module.HandleStrike();
            Debug.LogFormat("[Press X #{0}] Pressed A. STRIKE!", _moduleID);
        }
        if (i == 3)
        {
            Module.HandleStrike();
            Debug.LogFormat("[Press X #{0}] Pressed B. STRIKE!", _moduleID);
        }
        if (_isSolved == true)
        {
            if (i == 0 || i == 1 || i == 2 || i == 3)
            {
                Module.HandleStrike();
                Debug.LogFormat("[Press X #{0}] Dude, you've solved this already. Stop pressing buttons!", _moduleID);
            }
        }
        //Beep beep
        if (Info.IsIndicatorOn(Indicator.CAR))
        {
            if (i == 0)
            {
                Module.HandlePass();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                _isSolved = true;
            };
            if (i == 1)
            {
                Module.HandleStrike();
            }
        }
        //"No mistakes, just happy accidents." SAY THAT TO MY CODING ERRORS!
        else if (Info.IsIndicatorOff(Indicator.BOB))
        {
            if ((timeRemainingSeconds % 10) == (Info.GetSerialNumberNumbers().First()))
            {
                if (i == 0)
                {
                    Module.HandlePass();
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    _isSolved = true;
                };
                if (i == 1)
                {
                    Module.HandleStrike();
                };
            }
            else
            {
                if (i == 0)
                {
                    Module.HandleStrike();
                };
                if (i == 1)
                {
                    Module.HandleStrike();
                };
            }
        }
        //Unicorn
        else if (Info.GetBatteryCount() > 2 && Info.IsIndicatorOff(Indicator.FRQ))
        {
            if (new[] { 0, 2, 4, 11, 13, 19, 20, 22, 28, 31, 37, 40, 46, 55, 59 }.Contains(timeRemainingSeconds % 60))
            {
                if (i == 0)
                {
                    Module.HandlePass();
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    _isSolved = true;
                };
                if (i == 1)
                {
                    Module.HandleStrike();
                };
            }
            else
            {
                if (i == 0)
                {
                    Module.HandleStrike();
                };
                if (i == 1)
                {
                    Module.HandleStrike();
                };
            }
        }
        //Doomicorn
        else if (Info.GetSerialNumberLetters().Count() == 3 && Info.GetBatteryCount() == 3 && Info.IsIndicatorOn(Indicator.NSA))
        {
            if (new[] { 8, 9, 11, 13, 15, 23, 25, 29, 35, 37, 43, 47, 49, 53, 59, }.Contains(timeRemainingSeconds % 60))
            {
                if (i == 0)
                {
                    Module.HandleStrike();
                };
                if (i == 1)
                {
                    Module.HandlePass();
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    _isSolved = true;
                }
            }
            else
            {
                if (i == 0)
                {
                    Module.HandleStrike();
                };
                if (i == 1)
                {
                    Module.HandleStrike();
                }
            }
        }
        //If none apply
        else
        {
            if ((timeRemainingSeconds % 10) == (Info.GetSerialNumberNumbers().Last()))
            {
                if (i == 0)
                {
                    Module.HandlePass();
                }
            }
            else
            {
                if (i == 0)
                {
                    Module.HandleStrike();
                }
                if (i == 1)
                {
                    Module.HandleStrike();
                }
            }
        }
    }
}
