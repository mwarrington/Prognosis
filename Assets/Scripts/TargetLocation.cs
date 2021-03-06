﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetLocation : MonoBehaviour
{
    public GameObject TargetMenu;
   	public BoxCollider2D LockRecallButton;
	public GameObject spriteHighlight;
	public AudioClip mhuRecall, mhuLock, clickDisabled;
	private AudioSource audioSource;

    public SpriteRenderer spriteRenderer;
	public GameObject lockButtonRenderer, recallButtonRenderer;
    public ProfessionalsMenu TheProfMenu;
	public ProfessionalSlot[] ProSlots;
    public float STIRate,
                 TeenPregRate,
                 CrimeRate,
                 STIEffectPerTurn,
                 TeenPregEffectPerTurn,
                 CrimeRateEffectPerTurn;
    public bool Locked;


    public bool Active
    {
        get
        {
            return _active;
        }

        set
        {
            if (_active != value)
            {
                if (value)
                {
                    TargetMenu.SetActive(value);
					spriteHighlight.SetActive (value);
                    PM.Active = true;
                }
                else
                {
                    TargetMenu.SetActive(value);
					spriteHighlight.SetActive (value);

                }

                _active = value;
            }
        }
    }
    private bool _active;

    private List<TargetLocation> _otherTargetLocs = new List<TargetLocation>();
    private ProfessionalsMenu PM;
    private GameManager _gameManager;
    public int _currentTurn;


	private Color defaultColor;
	public Color CurrentColor;
    public SpriteRenderer HealthyGlow;

    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        TheProfMenu = FindObjectOfType<ProfessionalsMenu>();
        PM = FindObjectOfType<ProfessionalsMenu>();
        _otherTargetLocs.AddRange(FindObjectsOfType<TargetLocation>());
        _otherTargetLocs.Remove(this);
		defaultColor = spriteRenderer.color;
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = Resources.Load(name) as AudioClip;
		audioSource.Play();
        UpdateColor(1 - ((STIRate + TeenPregRate + CrimeRate) / 300));
        UpdateGlow();
    }

    void Update()
    {
        if (_currentTurn != _gameManager.CurrentTurn)
        {
            if (Locked)
                recallButtonRenderer.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void UpdateColor(float healthAverage)
    {
        if (healthAverage < 0.5f)
        {
            CurrentColor = new Color(1, healthAverage * 2, 0);
        }
        else if (healthAverage == 0.5f)
        {
            CurrentColor = new Color(1, 1, 0);
        }
        else if (healthAverage > 0.5f)
        {
            CurrentColor = new Color((1 - healthAverage) * 2, 1, 0);
        }

        spriteRenderer.color = CurrentColor;
    }

    private void UpdateGlow()
    {
        if (STIRate < 40 && TeenPregRate < 40 && CrimeRate < 40)
            HealthyGlow.enabled = true;
        else
            HealthyGlow.enabled = false;
    }

    public void SendMHU()
    {
        if (!Locked && _currentTurn != _gameManager.CurrentTurn)
        {
            _currentTurn = _gameManager.CurrentTurn;
            Locked = !Locked;
            audioSource.clip = mhuLock;
            audioSource.Play();
            lockButtonRenderer.SetActive(false);
            recallButtonRenderer.SetActive(true);
            recallButtonRenderer.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else if (Locked && _currentTurn == _gameManager.CurrentTurn)
        {
            audioSource.clip = clickDisabled;
            audioSource.Play();
        }
        else if (Locked && _currentTurn != _gameManager.CurrentTurn)
        {
            _currentTurn = _gameManager.CurrentTurn;
            Locked = !Locked;
            lockButtonRenderer.SetActive(true);
            recallButtonRenderer.SetActive(false);
            audioSource.clip = mhuRecall;
            audioSource.Play();
        }
        else
        { //if location is not locked and the current turn is  equal to the game manager's current turn
            _currentTurn = _gameManager.CurrentTurn;
            Locked = !Locked;
            audioSource.clip = mhuLock;
            audioSource.Play();
            lockButtonRenderer.SetActive(false);
            recallButtonRenderer.SetActive(true);
            recallButtonRenderer.GetComponent<SpriteRenderer>().color = new Color(.6f, .6f, .6f);
        }

        if (_currentTurn != _gameManager.CurrentTurn)
            recallButtonRenderer.GetComponent<SpriteRenderer>().color = Color.white;
    }
    

    public void HighlightOn()
    {
        spriteHighlight.SetActive(true);
    }

    public void HighlightOff()
    {
        spriteHighlight.SetActive(false);
    }

    public void Activate()
    {
        Active = true;
        //spriteHighlight.SetActive (true);
        foreach (TargetLocation tl in _otherTargetLocs)
        {
            tl.Active = false;
            //tl.spriteHighlight.SetActive (false);
        }
        TutorialToolTip currentTTT = FindObjectOfType<TutorialToolTip>();

        if (currentTTT)
        {
            if (currentTTT.name == "TryLocation(Clone)")
            {
                EventManager.TriggerEvent("Close");
            }
        }
    }

    public void UpdateValues()
    {
        //Initialization of local fields
        float AmbientBoost = 0,
              HealthBoost = 0,
              STIChange = 0,
              TeenPregChange = 0,
              CrimeRateChange = 0,
              BudgetChange = 0,
              EducationChange = 0,
              EducationModifier = 0;
        int STIChangeCount = 0,
            TeenPregChangeCount = 0,
            CrimeRateChangeCount = 0,
            BudgetChangeCount = -1,
            EducationChangeCount = 0;

        //Calculation of Education Modifier
        if (_gameManager.Education < 10)
            EducationModifier = 5;
        else if (_gameManager.Education < 30)
            EducationModifier = 0;
        else if (_gameManager.Education < 60)
            EducationModifier = -5;
        else if (_gameManager.Education < 99)
            EducationModifier = -10;
        else
            EducationModifier = -20;

        //Professional calculations
        for (int i = 0; i < ProSlots.Length; i++)
        {
            if (ProSlots[i].CurrentProfesional != null)
            {
                switch (ProSlots[i].CurrentProfesional.MyProfessionalType)
                {
                    case ProfessionalType.Doctor:
                        STIChange -= 20;
                        BudgetChange -= 6;
                        STIChangeCount++;
                        break;
                    case ProfessionalType.Nurse:
                        TeenPregChange -= 10;
                        STIChange -= 10;
                        BudgetChange -= 3;
                        STIChangeCount++;
                        TeenPregChangeCount++;
                        break;
                    case ProfessionalType.CommOrg:
                        CrimeRateChange -= 10;
                        EducationChange += 5;
                        BudgetChange -= 3;
                        CrimeRateChangeCount++;
                        EducationChangeCount++;
                        break;
                    case ProfessionalType.Politician:
                        AmbientBoost += 5;
                        BudgetChange += 5;
                        BudgetChangeCount++;
                        break;
                    case ProfessionalType.SocialWorker:
                        HealthBoost += 5;
                        EducationChange += 5;
                        BudgetChange -= 3;
                        EducationChangeCount++;
                        break;
                }
            }
        }

        //Assures that the professionals never make things worse due to financial modifier
        //At worst they will do nothing
        STIChange = Mathf.Clamp(STIChange, -100, 0);
        TeenPregChange = Mathf.Clamp(TeenPregChange, -100, 0);
        CrimeRateChange = Mathf.Clamp(CrimeRateChange, -100, 0);
        BudgetChange = Mathf.Clamp(BudgetChange, -100, 100);
        EducationChange = Mathf.Clamp(EducationChange, 0, 100);

        //I dont feel like explaining what this does and why. Suffice to say it's important.
        //EDIT: It must be possible, somehow, to get either of these values over 2 so this keeps it at 2 since that should be the max
        Mathf.Clamp(BudgetChangeCount, 0, 2);
        Mathf.Clamp(EducationChangeCount, 0, 2);

        //This debug log is useful for finding any problems with the professional effects
        //Debug.Log(STIChange + " - " + "(" + HealthBoost + " * " + STIChangeCount + ")");

        //Apply professional effects
        STIRate += STIChange - (HealthBoost * STIChangeCount);
        TeenPregRate += TeenPregChange - (HealthBoost * TeenPregChangeCount);
        CrimeRate += CrimeRateChange - (HealthBoost * CrimeRateChangeCount);
        _gameManager.Education += EducationChange + (AmbientBoost * EducationChangeCount);
        if (BudgetChangeCount != 1)
            _gameManager.Budget += BudgetChange + (AmbientBoost * BudgetChangeCount);
        else
            _gameManager.Budget += BudgetChange;

        //Apply per turn effects
        STIRate += Mathf.Clamp(STIEffectPerTurn + EducationModifier, 0, 100);
        TeenPregRate += Mathf.Clamp(TeenPregEffectPerTurn + EducationModifier, 0, 100);
        CrimeRate += Mathf.Clamp(CrimeRateEffectPerTurn + EducationModifier, 0, 100);

        float overallHealthRate = 1 - ((STIRate + TeenPregRate + CrimeRate) / 300);

        //Clamping final values
        STIRate = Mathf.Clamp(STIRate, 0, 100);
        TeenPregRate = Mathf.Clamp(TeenPregRate, 0, 100);
        CrimeRate = Mathf.Clamp(CrimeRate, 0, 100);
        _gameManager.Budget = Mathf.Clamp(_gameManager.Budget, 0, 100);
        _gameManager.Education = Mathf.Clamp(_gameManager.Education, 0, 100);

        UpdateColor(overallHealthRate);
        UpdateGlow();
    }
}