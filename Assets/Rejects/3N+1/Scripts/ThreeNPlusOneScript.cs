using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Collections.Generic;

public class ThreeNPlusOneScript : MonoBehaviour {

    //Bomb Sound and Info
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    //Buttons
    public KMSelectable[] NumberButtons;
    public KMSelectable SubmitButton;
    public KMSelectable CLRButton;
    public TextMesh DisplayText;
    public Animator[] Moving;
    public KMSelectable[] AllButtons;

    //Variables
    private string storedEntry = "";
    private int selectedNumber, Answer, Stage;
    static int moduleIdCounter = 1;
    int moduleId;
    bool isSolved = false;
    private float InteractionPunchIntensityModifier = .5f;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        SubmitButton.OnInteract += delegate { SubmitEntry(); return false; };
        CLRButton.OnInteract += delegate { ClearEntry(); return false; };

        for (int i = 0; i < AllButtons.Length; i++)
        {
            Animator anim = AllButtons[i].GetComponentInChildren<Animator>();
            string name = AllButtons[i].name;

            AllButtons[i].OnInteract += delegate ()
            {
                anim.SetTrigger("PushTrigger");
                return false;
            };
        }

        for (var i = 0; i < NumberButtons.Length; i++)
        {
            int j = i;
            NumberButtons[i].OnInteract += delegate { NumberInput(j); return false; };
        }
    }

    // Use this for initialization
    void Start ()
    {
        selectedNumber = Rnd.Range(2, 100);
        if (selectedNumber % 2 == 0) selectedNumber -= 1;
        if (selectedNumber == 27) selectedNumber -= 2;
        if (selectedNumber == 97) selectedNumber -= 2;
        DisplayText.text = selectedNumber.ToString();
        Stage = 0;
        Answer = cycleSize(selectedNumber)-1;
        Debug.Log(Answer);
    }

    void ClearEntry()
    {
        if (isSolved)
            return;

        Animator anim = Moving[11];
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, CLRButton.transform);
        CLRButton.AddInteractionPunch(InteractionPunchIntensityModifier);

        storedEntry = "";
        DisplayText.text = selectedNumber.ToString(); Debug.LogFormat("[3N+1 #{0}] Clearing Screen, Showing Current number {1}", moduleId, selectedNumber);
    }


    void SubmitEntry()
    {
        if (isSolved)
            return;

        Animator anim = Moving[10];
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SubmitButton.transform);
        SubmitButton.AddInteractionPunch(InteractionPunchIntensityModifier);
        if (storedEntry == Answer.ToString() && !isSolved)
        {
            for (int i = 0; i < 14; i++)
            {
                if ((storedEntry == Math.Pow(2, i).ToString() && Answer == Math.Pow(2, i)) || (storedEntry == "7" && Answer == 7) || (storedEntry == "3" && Answer == 3))
                {
                    if (Bomb.GetSolvedModuleNames().Count < Bomb.GetSolvableModuleNames().Count)
                        Audio.PlaySoundAtTransform("solve", transform);
                    isSolved = true;
                    Module.HandlePass();
                    Debug.LogFormat("[3N+1 #{0}] {1} is {2}. Completed All Stages. Module Solved.", moduleId, selectedNumber, Answer);
                }
            }
            if (!isSolved)
            {
                Debug.LogFormat("[3N+1 #{0}] {1} is {2}. Continuing...", moduleId, selectedNumber, Answer);
                Audio.PlaySoundAtTransform("Select", transform);
                selectedNumber = Answer;
                DisplayText.text = selectedNumber.ToString();
                storedEntry = "";

                Stage++;
                Answer = cycleSize(selectedNumber) - 1;
                Debug.Log(Answer);
            }
            
        }
        else
        {
            Debug.LogFormat("[3N+1 #{0}] Displayed number is {1}. You submitted {2}. I wanted {3}. Strike!", moduleId, selectedNumber, storedEntry, Answer);
            Module.HandleStrike();
            DisplayText.text = selectedNumber.ToString();
            storedEntry = "";
        }
        
        
    }

    void NumberInput(int number)
    {
        if (isSolved)
            return;
        Animator anim = Moving[number];
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, NumberButtons[number].transform);
        NumberButtons[number].AddInteractionPunch(InteractionPunchIntensityModifier);

        if (storedEntry.Length == 3)
           return;

        storedEntry = storedEntry + number;
        Debug.LogFormat("[3N+1 #{0}] Currently Displaying {1}.", moduleId, storedEntry);
        DisplayText.text = storedEntry;
    }

    int cycleSize(int x)
    {
        int cycle = 1;

        while (x != 1)
        {
            if (x % 2 == 0)
            { //if odd
                x = x/2;
            }
            else
            { //if even
                x = x*3+1;
            }
            ++cycle;
        }
        return cycle;

    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use the command !{0} submit ## to submit a two/one digit number.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
        Command = Command.Trim().ToUpper();
        string[] Parameters = Command.Split(' ');
        yield return null;
        if (Parameters[0] != "SUBMIT" || Parameters.Length != 2)
            yield return "sendtochaterror I don't understand";
        else if (Parameters[1].Length > 3 || Parameters[1].Length == 0)
            yield return "sendtochaterror I don't understand";
        else if (!(Parameters[1].Any(x => "0123456789".Contains(x))))
            yield return "sendtochaterror I don't understand";
        else
        {
            for (int i = 0; i < Parameters[1].Length; i++)
            {
                NumberButtons[int.Parse(Parameters[1][i].ToString())].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            SubmitButton.OnInteract();
        }
    }



    IEnumerator TwitchHandleForcedSolve()
    {
        while (!isSolved)
        {
            for (int i = 0; i < Answer.ToString().Length; i++)
            {
                NumberButtons[Answer.ToString()[i] - '0'].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            SubmitButton.OnInteract();
            yield return new WaitForSeconds(.5f);
        }
            
    }
}



