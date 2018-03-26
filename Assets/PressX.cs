using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using KMHelper;

public class PressX : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMSelectable[] Buttons;

    private static int _moduleIDCounter = 1;
    private int _moduleID;
    private bool _isSolved;

    // Loading Screen
    void Start()
    {
        _moduleID = _moduleIDCounter++;
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
            if (Info.IsIndicatorOn(Indicator.CAR))
            {
            Debug.LogFormat("[Press X #{0}] Lit CAR (rule 1).", _moduleID);
            }
            else if (Info.IsIndicatorOff(Indicator.BOB))
            {
            Debug.LogFormat("[Press X #{0}] Unlit BOB (rule 2).", _moduleID);
            }
            else if (Info.GetBatteryCount() > 2 && Info.IsIndicatorOff(Indicator.FRQ))
            {
            Debug.LogFormat("[Press X #{0}] Unlit FRQ and 3+ batteries (rule 3).", _moduleID);
            }
            else if (Info.GetSerialNumberLetters().Count() == 3 && Info.GetBatteryCount() == 3 && Info.IsIndicatorOn(Indicator.NSA))
            {
            Debug.LogFormat("[Press X #{0}] Lit NSA, 3 batteries and equal number of digits and letters in serial (rule 4).", _moduleID);
            }
            else
            {
            Debug.LogFormat("[Press X #{0}] No special rule applies — using otherwise rule.", _moduleID);
        }
    }

    void Solved()
    {
        Module.HandlePass();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        _isSolved = true;
        Debug.LogFormat("[Press X #{0}] The module has been solved!", _moduleID);
    }

    void Strike(string message)
    {
        Module.HandleStrike();
        Debug.LogFormat("[Press X #{0}] {1}", _moduleID, message);
    }

    // The module answer
    void Answer(int i)
    {
        float timeRemaining = Info.GetTime();
        int timeRemainingSeconds = Mathf.FloorToInt(timeRemaining);

        if (_isSolved == true)
        {
            if (i == 0 || i == 1 || i == 2 || i == 3)
            {
                Module.HandleStrike();
                Debug.LogFormat("[Press X #{0}] Dude, you've solved this already. Stop pressing buttons!", _moduleID);
            }
            return;
        }

        if (i == 2)
        {
            Strike("Pressed A. STRIKE!");
        }
        else if (i == 3)
        {
            Strike("Pressed B. STRIKE!");
        }
        
        //Beep beep
        else if (Info.IsIndicatorOn(Indicator.CAR))
        {
            if (i == 0)
            {
                Solved();
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
                    Solved();
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
                    Solved();
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
            if (new[] { 8, 9, 11, 13, 17, 19, 23, 25, 29, 35, 37, 43, 47, 49, 53, 59 }.Contains(timeRemainingSeconds % 60))
            {
                if (i == 0)
                {
                    Module.HandleStrike();
                };
                if (i == 1)
                {
                    Solved();
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
                    Solved();
                }
                if (i == 1)
                {
                    Module.HandleStrike();
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
    private string TwitchHelpMessage = "Submit button presses using !{0} press x on 1 or !{0} press y on 23 or !press x on 8 28 48.";
    private int TwitchPlaysModuleScore = 1;
    private bool TwitchZenMode = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string inputCommand)
    {
        var match = Regex.Match(inputCommand.ToLowerInvariant(),
            "^(?:press |tap )?(x|y|a|b)(?:(?: at| on)?([0-9: ]+))?$");
        if (!match.Success) yield break;
        int index = "xyab".IndexOf(match.Groups[1].Value, StringComparison.Ordinal);
        if (index < 0) yield break;

        int target = Mathf.FloorToInt(Info.GetTime());
        bool waitingMusic = true;
        bool minutes;

        string[] times = match.Groups[2].Value.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        List<int> result = new List<int>();

        if (!times.Any() || index >= 2)
        {
            minutes = false;
            for (int i = 0; i < 60; i++)
            {
                result.Add(i);
            }
        }
        else
        {
            minutes = times.Any(x => x.Contains(":"));
            foreach (string time in times)
            {
                int daysInt = 0, hoursInt = 0, minutesInt = 0, secondsInt;
                string[] split = time.Split(':');
                if ((split.Length == 1 && int.TryParse(split[0], out secondsInt)) ||
                    (split.Length == 2 && int.TryParse(split[0], out minutesInt) && int.TryParse(split[1], out secondsInt)) ||
                    (split.Length == 3 && int.TryParse(split[0], out hoursInt) && int.TryParse(split[1], out minutesInt) && int.TryParse(split[2], out secondsInt)) ||
                    (split.Length == 4 && int.TryParse(split[0], out daysInt) && int.TryParse(split[1], out hoursInt) && int.TryParse(split[2], out minutesInt) && int.TryParse(split[3], out secondsInt)))
                    result.Add((daysInt * 86400) + (hoursInt * 3600) + (minutesInt * 60) + secondsInt);
                else
                {
                    yield return string.Format("sendtochaterror Badly formatted time {0}. Time should either be in seconds (53) or in full time (1:23:45)", time);
                    yield break;
                }
            }
            minutes |= result.Any(x => x >= 60);
        }
        yield return null;

        if (!minutes)
        {
            target %= 60;
            result = result.Select(x => x % 60).Distinct().ToList();
        }

        for (int i = result.Count - 1; i >= 0; i--)
        {
            int r = result[i];
            if (!minutes && !TwitchZenMode)
            {
                waitingMusic &= ((target + (r > target ? 60 : 0)) - r) > 30;
            }
            else if (!minutes)
            {
                waitingMusic &= ((r + (r < target ? 60 : 0)) - target) > 30;
            }
            else if (!TwitchZenMode)
            {
                if (r > target)
                {
                    result.RemoveAt(i);
                    continue;
                }
                waitingMusic &= (target - r) > 30;
            }
            else
            {
                if (r < target)
                {
                    result.RemoveAt(i);
                    continue;
                }
                waitingMusic &= (r - target) > 30;
            }
        }

        if (!result.Any())
        {
            yield return string.Format("sendtochaterror Button {0} was NOT pressed because all of your specified times have gone by already.", "xy"[index]);
            yield break;
        }

        if (waitingMusic)
            yield return "waiting music";

        while (!result.Contains(target))
        {
            yield return "trycancel The button was not pressed due to a request to cancel";
            target = (Mathf.FloorToInt(Info.GetTime()));
            if (!minutes) target %= 60;
        }
        yield return new KMSelectable[] {Buttons[index]};
    }
}
