using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class MyMomScript : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMSelectable[] buttons;
    public KMSelectable submitButton;
    public TextMesh[] labels;
    public Material[] symbols;
    public Material blankScreen;
    public Material WhiteButton;
    public Material RedButton;

    int shift = 0;

    // This is the arrangement of the letters in My Mom.
    static char[,] symbolTable = new char[,] {
        {'M', 'Y', ' '},
        {'M', 'O', 'M'}
    };

    private char[,] newSymbolTable = new char[,] {
        {'M', 'Y', ' '},
        {'M', 'O', 'M'}
    };

    private char[,] solveSymbolTable = new char[,] {
        {'M', 'Y', ' '},
        {'M', 'O', 'M'}
    };

    // This is the letters.
    readonly string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly string space = " ";

    char[,] display;
    int x;
    int y;
    readonly int caesar;
    int moduleId;
    static int moduleIdCounter = 1;

    private bool isSolved = false;

    void Start()
    {
        isSolved = false;
        moduleId = moduleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += OnActivate;

        buttons[0].OnInteract += delegate { return RotateVertical(display, buttons[0], 1); };
        buttons[1].OnInteract += delegate { return RotateVertical(display, buttons[1], 1); };
        buttons[2].OnInteract += delegate { return RotateVertical(display, buttons[2], 2); };
        buttons[3].OnInteract += delegate { return RotateVertical(display, buttons[3], 2); };
        buttons[4].OnInteract += delegate { return RotateVertical(display, buttons[4], 3); };
        buttons[5].OnInteract += delegate { return RotateVertical(display, buttons[5], 3); };
        buttons[6].OnInteract += delegate { return RotateHorizontal(display, buttons[6], 0, -1); };
        buttons[7].OnInteract += delegate { return RotateHorizontal(display, buttons[7], 0, 1); };
        buttons[8].OnInteract += delegate { return RotateHorizontal(display, buttons[8], 1, -1); };
        buttons[9].OnInteract += delegate { return RotateHorizontal(display, buttons[9], 1, 1); };

        /*
        for (int i = 0; i < 10; i += 2) {
            var j = i;
            var k = i + 1;
            buttons[j].OnSelect += delegate { SelectGroup(j, k); };
            buttons[k].OnSelect += delegate { SelectGroup(j, k); };
            buttons[j].OnDeselect += delegate { DeselectGroup(j, k); };
            buttons[k].OnDeselect += delegate { DeselectGroup(j, k); };
        }
        */
        submitButton.OnInteract += delegate {Submit(); return false; } ;

        x = 0;
        y = 0;
        Debug.LogFormat("[My Mom #{0}] Position in grid: ({1}, {2})", moduleId, x + 1, y + 1);

        display = new char[2, 3];
        display[0, 0] = symbolTable[0,0];
        display[0, 1] = symbolTable[0,1];
        display[0, 2] = symbolTable[0,2];
        display[1, 0] = symbolTable[1,0];
        display[1, 1] = symbolTable[1,1];
        display[1, 2] = symbolTable[1,2];

        int cipher = Rnd.Range(0,26);

        display[0, 0] = characters[(display[0,0] + cipher)%26];
        display[0, 1] = characters[(display[0,1] + cipher)%26];
        display[1, 0] = characters[(display[1, 0] + cipher) % 26];
        display[1, 1] = characters[(display[1, 1] + cipher) % 26];
        display[1, 2] = characters[(display[1, 2] + cipher) % 26];

        solveSymbolTable[0, 0] = display[0, 0];
        solveSymbolTable[0, 1] = display[0, 1];
        solveSymbolTable[0, 2] = display[0, 2];
        solveSymbolTable[1, 0] = display[1, 0];
        solveSymbolTable[1, 1] = display[1, 1];
        solveSymbolTable[1, 2] = display[1, 2];

        newSymbolTable[0, 0] = display[0, 0];
        newSymbolTable[0, 1] = display[0, 1];
        newSymbolTable[0, 2] = display[0, 2];
        newSymbolTable[1, 0] = display[1, 0];
        newSymbolTable[1, 1] = display[1, 1];
        newSymbolTable[1, 2] = display[1, 2];

        

        var initialOrder = Enumerable.Range(0, 6).ToArray();
        ShuffleArray(initialOrder);

        for (int i = 0; i < 6; i++)
            display[i / 3, i % 3] = newSymbolTable[y + initialOrder[i] / 3, x + initialOrder[i] % 3];
        Debug.LogFormat("[My Mom #{0}] Initial display: {1} / {2}", moduleId, new string(Enumerable.Range(0, 2).Select(i => display[0, i]).ToArray()), new string(Enumerable.Range(0, 3).Select(i => display[1, i]).ToArray()));
    }

    void OnActivate()
    {
        RedrawSymbols();
    }

    private bool RotateVertical(char[,] disp, KMSelectable button, int column)
    {
        Audio.PlaySoundAtTransform("tick", button.transform);
        button.AddInteractionPunch(0.25f);
        RotateVertical(disp, column);
        RedrawSymbols();
        return false;
    }

    private static void RotateVertical(char[,] disp, int column)
    {
        var temp = disp[0, column - 1];
        disp[0, column - 1] = disp[1, column - 1];
        disp[1, column - 1] = temp;
    }

    private bool RotateHorizontal(char[,] disp, KMSelectable button, int line, int direction = 0)
    {
        Audio.PlaySoundAtTransform("tick", button.transform);
        button.AddInteractionPunch(0.25f);
        RotateHorizontal(disp, line, direction);
        RedrawSymbols();
        return false;
    }

    private static void RotateHorizontal(char[,] disp, int line, int direction)
    {
        if (direction == -1)
        {
            var temp = disp[line, 0];
            disp[line, 0] = disp[line, 1];
            disp[line, 1] = disp[line, 2];
            disp[line, 2] = temp;
        }
        else
        {
            var temp = disp[line, 2];
            disp[line, 2] = disp[line, 1];
            disp[line, 1] = disp[line, 0];
            disp[line, 0] = temp;
        }
    }


    void RedrawSymbols()
    {
        labels[0].text = display[0, 0].ToString();
        labels[1].text = display[0, 1].ToString();
        labels[2].text = display[0, 2].ToString();
        labels[3].text = display[1, 0].ToString();
        labels[4].text = display[1, 1].ToString();
        labels[5].text = display[1, 2].ToString();

    }

    void Submit()
    {
        Audio.PlaySoundAtTransform("tick", this.transform);
        GetComponent<KMSelectable>().AddInteractionPunch();
        Debug.LogFormat("[My Mom #{0}] You submitted: {1} / {2}", moduleId, new string(Enumerable.Range(0, 3).Select(i => display[0, i]).ToArray()), new string(Enumerable.Range(0, 3).Select(i => display[1, i]).ToArray()));
        bool nani = false;
        bool played = false;
        int cheese = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                
                if (display[j, i] != newSymbolTable[y + j, x + i])
                {
                    Debug.LogFormat("[My Mom #{0}] Wrong solution. Strike.", moduleId);
                    nani = true;
                    Audio.PlaySoundAtTransform("screech", submitButton.transform);
                }
                if (display[j, i] == newSymbolTable[y + j, x + i])
                {
                    cheese++;
                }
                if (!nani && cheese == 6)
                {
                   isSolved = true;
                }
            }
        }
        if (nani)
        {
            Debug.LogFormat("[My Mom #{0}] Wrong solution. Strike.", moduleId);
            BombModule.HandleStrike();
            nani = false;
            Audio.PlaySoundAtTransform("screech", submitButton.transform);
        }
        if (isSolved && !played)
        {
            Debug.LogFormat("[My Mom #{0}] Module solved.", moduleId);
            BombModule.HandlePass();
            Audio.PlaySoundAtTransform("mother", submitButton.transform);
            StartCoroutine(Animate());
            played = true;
        }
    }

    IEnumerator Animate()
    {
        labels[0].text = symbolTable[0,0].ToString();
        yield return new WaitForSecondsRealtime(1.0f);
        labels[1].text = symbolTable[0,1].ToString();
        yield return new WaitForSecondsRealtime(1.0f);
        labels[3].text = symbolTable[1,0].ToString();
        yield return new WaitForSecondsRealtime(1.0f);
        labels[4].text = symbolTable[1, 1].ToString();
        yield return new WaitForSecondsRealtime(1.0f);
        labels[5].text = symbolTable[1, 2].ToString();
    }

    void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = Rnd.Range(0, i);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }

    void SelectGroup(int b1, int b2) {
        Debug.LogFormat("[My Mom #{0}] Group <{1},{2}> selected.", moduleId, b1 + 1, b2 + 2);
        buttons[b1].gameObject.GetComponent<Renderer>().sharedMaterial = RedButton;
        buttons[b2].gameObject.GetComponent<Renderer>().sharedMaterial = RedButton;
    }

    void DeselectGroup(int b1, int b2) {
        Debug.LogFormat("[My Mom #{0}] Group <{1},{2}> deselected.", moduleId, b1 + 1, b2 + 2);
        buttons[b1].gameObject.GetComponent<Renderer>().sharedMaterial = WhiteButton;
        buttons[b2].gameObject.GetComponent<Renderer>().sharedMaterial = WhiteButton;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Cycle a row with cycle t l. Cycle a column with cycle m. Submit with !{0} submit. Rows are TL/TR/BL/BR, columns are L/R/M. Spaces are important!";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToLowerInvariant();
        var pieces = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length >= 2 && pieces[0] == "cycle")
        {
            var list = new List<KMSelectable>();
            int button;
            for (int i = 1; i < pieces.Length; i++)
            {
                switch (pieces[i])
                {
                    case "l": case "left": list.Add(buttons[0]); break;
                    case "m": case "middle": case "c": case "center": case "centre": list.Add(buttons[2]); break;
                    case "r": case "right": list.Add(buttons[4]); break;

                    case "t":
                    case "top":
                    case "u":
                    case "up":
                    case "upper":
                        if ((i + 1) == pieces.Length)
                            return null;
                        switch (pieces[i + 1])
                        {
                            case "l": case "left": button = 6; break;
                            case "r": case "right": button = 7; break;
                            default: return null;
                        }
                        list.Add(buttons[button]);
                        i++;
                        break;

                    case "tl":
                    case "topleft":
                    case "ul":
                    case "upleft":
                    case "upperleft":
                        list.Add(buttons[6]);
                        break;

                    case "tr":
                    case "topright":
                    case "ur":
                    case "upright":
                    case "upperright":
                        list.Add(buttons[7]);
                        break;

                    case "b":
                    case "bottom":
                    case "d":
                    case "down":
                    case "lower":
                        if ((i + 1) == pieces.Length)
                            return null;
                        switch (pieces[i + 1])
                        {
                            case "l": case "left": button = 8; break;
                            case "r": case "right": button = 9; break;
                            default: return null;
                        }
                        list.Add(buttons[button]);
                        i++;
                        break;

                    case "bl":
                    case "bottomleft":
                    case "dl":
                    case "downleft":
                    case "lowerleft":
                        list.Add(buttons[8]);
                        break;

                    case "br":
                    case "bottomright":
                    case "dr":
                    case "downright":
                    case "lowerright":
                        list.Add(buttons[9]);
                        break;

                    default:
                        return null;
                }
            }
            return list.ToArray();
        }
        else if (pieces.Length == 1 && pieces[0] == "submit")
            return new[] { submitButton };
        else
            return null;
    }

    struct SolverQueueItem
    {
        public char[,] PrevConfiguration;
        public char[,] NextConfiguration;
        public int Button;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (isSolved)
            yield break;

        var q = new Queue<SolverQueueItem>();
        q.Enqueue(new SolverQueueItem { Button = -1, NextConfiguration = display });
        var already = new Dictionary<string, SolverQueueItem>();
        var solutionKey = Enumerable.Range(0, 6).Select(ix => solveSymbolTable[y + ix / 3, x + ix % 3]).Join("");

        while (q.Count > 0)
        {
            var item = q.Dequeue();
            var str = Enumerable.Range(0, 6).Select(ix => item.NextConfiguration[ix / 3, ix % 3]).Join("");

            if (already.ContainsKey(str))
                continue;
            already[str] = item;

            if (str == solutionKey)
                break;

            var config = (char[,])item.NextConfiguration.Clone();
            RotateVertical(config, 1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 0 });
            config = (char[,])item.NextConfiguration.Clone();
            RotateVertical(config, 2);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 2 });
            config = (char[,])item.NextConfiguration.Clone();
            RotateVertical(config, 3);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 4 });
            config = (char[,])item.NextConfiguration.Clone();
            RotateHorizontal(config, 0, -1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 6 });
            config = (char[,])item.NextConfiguration.Clone();
            RotateHorizontal(config, 0, 1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 7 });
            config = (char[,])item.NextConfiguration.Clone();
            RotateHorizontal(config, 1, -1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 8 });
            config = (char[,])item.NextConfiguration.Clone();
            RotateHorizontal(config, 1, 1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 9 });
        }

        if (!already.ContainsKey(solutionKey))
            throw new InvalidOperationException();

        var buttonPresses = new List<int>();
        var cnf = solutionKey;
        while (cnf != null)
        {
            var item = already[cnf];
            buttonPresses.Add(item.Button);
            cnf = item.PrevConfiguration == null ? null : Enumerable.Range(0, 6).Select(ix => item.PrevConfiguration[ix / 3, ix % 3]).Join("");
        }

        for (int i = buttonPresses.Count - 2; i >= 0; i--)
        {
            buttons[buttonPresses[i]].OnInteract();
            yield return new WaitForSeconds(.5f);
        }

        submitButton.OnInteract();
        yield return new WaitForSeconds(.1f);
    }
}

