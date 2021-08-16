using UnityEngine;
using System.Linq;
using Rnd = UnityEngine.Random;
using System.Collections;
using System;
using KModkit;

public class trickOrTreatScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombModule bombModule;
    public Material[] costumes;
    public Renderer costumeRender;
    public Renderer closeDoorRender;
    public Renderer openDoorRender;

    private int _costumes;
    private int _stages;
    private int _dingDong;
    private float _doorDong;
    private float _doorDongGuest;
    private int _running = 1;
    private int _runningGuest = 0;
    private int _stagesSolve;
    private bool isActive;
    private bool clock;
    private bool door;
    private int _displayedCostume;
    private int _costumeIndex;

    public KMSelectable trick;
    public KMSelectable treat;
    public KMSelectable table;
    public KMSelectable doorknob;
    public GameObject openDoor;
    public GameObject closeDoor;
    public GameObject TableE;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool _isSolved;
    public static string[] ignoredModules = null;

    private bool isRunning = false;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(Waiting());
        moduleId = moduleIdCounter++;
        bombModule.OnActivate += Activate;
        door = true;
        clock = false;
        _doorDong = Rnd.Range(10,60);
        _running = 1;
        _runningGuest = 0;
        _stagesSolve = 10;
        Debug.LogFormat("[Trick Or Treat #{0}] {1}", moduleId, _stagesSolve);
        audio = GetComponent<KMAudio>();
        trick.OnInteract += delegate { Trick(); return true; };
        treat.OnInteract += delegate { Treat(); return true; };
        table.OnInteract += delegate { Table(); return true; };
        doorknob.OnInteract += delegate { Doorknob(); return false; };
    }
    
        // Update is called once per frame
    void Update()
    {
        
        if (door == true)
        {
            closeDoor.gameObject.SetActive(true);
            openDoor.gameObject.SetActive(false);
        }
        else
        {
            closeDoor.gameObject.SetActive(false);
            openDoor.gameObject.SetActive(true);
        }
        if (_stages >= _stagesSolve)
        {
            bombModule.HandlePass();
        }
        if (!isRunning) StartCoroutine(Waiting());

        if(!clock) TableE.gameObject.SetActive(false);
        if(clock) TableE.gameObject.SetActive(true);
    }

    void Activate()
    {
        isActive = true;
    }


    void Trick()
    {
        if (_costumeIndex == 11)
        {
            _stages = (_stages + 1);
            Softlock();
        }
        else
        {
            bombModule.HandleStrike();
            _stages = 0;
            Softlock();
        }
    }

    void Treat()
    {
        if (_costumeIndex <= 10)
        {
            _stages = _stages + 1;
            Softlock();

        }
        else
        {
            bombModule.HandleStrike();
            _stages = 0;
            Softlock();
        }
    }

    void Table()
    {
        if (_costumeIndex == 12)
        {
            bombModule.HandlePass();
            _doorDong = -1;
            StopCoroutine(Waiting());
            door = true;
            audio.PlaySoundAtTransform("solve", transform);
        }
        else
        {
            bombModule.HandleStrike();
            _stages = 0;
            Softlock();
        }
    }

    void Doorknob()
    {
        audio.PlaySoundAtTransform("answer", doorknob.transform);
        StopCoroutine(Waiting());
        if (_dingDong > 0)
        {
            _doorDong = -1;
            door = false;
        }
        else
        {
            bombModule.HandleStrike();
        }
    }

    void dingDong()
    {
        audio.PlaySoundAtTransform("doorbell", transform);
        Debug.LogFormat("[Trick Or Treat #{0}] Ding Dong", moduleId);
    }

    void PickCostume()
    {
        if (!(_stages >= _stagesSolve))
        {
            _costumeIndex = Rnd.Range(0, 13);
            costumeRender.material = costumes[_costumeIndex];
            Debug.LogFormat("[Trick Or Treat #{0}] Costume is {1}. {2}", moduleId, costumes[_costumeIndex], _stages);
        }
    }
    
    IEnumerator Waiting()
    {
        isRunning = true;
        yield return new WaitForSecondsRealtime(10.0f);
        dingDong();
        PickCostume();
        _doorDong = 10;
        clock = true;
        while (clock)
        {
            _dingDong = 1;
            yield return new WaitForSecondsRealtime(1.0f);
            _doorDong -= 1;
            if (_doorDong == 0)
            {
                bombModule.HandleStrike();
                _stages = 0;
                Softlock();
            }
        }
    }

    void Softlock()
    {
        StopCoroutine(Waiting());
        _doorDong = Rnd.Range(10, 60);
        _dingDong = 0;
        door = true;
        clock = false;
        isRunning = false;
    }
}
