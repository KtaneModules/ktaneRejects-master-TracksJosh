using UnityEngine;
using Rnd = UnityEngine.Random;
using System.Collections;
using System.Text.RegularExpressions;

public class trickOrTreatScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule bombModule;
    public Material[] costumes;
    public Material secretariatmat;
    public Renderer costumeRender;
    public AudioSource HorseMusic;

    public KMSelectable trick;
    public KMSelectable treat;
    public KMSelectable table;
    public KMSelectable doorknob;
    public GameObject sky;
    public GameObject TableE;
    public GameObject DoorParent;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool _isSolved;

    bool Activated = false, DoorbellRinging = false, DoorOpened = false;
    int StageAmount, StageAdvanced = 0, CostumeNumber;
    Coroutine TimeIntervals;
    private Coroutine _doorAnimation;

    private bool _specialPerson;
    private bool _specialHorse;

#pragma warning disable 0649
    private bool TwitchPlaysActive;
#pragma warning restore 0649
    float waitTime = 5f;
    float delayMultiplier = 10f;

    void Awake()
    {
        trick.OnInteract += delegate { Trick(); return false; };
        treat.OnInteract += delegate { Treat(); return false; };
        table.OnInteract += delegate { Table(); return false; };
        doorknob.OnInteract += delegate { Doorknob(); return false; };
        bombModule.OnActivate += delegate () { waitTime = TwitchPlaysActive ? 7.5f : 5f; };
    }

    void Start()
    {
        moduleId = moduleIdCounter++;
        StageAmount = Rnd.Range(7, 11);
        Debug.LogFormat("[Trick Or Treat #{0}] Correctly handled trick or treaters needed*: {1}", moduleId, StageAmount.ToString());
        trick.gameObject.SetActive(false);
        treat.gameObject.SetActive(false);
        TableE.gameObject.SetActive(false);
        costumeRender.gameObject.SetActive(false);
        sky.gameObject.SetActive(false);
    }

    void Doorknob()
    {
        if (!_isSolved)
        {
            if (!Activated)
            {
                trick.gameObject.SetActive(true);
                treat.gameObject.SetActive(true);
                TimeIntervals = StartCoroutine(TimerVariable());
                Activated = true;
                Debug.LogFormat("[Trick Or Treat #{0}] The trick or treat session begins.", moduleId, StageAmount.ToString());
            }

            if (DoorbellRinging)
            {
                StopCoroutine(TimeIntervals);
                DoorbellRinging = false;
                DoorOpened = true;
                int rndSecretariat = Rnd.Range(0, 100);
                int rndKanye = Rnd.Range(0, 13);
                _specialPerson = rndKanye == 0 && StageAdvanced > 3;
                _specialHorse = rndSecretariat == 0 && StageAdvanced > 3;
                if (_specialPerson)
                {
                    CostumeNumber = 12;
                    Debug.LogFormat("[Trick Or Treat #{0}] Is that KANYE!? Give him ALL THE CANDY.", moduleId);
                }
                else if (_specialHorse)
                {
                    CostumeNumber = 0;
                    Debug.LogFormat("[Trick Or Treat #{0}] WHO'S THAT AT THE DOOR?!?!", moduleId);
                }
                else
                {
                    if (Rnd.Range(0, 3) == 0)
                    {
                        CostumeNumber = 11;
                        Debug.LogFormat("[Trick Or Treat #{0}] That is just a regular dude. No candy for that dude.", moduleId);
                    }

                    else
                    {
                        CostumeNumber = Rnd.Range(0, 11);
                        Debug.LogFormat("[Trick Or Treat #{0}] Cool costume. A candy for you.", moduleId);
                    }
                }
                if (_specialHorse)
                {
                    Audio.PlaySoundAtTransform("whosthatatthedoor", transform);
                    costumeRender.material = secretariatmat;
                    StartCoroutine(HorseSong());
                }
                else
                {
                    Audio.PlaySoundAtTransform("answer", doorknob.transform);
                costumeRender.material = costumes[CostumeNumber];
                }
                if (_doorAnimation != null)
                    StopCoroutine(_doorAnimation);
                _doorAnimation = StartCoroutine(RotateDoor(true));
                costumeRender.gameObject.SetActive(true);
                sky.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator HorseSong()
    {
        yield return new WaitForSeconds(1.5f);
        HorseMusic.Play();
        yield break;
    }

    void Trick()
    {
        if (DoorOpened)
        {
            if (CostumeNumber == 11)
            {
                StageAdvanced++;
                if (StageAdvanced == StageAmount)
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] NO CANDY. It means 0 CANDY! You have dealt with {1} trick or treater(s) successfully. That should be all of them. Module solved.", moduleId, StageAdvanced.ToString());
                    PassTheModule();
                }

                else
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] Only costume wearing people get the candy. Good for you for giving 0 candy to that. You have dealt with {1} trick or treater(s) successfully.", moduleId, StageAdvanced.ToString());
                    SoftReset();
                }
            }

            else
            {
                if (_specialPerson)
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] YOU DID NOT GIVE THE SPECIAL PERSON ANY CANDY!? YOU ARE A MONSTER! A MONSTER!!! THAT IS A STRIKE!!!!!!", moduleId, StageAdvanced.ToString());
                }

                else
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] You ignored the cool costume(s). A strike for that kind of mistake.", moduleId, StageAdvanced.ToString());
                }

                bombModule.HandleStrike();
                SoftReset();
            }
        }
    }

    void Treat()
    {
        if (DoorOpened)
        {
            if (CostumeNumber < 11)
            {
                StageAdvanced++;
                if (StageAdvanced == StageAmount)
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] You looked into the abyss. You saw 10 costumes. Only 10 trick or treaters exists in this plane. Ignoring the previous statement, you have dealt with {1} trick or treater(s) successfully. That should be all of them. Module solved.", moduleId, StageAdvanced.ToString());
                    PassTheModule();
                }

                else
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] Cool costumes deserves some delicious candy as a gift. You have dealt with {1} trick or treater(s) successfully.", moduleId, StageAdvanced.ToString());
                    SoftReset();
                }
            }

            else
            {
                if (_specialPerson)
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] I know you should give away some candy to the people that arrive, but you could have done the module faster if you just gave away all of them to that special person. Instead of solving, it struck instead.", moduleId, StageAdvanced.ToString());
                }

                else
                {
                    Debug.LogFormat("[Trick Or Treat #{0}] That is just a regular dude. Not even a single effort was put into that costume. A strike for that kind of mistake.", moduleId, StageAdvanced.ToString());
                }

                bombModule.HandleStrike();
                SoftReset();
            }
        }
    }

    void Table()
    {
        if (DoorOpened)
        {
            if (_specialPerson)
            {
                Debug.LogFormat("[Trick Or Treat #{0}] Special bowl of candy for a special person. You don't have to deal with other people know because your candy count is 0. Module solved.", moduleId, StageAdvanced.ToString());
                PassTheModule();
            }

            else
            {
                Debug.LogFormat("[Trick Or Treat #{0}] That was not a special person at all. You just gave away a big bowl of candy. Although, you have infinite number of candy filled bowls in this plane (hopefully not creating a paradox). Still, a strike for that kind of mistake.", moduleId);
                bombModule.HandleStrike();
                SoftReset();
            }
        }
    }

    void PassTheModule()
    {
        bombModule.HandlePass();
        _isSolved = true;
        trick.gameObject.SetActive(false);
        treat.gameObject.SetActive(false);
        if (_doorAnimation != null)
            StopCoroutine(_doorAnimation);
        _doorAnimation = StartCoroutine(RotateDoor(false));
        TableE.gameObject.SetActive(false);
    }

    void SoftReset()
    {
        DoorOpened = false;
        if (_doorAnimation != null)
            StopCoroutine(_doorAnimation);
        _doorAnimation = StartCoroutine(RotateDoor(false));
        TableE.gameObject.SetActive(false);
        TimeIntervals = StartCoroutine(TimerVariable());
    }

    private void OnDestroy()
    {
        if (HorseMusic.isPlaying)
            HorseMusic.Stop();
    }

    IEnumerator TimerVariable()
    {
        var delayTime = Rnd.Range(1f, 3f) * delayMultiplier;
        yield return new WaitForSecondsRealtime(delayTime);
        DoorbellRinging = true;
        Debug.LogFormat("[Trick Or Treat #{0}] The doorbell is ringing. Who could it be?", moduleId);
        TableE.gameObject.SetActive(true);
        Audio.PlaySoundAtTransform("doorbell", doorknob.transform);
        yield return new WaitForSecondsRealtime(waitTime);
        Audio.PlaySoundAtTransform("doorbell", doorknob.transform);
        yield return new WaitForSecondsRealtime(waitTime);
        if (!DoorOpened)
        {
            Debug.LogFormat("[Trick Or Treat #{0}] You did not answer the door. That could have been anyone, even the creator of this existance. All creatures living in this place are safe, you know? A strike for that kind of mistake.", moduleId);
            bombModule.HandleStrike();
            DoorbellRinging = false;
            TableE.gameObject.SetActive(false);
            TimeIntervals = StartCoroutine(TimerVariable());
        }
    }

    private IEnumerator RotateDoor(bool open)
    {
        var duration = 0.4f;
        var elapsed = 0f;
        var curPos = DoorParent.transform.localEulerAngles;
        while (elapsed < duration)
        {
            DoorParent.transform.localEulerAngles = new Vector3(0f, 0f, Easing.InOutQuad(elapsed, curPos.z, open ? 90f : 0f, duration));
            yield return null;
            elapsed += Time.deltaTime;
        }
        if (HorseMusic.isPlaying && !open)
            HorseMusic.Stop();
        DoorParent.transform.localEulerAngles = new Vector3(0f, 0f, open ? 90f : 0f);
        if (!open)
        {
            costumeRender.gameObject.SetActive(false);
            sky.gameObject.SetActive(false);
        }
    }

    //twitch plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To press the door/trick/treat/table button, use !{0} door/trick/treat/table";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*((?<door>door)|(?<trick>trick)|(?<treat>treat)|(?<treat>)|table)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;
        yield return null;
        if (m.Groups["door"].Success)
        {
            if (Activated && (!DoorbellRinging || DoorOpened))
            {
                yield return "sendtochaterror I can't allow you to use the door at the current situation. Command ignored.";
                yield break;
            }
            doorknob.OnInteract();
            yield break;
        }
        if (m.Groups["treat"].Success)
        {
            if (!Activated || !DoorOpened)
            {
                yield return "sendtochaterror I can't allow you to use the treat crescent at the current situation. Command ignored.";
                yield break;
            }
            treat.OnInteract();
            yield break;
        }
        if (m.Groups["trick"].Success)
        {
            if (!Activated || !DoorOpened)
            {
                yield return "sendtochaterror I can't allow you to use the trick crescent at the current situation. Command ignored.";
                yield break;
            }
            trick.OnInteract();
            yield break;
        }
        if (!Activated || !DoorOpened)
        {
            yield return "sendtochaterror I can't allow you to use the table with the candy bowl at the current situation. Command ignored.";
            yield break;
        }
        table.OnInteract();
        yield break;
    }

    private void TwitchHandleForcedSolve()
    {
        StartCoroutine(Autosolve());
    }

    private IEnumerator Autosolve()
    {
        delayMultiplier = 3f;
        if (!Activated)
            doorknob.OnInteract();
        while (!_isSolved)
        {
            if (!DoorbellRinging)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            yield return new WaitForSeconds(0.5f);
            doorknob.OnInteract();
            yield return new WaitForSeconds(1.5f);
            if (CostumeNumber == 12)
            {
                table.OnInteract();
                yield break;
            }
            if (CostumeNumber < 11)
            {
                if (_specialHorse)
                    yield return new WaitForSeconds(6f);
                treat.OnInteract();
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            trick.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}