using UnityEngine;
using System.Linq;
using Rnd = UnityEngine.Random;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using KModkit;

public class trickOrTreatScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombModule bombModule;
    public Material[] costumes;
    public Renderer costumeRender;
    public Renderer closeDoorRender;
    public Renderer openDoorRender;

    public KMSelectable trick;
    public KMSelectable treat;
    public KMSelectable table;
    public KMSelectable doorknob;
    public GameObject sky;
    public GameObject openDoor;
    public GameObject closeDoor;
    public GameObject TableE;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool _isSolved;
	
	bool Activated = false, DoorbellRinging = false, DoorOpened = false;
	int StageAmount, StageAdvanced = 0, CostumeNumber;
	Coroutine TimeIntervals;
	
	#pragma warning disable 0649
    private bool TwitchPlaysActive;
    #pragma warning restore 0649
	float waitTime = 5f;
	
	void Awake()
	{
		trick.OnInteract += delegate { Trick(); return false; };
        treat.OnInteract += delegate { Treat(); return false; };
        table.OnInteract += delegate { Table(); return false; };
        doorknob.OnInteract += delegate { Doorknob(); return false; };
		bombModule.OnActivate += TrickOrTreatOnTP;
	}
	
	void Start()
    {
		moduleId = moduleIdCounter++;
		StageAmount = Rnd.Range(7,11);
		Debug.LogFormat("[Trick Or Treat #{0}] Correctly handled trick or treaters needed*: {1}", moduleId, StageAmount.ToString());
		trick.gameObject.SetActive(false);
		treat.gameObject.SetActive(false);
		closeDoor.gameObject.SetActive(true);
		openDoor.gameObject.SetActive(false);
		TableE.gameObject.SetActive(false);
		costumeRender.gameObject.SetActive(false);
		sky.gameObject.SetActive(false);
	}
	
	void TrickOrTreatOnTP()
	{
		waitTime = TwitchPlaysActive ? 7.5f : 5f;
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
				if (StageAdvanced > 3 && Rnd.Range(0,13) == 0)
				{
					CostumeNumber = 12;
					Debug.LogFormat("[Trick Or Treat #{0}] Is that KANYE!? Give him ALL THE CANDY.", moduleId);
				}
				
				else
				{
					if (Rnd.Range(0,3) == 0)
					{
						CostumeNumber = 11;
						Debug.LogFormat("[Trick Or Treat #{0}] That is just a regular dude. No candy for that dude.", moduleId);
					}
					
					else
					{
						CostumeNumber = Rnd.Range(0,11);
						Debug.LogFormat("[Trick Or Treat #{0}] Cool costume. A candy for you", moduleId);
					}
				}
				audio.PlaySoundAtTransform("answer", doorknob.transform);
				costumeRender.material = costumes[CostumeNumber];
				closeDoor.gameObject.SetActive(false);
				openDoor.gameObject.SetActive(true);
				costumeRender.gameObject.SetActive(true);
				sky.gameObject.SetActive(true);
			}
		}
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
				if (CostumeNumber == 12)
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
				if (CostumeNumber == 12)
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
			if (CostumeNumber == 12)
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
		closeDoor.gameObject.SetActive(true);
		openDoor.gameObject.SetActive(false);
		TableE.gameObject.SetActive(false);
		costumeRender.gameObject.SetActive(false);
		sky.gameObject.SetActive(false);
	}
	
	void SoftReset()
	{
		DoorOpened = false;
		closeDoor.gameObject.SetActive(true);
		openDoor.gameObject.SetActive(false);
		TableE.gameObject.SetActive(false);
		costumeRender.gameObject.SetActive(false);
		sky.gameObject.SetActive(false);
		TimeIntervals = StartCoroutine(TimerVariable());
	}
	
	IEnumerator TimerVariable()
	{
		yield return new WaitForSecondsRealtime(Rnd.Range(10f, 30f));
		DoorbellRinging = true;
		Debug.LogFormat("[Trick Or Treat #{0}] The doorbell is ringing. Who could it be?", moduleId);
		TableE.gameObject.SetActive(true);
		audio.PlaySoundAtTransform("doorbell", doorknob.transform);
		yield return new WaitForSecondsRealtime(waitTime);
		audio.PlaySoundAtTransform("doorbell", doorknob.transform);
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
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To press the door/trick/treat/table button, use !{0} door/trick/treat/table";
    #pragma warning restore 414
	
	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*door\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (Activated && (!DoorbellRinging || DoorOpened))
			{
				yield return "sendtochaterror I can't allow you to use the door at the current situation. Command ignored.";
				yield break;
			}
			
			doorknob.OnInteract();
		}
		
		if (Regex.IsMatch(command, @"^\s*treat\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Activated || !DoorOpened)
			{
				yield return "sendtochaterror I can't allow you to use the treat crescent at the current situation. Command ignored.";
				yield break;
			}
			
			treat.OnInteract();
		}
		
		if (Regex.IsMatch(command, @"^\s*trick\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Activated || !DoorOpened)
			{
				yield return "sendtochaterror I can't allow you to use the trick crescent at the current situation. Command ignored.";
				yield break;
			}
			
			trick.OnInteract();
		}
		
		if (Regex.IsMatch(command, @"^\s*table\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Activated || !DoorOpened)
			{
				yield return "sendtochaterror I can't allow you to use the table with the candy bowl at the current situation. Command ignored.";
				yield break;
			}
			
			table.OnInteract();
		}
	}
}