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
    private int final;
    private bool _isSolved;
    private bool _doomicorn;

    //Room shown, lights off
    void Start()
    {
        _moduleID = _moduleIDCounter++;

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

        Debug.LogFormat("[Press X #{0}] Pressed {1} when time remaining was {2:00}:{3:00}.", _moduleID, "XYAB"[i], timeRemainingSeconds / 60, timeRemainingSeconds % 60);

        if (_isSolved == true)
        {
            Strike("Dude, you've solved this already. Stop pressing buttons!");
            return;
        }
        //Testing which button
        int correctButton = 0;
        int final = (Info.GetSolvedModuleNames().Count() % 4);
        int offIndicators = Info.GetOffIndicators().Count();
        int onIndicators = Info.GetOnIndicators().Count();
        if (offIndicators > onIndicators)
            {
                if (final == 0)
                {
                    correctButton = 2;
                }
                else if (final == 1)
                {
                    correctButton = 0;
                }
                else if (final == 2)
                {
                    correctButton = 3;
                }
                else
                {
                    correctButton = 1;
                }
            }
        else if (offIndicators < onIndicators)
        {
            if (final == 0)
            {
                correctButton = 3;
            }
            else if (final == 1)
            {
                correctButton = 1;
            }
            else if (final == 2)
            {
                correctButton = 2;
            }
            else
            {
                correctButton = 0;
            }
        }
        else
        {
            if (final == 0)
            {
                correctButton = 1;
            }
            else if (final == 1)
            {
                correctButton = 2;
            }
            else if (final == 2)
            {
                correctButton = 0;
            }
            else
            {
                correctButton = 3;
            }
        }
        //Logging
        if (offIndicators > onIndicators)
        {
            Debug.LogFormat("[Press X #{0}] For 0 solved, the correct button is A.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 1 solved, the correct button is X.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 2 solved, the correct button is B.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 3 solved, the correct button is Y.", _moduleID);
        }
        else if (offIndicators < onIndicators)
        {
            Debug.LogFormat("[Press X #{0}] For 0 solved, the correct button is B.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 1 solved, the correct button is Y.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 2 solved, the correct button is A.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 3 solved, the correct button is X.", _moduleID);
        }
        else
        {
            Debug.LogFormat("[Press X #{0}] For 0 solved, the correct button is Y.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 1 solved, the correct button is A.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 2 solved, the correct button is X.", _moduleID);
            Debug.LogFormat("[Press X #{0}] For 3 solved, the correct button is B.", _moduleID);
        }
        //CAR, 0 batt and X
        if (Info.IsIndicatorOn(Indicator.CAR) && correctButton == 0 && Info.GetBatteryCount() < 2)
        {
            if (i <= 3)
            {
                Solved();
                return;
            }
        }
        //3+ batt
        else if (Info.GetBatteryCount() >= 3)
        {
            if (i == correctButton)
            {
                if ((timeRemainingSeconds % 10) == (Info.GetSerialNumberNumbers().First()))
                {
                    Solved();
                }
                else
                {
                    Strike(string.Format("{0} is not equal to the first serial number digit of {1}, STRIKE.", timeRemainingSeconds % 10, Info.GetSerialNumberNumbers().First()));
                }
                return;
            }
            else
            {
                Strike(string.Format("Didn't press the correct button. STRIKE."));
            }
        }
        //Half an A press
        else if (correctButton == 2 && (Info.GetSerialNumberNumbers().Contains(2) || Info.GetSerialNumberNumbers().Contains(5)))
        {
            if (i == correctButton)
            {
                if (new[] {05, 30}.Contains(timeRemainingSeconds % 60))
                {
                    Solved();
                }
                else
                {
                    Strike(string.Format("{0} is not equal to 05 or 30, STRIKE.", timeRemainingSeconds % 60));
                }
                return;
            }
            else
            {
                Strike(string.Format("Didn't press the correct button. STRIKE."));
            }
        }
        //Not Y and lit NSA
        else if (correctButton != 1 && Info.IsIndicatorOn(Indicator.NSA))
        {
            if (i == correctButton)
            {
                if (new[] {00, 11, 22, 33, 44, 55}.Contains(timeRemainingSeconds % 60))
                {
                    Solved();
                }
                else
                {
                    Strike(string.Format("{0}. The seconds digits are not equal, STRIKE.", timeRemainingSeconds % 60));
                }
                return;
            }
            else
            {
                Strike(string.Format("Didn't press the correct button. STRIKE."));
            }
        }
        //If none apply
        else
        {
            if (i == correctButton)
            {
                if (new[] {09, 18, 27, 36, 45, 54}.Contains(timeRemainingSeconds % 60))
                {
                    Solved();
                }
                else
                {
                    Strike(string.Format("{0}. The seconds do not add to 9, STRIKE.", timeRemainingSeconds % 60));
                }
                return;
            }
            else
            {
                Strike(string.Format("Didn't press the correct button. STRIKE."));
            }
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Submit button presses using !{0} press x on 1 or !{0} press y on 23 or !press a on 8 28 48. Acceptable buttons are a, b, x and y.";
    private int TwitchPlaysModuleScore = 1;
    private bool TwitchZenMode = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string inputCommand)
    {
		inputCommand = inputCommand.Trim();
        var match = Regex.Match(inputCommand.ToLowerInvariant(),
            "^(?:press |tap )?(x|y|a|b)(?:(?: at| on)?([0-9: ]+))?$");
        if (!match.Success) yield break;
        int index = "xyab".IndexOf(match.Groups[1].Value, StringComparison.Ordinal);
        if (index < 0) yield break;

        bool waitingMusic = true;
        bool minutes;

        string[] times = match.Groups[2].Value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        List<int> result = new List<int>();

        if (!times.Any() && index > 3)
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
        yield return null;
        int target = Mathf.FloorToInt(Info.GetTime());

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
        Buttons[index].OnInteract();
        yield return new WaitForSeconds(0.1f);
    }
}
