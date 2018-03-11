using System;
using System.Collections;
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
        for (int i = 0; i < 4; i++)
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
            if (new[] { 0, 2, 4, 8, 10, 14, 16, 20, 26, 28, 34, 38, 40, 44, 50, 56, 58 }.Contains(timeRemainingSeconds % 60))
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

#pragma warning disable 414
    private string TwitchHelpMessage = "Submit button presses using !{0} press x on 1.";
    private int TwitchPlaysModuleScore = 1;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string inputCommand)
    {
        KMSelectable button;
        var command = inputCommand.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (command.Length == 2) command = new string[] { command[0], command[1], "on", "" };
        if (!command[0].Equals("press") && !command.Length.Equals(4)) yield break;
        switch (command[1])
        {
            case "x":
                button = Buttons[0];
                break;
            case "y":
                button = Buttons[1];
                break;
            case "a":
                button = null;
                yield return null;
                yield return new KMSelectable[] { Buttons[2] };
                yield break;
            case "b":
                button = null;
                yield return null;
                yield return new KMSelectable[] { Buttons[3] };
                yield break;
            default:
                button = null;
                yield break;
        }
        if (!command[2].Equals("at") && !command[2].Equals("on")) yield break;
        int result;
        float timeRemaining = Info.GetTime();
        int target = Mathf.FloorToInt(timeRemaining) % 10;
        if (int.TryParse(command[3], out result))
        {
            if (button == null || result < 0 || result > 9 ) yield break;
            yield return null;
            int i = 0;
            while (!(target == result))
            {
                yield return new WaitForSeconds(.1f);
                target = (Mathf.FloorToInt(Info.GetTime()) % 10);
                i++;
                if (i > 200)
                {
                    yield return null;
                    yield return "sendtochat There was an issue processing your command and it will be cancelled.";
                    yield break;
                }
            }
            yield return new KMSelectable[] { button };
        }
        else
        {
            if (button == null) yield break;
            yield return null;
            yield return new KMSelectable[] { button };
        }
    }
}
