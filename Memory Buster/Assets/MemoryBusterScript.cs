using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class MemoryBusterScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    public KMSelectable[] buttonSelectables;
    public MeshRenderer[] buttons;
    public TextMesh[] labels;
    public MeshRenderer display;
    public MeshRenderer[] stageLights;

    public Material[] buttonColors;
    private Material[] rndButtonColors = new Material[4];
    private String[] rndButtonColorStrings = new String[4];

    public Color[] labelColors;
    private Color[] rndLabelColors = new Color[4];
    private String[] rndLabelColorStrings = new String[4];
    private String[] labelTexts = new String[4];

    public Material[] displayColors;
    private Material displayColor;
    private String displayColorStr = "";

    public Material unlit;
    public Material lit;
    public Material darkGrey;
    public Material black;

    private List<String> pressedButtons = new List<String>();
    private List<String> stage1ButtonInfo = new List<String>();
    private List<String> stage2ButtonInfo = new List<String>();
    private List<String> stage3ButtonInfo = new List<String>();
    private List<String> stage4ButtonInfo = new List<String>();
    private List<String> stage5ButtonInfo = new List<String>();
    private List<String> displayInfo = new List<String>();

    string[] sounds = { "stage 1", "stage 2", "stage 3", "stage 4", "stage 5", "solve"};
    /*
     * INFO KEY:
     * E.G: 1R1R
     * POSITION 1, RED BUTTON, READS 1, RED LABEL
     */

    private int stage = 0;

    int animFrame = 0;
    int animFrameMax = 0;
    bool animating = false;

    private int clickDelay = 0;

   void Awake () {
      ModuleId = ModuleIdCounter++;
      /*
      foreach (KMSelectable object in keypad) {
          object.OnInteract += delegate () { keypadPress(object); return false; };
      }
      */

        buttonSelectables[0].OnInteract += delegate () { CheckPress(0); return false; };
        buttonSelectables[1].OnInteract += delegate () { CheckPress(1); return false; };
        buttonSelectables[2].OnInteract += delegate () { CheckPress(2); return false; };
        buttonSelectables[3].OnInteract += delegate () { CheckPress(3); return false; };

    }

   void Start () {
        for (int i = 0; i < 6; i++)
            stageLights[i].material = unlit;
        DefaultState();
        NewStage();
   }

   void Update () {
        if (clickDelay > 0)
            clickDelay--;
        //warning you are about to see the most inefficient animation code ever
        //i will optimize it one day but i am not that skilled yet and if its not broken then dont fix it
        if (animFrame < animFrameMax)
            animFrame++;
        if (animating)
        {
            switch (animFrame)
            {
                case 6:
                    buttons[3].material = darkGrey;
                    labels[3].color = Color.white;
                    if (!ModuleSolved)
                        labels[3].text = "?";
                    else
                        labels[3].text = "E";
                    display.material = black;
                    break;
                case 12:
                    buttons[2].material = darkGrey;
                    labels[2].color = Color.white;
                    if (!ModuleSolved)
                        labels[2].text = "?";
                    else
                        labels[2].text = "C";
                    break;
                case 18:
                    buttons[1].material = darkGrey;
                    labels[1].color = Color.white;
                    if (!ModuleSolved)
                        labels[1].text = "?";
                    else
                        labels[1].text = "I";
                    break;
                case 24:
                    buttons[0].material = darkGrey;
                    labels[0].color = Color.white;
                    if (!ModuleSolved)
                        labels[0].text = "?";
                    else
                    {
                        labels[0].text = "N";
                        display.material = buttonColors[3]; //green
                    }
                    break;
                case 45:
                    display.material = displayColor;
                    AnimateButton(0);
                    break;
                case 51:
                    AnimateButton(1);
                    break;
                case 57:
                    AnimateButton(2);
                    break;
                case 63:
                    AnimateButton(3);
                    break;
            }
        }
        if(animFrame == animFrameMax)
        {
            animFrameMax = 0;
            animFrame = 0;
            animating = false;
        }
    }
    void CheckPress(int index)
    {
        buttonSelectables[index].AddInteractionPunch();
        if (ModuleSolved || animating)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttonSelectables[index].transform);

        String check = "";
        switch (stage)
        {
            case 1:
                switch (displayColorStr)
                {
                    case "red": //first pos
                        if (index == 0)
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "blue": //third pos
                        if (index == 2)
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "yellow": //yellow button
                        if (rndButtonColorStrings[index].Equals("yellow"))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "green": //yellow label
                        if (rndLabelColorStrings[index].Equals("yellow"))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "orange": //label 4
                        if (labelTexts[index].Equals("4"))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "purple": //label 4
                        if (labelTexts[index].Equals("4"))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                }
                break;
            case 2:
                switch (displayColorStr)
                {
                    case "red": //stage 1 label
                        if (pressedButtons.ElementAt(0).Substring(2,1).Equals(labelTexts[index]))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "blue": //stage 1 position
                        if (pressedButtons.ElementAt(0).Substring(0,1).Equals((index+1).ToString()))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "yellow": //first pos
                        if (index == 0)
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "green": //color green
                        if (rndButtonColorStrings[index].Equals("green"))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "orange": //stage 1 label 4 color
                        for(int i = 0; i < 4; i++)
                        {
                            check = stage1ButtonInfo.ElementAt(i);
                            if (check.Substring(2, 1).Equals("4"))
                                i = 4; //flag
                        }
                        if (check.Substring(1,1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0,1)))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "purple": //stage 1 green label pos
                        for(int i = 0; i < 4; i++)
                        {
                            check = stage1ButtonInfo.ElementAt(i);
                            if (check.Substring(3, 1).Equals("G"))
                                i = 4;
                        }
                        if (check.Substring(0,1).Equals((index+1).ToString()))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                }
                break;
            case 3:
                switch (displayColorStr)
                {
                    case "red": //stage 1 label color
                        if (pressedButtons.ElementAt(0).Substring(3, 1).EqualsIgnoreCase(rndLabelColorStrings[index].Substring(0,1)))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "blue": //stage 2 ruleset
                        switch (displayInfo.ElementAt(1))
                        {
                            case "red":
                                if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue":
                                if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (index == 0)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (rndButtonColorStrings[index].Equals("green"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "orange":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(2, 1).Equals("4"))
                                        i = 4; //flag
                                }
                                if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(3, 1).Equals("G"))
                                        i = 4;
                                }
                                if (check.Substring(0, 1).Equals(index + 1))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                        }
                        break;
                    case "yellow": //stage 2 button color
                        if (pressedButtons.ElementAt(1).Substring(1,1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0,1)))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "green": //stage 1 ruleset
                        switch(displayInfo.ElementAt(0))
                        {
                            case "red":
                                if (index == 0)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue":
                                if (index == 2)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (rndButtonColorStrings[index].Equals("yellow"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (rndLabelColorStrings[index].Equals("yellow"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "orange":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                        }
                        break;
                    case "orange": //stage 1 label 4 button color
                        for (int i = 0; i < 4; i++)
                        {
                            check = stage1ButtonInfo.ElementAt(i);
                            if (check.Substring(2, 1).Equals("4"))
                                i = 4; //flag
                        }
                        if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "purple": //stage 2 ruleset
                        switch (displayInfo.ElementAt(1))
                        {
                            case "red":
                                if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue":
                                if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (index == 0)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (rndButtonColorStrings[index].Equals("green"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "orange":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(2, 1).Equals("4"))
                                        i = 4; //flag
                                }
                                if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(3, 1).Equals("G"))
                                        i = 4;
                                }
                                if (check.Substring(0, 1).Equals((index + 1).ToString()))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                        }
                        break;
                }
                break;
            case 4:
                switch (displayColorStr)
                {
                    case "red": //stage 3 green label label number
                        for (int i = 0; i < 4; i++)
                        {
                            check = stage3ButtonInfo.ElementAt(i);
                            if (check.Substring(3, 1).Equals("G"))
                                i = 4; //flag
                        }
                        if (check.Substring(2, 1).Equals(labelTexts[index]))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "blue": //stage 3 label color
                        if (pressedButtons.ElementAt(2).Substring(3,1).EqualsIgnoreCase(rndLabelColorStrings[index].Substring(0,1)))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "yellow": //stage 3 button color
                        if (pressedButtons.ElementAt(2).Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "green": //third pos
                        if(index == 2)
                        {
                            LogPress(index);
                            NewStage();
                        }
                        break;
                    case "orange": //stage 3 green button pos
                        for (int i = 0; i < 4; i++)
                        {
                            check = stage3ButtonInfo.ElementAt(i);
                            if (check.Substring(1, 1).Equals("G"))
                                i = 4; //flag
                        }
                        if (check.Substring(0, 1).Equals((index+1).ToString()))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "purple": //label 3
                        if(labelTexts[index].Equals("3"))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        break;
                }
                break;
            case 5:
                switch (displayColorStr)
                {
                    case "red": //stage 4 ruleset
                        switch (displayInfo.ElementAt(3))
                        {
                            case "red":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage3ButtonInfo.ElementAt(i);
                                    if (check.Substring(3, 1).Equals("G"))
                                        i = 4; //flag
                                }
                                if (check.Substring(2, 1).Equals(labelTexts[index]))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue":
                                if (pressedButtons.ElementAt(2).Substring(3, 1).EqualsIgnoreCase(rndLabelColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (pressedButtons.ElementAt(2).Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (index == 2)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                break;
                            case "orange":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage3ButtonInfo.ElementAt(i);
                                    if (check.Substring(1, 1).Equals("G"))
                                        i = 4; //flag
                                }
                                if (check.Substring(0, 1).Equals((index + 1).ToString()))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple":
                                if (labelTexts[index].Equals("3"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                break;
                        }
                        break;
                    case "blue": //stage 4 pos
                        if (pressedButtons.ElementAt(3).Substring(0, 1).Equals((index+1).ToString()))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "yellow": //label 4
                        if (labelTexts[index].Equals("4"))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        else
                            Reset();
                        break;
                    case "green": //second pos
                        if (index == 1)
                        {
                            LogPress(index);
                            NewStage();
                        }
                        break;
                    case "orange": //stage 2 ruleset
                        switch (displayInfo.ElementAt(1))
                        {
                            case "red":
                                if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue":
                                if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (index == 0)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (rndButtonColorStrings[index].Equals("green"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "orange":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(2, 1).Equals("4"))
                                        i = 4; //flag
                                }
                                if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple":
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(3, 1).Equals("G"))
                                        i = 4;
                                }
                                if (check.Substring(0, 1).Equals(index + 1))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                        }
                        break;
                    case "purple": //stage 2 label number
                        if (pressedButtons.ElementAt(1).Substring(2,1).Equals(labelTexts[index]))
                        {
                            LogPress(index);
                            NewStage();
                        }
                        break;
                }
                break;
            case 6:
                switch (displayColorStr)
                {
                    case "red": //stage 5 ruleset
                        switch (displayInfo.ElementAt(4))
                        {
                            case "red":
                                switch (displayInfo.ElementAt(3))
                                {
                                    case "red":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage3ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(2).Substring(3, 1).EqualsIgnoreCase(rndLabelColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (pressedButtons.ElementAt(2).Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (index == 2)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage3ButtonInfo.ElementAt(i);
                                            if (check.Substring(1, 1).Equals("G"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        if (labelTexts[index].Equals("3"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        break;
                                }
                                break;
                            case "blue":
                                if (pressedButtons.ElementAt(3).Substring(0, 1).Equals((index + 1).ToString()))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (index == 1)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                break;
                            case "orange":
                                switch (displayInfo.ElementAt(1))
                                {
                                    case "red":
                                        if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndButtonColorStrings[index].Equals("green"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(2, 1).Equals("4"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4;
                                        }
                                        if (check.Substring(0, 1).Equals(index + 1))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                            case "purple":
                                if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                break;
                        }
                        break;
                    case "blue": //stage 5 ruleset
                        switch (displayInfo.ElementAt(4))
                        {
                            case "red":
                                switch (displayInfo.ElementAt(3))
                                {
                                    case "red":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage3ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(2).Substring(3, 1).EqualsIgnoreCase(rndLabelColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (pressedButtons.ElementAt(2).Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (index == 2)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage3ButtonInfo.ElementAt(i);
                                            if (check.Substring(1, 1).Equals("G"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        if (labelTexts[index].Equals("3"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        break;
                                }
                                break;
                            case "blue":
                                if (pressedButtons.ElementAt(3).Substring(0, 1).Equals((index + 1).ToString()))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (index == 1)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                break;
                            case "orange":
                                switch (displayInfo.ElementAt(1))
                                {
                                    case "red":
                                        if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndButtonColorStrings[index].Equals("green"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(2, 1).Equals("4"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4;
                                        }
                                        if (check.Substring(0, 1).Equals(index + 1))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                            case "purple":
                                if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                break;
                        }
                        break;
                    case "yellow": //stage 1 ruleset
                        switch (displayInfo.ElementAt(0))
                        {
                            case "red":
                                if (index == 0)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue":
                                if (index == 2)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (rndButtonColorStrings[index].Equals("yellow"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (rndLabelColorStrings[index].Equals("yellow"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "orange":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                        }
                        break;
                    case "green": //stage 1 ruleset
                        switch (displayInfo.ElementAt(0))
                        {
                            case "red":
                                if (index == 0)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue":
                                if (index == 2)
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "yellow":
                                if (rndButtonColorStrings[index].Equals("yellow"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green":
                                if (rndLabelColorStrings[index].Equals("yellow"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "orange":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple":
                                if (labelTexts[index].Equals("4"))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                        }
                        break;
                    case "orange": //stage 3 ruleset
                        switch (displayInfo.ElementAt(2))
                        {
                            case "red": //stage 1 label color
                                if (pressedButtons.ElementAt(0).Substring(3, 1).EqualsIgnoreCase(rndLabelColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue": //stage 2 ruleset
                                switch (displayInfo.ElementAt(1))
                                {
                                    case "red":
                                        if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndButtonColorStrings[index].Equals("green"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(2, 1).Equals("4"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4;
                                        }
                                        if (check.Substring(0, 1).Equals(index + 1))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                            case "yellow": //stage 2 button color
                                if (pressedButtons.ElementAt(1).Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green": //stage 1 ruleset
                                switch (displayInfo.ElementAt(0))
                                {
                                    case "red":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (index == 2)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (rndButtonColorStrings[index].Equals("yellow"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndLabelColorStrings[index].Equals("yellow"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        if (labelTexts[index].Equals("4"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        if (labelTexts[index].Equals("4"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                            case "orange": //stage 1 label 4 button color
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(2, 1).Equals("4"))
                                        i = 4; //flag
                                }
                                if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple": //stage 2 ruleset
                                switch (displayInfo.ElementAt(1))
                                {
                                    case "red":
                                        if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndButtonColorStrings[index].Equals("green"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(2, 1).Equals("4"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4;
                                        }
                                        if (check.Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                        }
                        break;
                    case "purple": //stage 3 ruleset
                        switch (displayInfo.ElementAt(2))
                        {
                            case "red": //stage 1 label color
                                if (pressedButtons.ElementAt(0).Substring(3, 1).EqualsIgnoreCase(rndLabelColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "blue": //stage 2 ruleset
                                switch (displayInfo.ElementAt(1))
                                {
                                    case "red":
                                        if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndButtonColorStrings[index].Equals("green"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(2, 1).Equals("4"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4;
                                        }
                                        if (check.Substring(0, 1).Equals(index + 1))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                            case "yellow": //stage 2 button color
                                if (pressedButtons.ElementAt(1).Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "green": //stage 1 ruleset
                                switch (displayInfo.ElementAt(0))
                                {
                                    case "red":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (index == 2)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (rndButtonColorStrings[index].Equals("yellow"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndLabelColorStrings[index].Equals("yellow"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        if (labelTexts[index].Equals("4"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        if (labelTexts[index].Equals("4"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                            case "orange": //stage 1 label 4 button color
                                for (int i = 0; i < 4; i++)
                                {
                                    check = stage1ButtonInfo.ElementAt(i);
                                    if (check.Substring(2, 1).Equals("4"))
                                        i = 4; //flag
                                }
                                if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                {
                                    LogPress(index);
                                    NewStage();
                                }
                                else
                                    Reset();
                                break;
                            case "purple": //stage 2 ruleset
                                switch (displayInfo.ElementAt(1))
                                {
                                    case "red":
                                        if (pressedButtons.ElementAt(1).Substring(2, 1).Equals(labelTexts[index]))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "blue":
                                        if (pressedButtons.ElementAt(1).Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "yellow":
                                        if (index == 0)
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "green":
                                        if (rndButtonColorStrings[index].Equals("green"))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "orange":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(2, 1).Equals("4"))
                                                i = 4; //flag
                                        }
                                        if (check.Substring(1, 1).EqualsIgnoreCase(rndButtonColorStrings[index].Substring(0, 1)))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                    case "purple":
                                        for (int i = 0; i < 4; i++)
                                        {
                                            check = stage1ButtonInfo.ElementAt(i);
                                            if (check.Substring(3, 1).Equals("G"))
                                                i = 4;
                                        }
                                        if (check.Substring(0, 1).Equals((index + 1).ToString()))
                                        {
                                            LogPress(index);
                                            NewStage();
                                        }
                                        else
                                            Reset();
                                        break;
                                }
                                break;
                        }
                        break;
                }
                break;
        }
    }
    void Reset()
    {
        stage = 0;
        for (int i = 0; i < 6; i++)
            stageLights[i].material = unlit;
        pressedButtons.Clear();
        stage1ButtonInfo.Clear();
        stage2ButtonInfo.Clear();
        stage3ButtonInfo.Clear();
        stage4ButtonInfo.Clear();
        stage5ButtonInfo.Clear();
        displayInfo.Clear();
        GetComponent<KMBombModule>().HandleStrike();
        NewStage();
    }
    void NewStage() {
        clickDelay = 64;
        stage++;
        Debug.LogFormat("[Memory Buster #{0}] Proceeding to stage {1}.", ModuleId, stage);
        GenerateButtons();
        GenerateLabelColors();
        GenerateLabelTexts();
        GenerateDisplay();
        RememberInfo();
        StageAnim();
    }
    void StageAnim()
    {
        // Debug.LogFormat("[Memory Buster #{0}] Animating.", ModuleId);
        if (!ModuleSolved)
            animFrameMax = 64;
        else
            animFrameMax = 25;
        animating = true;
        for (int i = 0; i < stage; i++)
            stageLights[i].material = lit;
    }
    void SolveAnim()
    {
        ModuleSolved = true;
        StageAnim();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, display.transform); //PLACEHOLDER SOUND
        GetComponent<KMBombModule>().HandlePass();
    }
    void DefaultState()
    {
        for (int i = 0; i < 4; i++)
        {
            buttons[i].material = darkGrey;
            labels[i].text = "?";
            labels[i].color = Color.white;
        }
        display.material = black;
    }
    void AnimateButton(int index)
    {
        buttons[index].material = rndButtonColors[index];
        labels[index].text = labelTexts[index];
        labels[index].color = rndLabelColors[index];
    }
    void RememberInfo()
    {
        /*
         * INFO KEY:
         * E.G: 1R1R
         * POSITION 1, RED BUTTON, READS 1, RED LABEL
         */
        List<String> buttonInfoList = new List<String>();
        for(int i = 0; i < 4; i++)
        {
            String buttonInfo = "" + (i + 1) + rndButtonColorStrings[i].Substring(0, 1).ToUpper() + labelTexts[i] + rndLabelColorStrings[i].Substring(0,1).ToUpper();
            buttonInfoList.Add(buttonInfo);
        }
        Debug.LogFormat("[Memory Buster #{0}] Stage {5} full button info: {1} {2} {3} {4}", ModuleId, buttonInfoList.ElementAt(0), buttonInfoList.ElementAt(1), buttonInfoList.ElementAt(2), buttonInfoList.ElementAt(3), stage);
        switch (stage)
        {
            case 1:
                stage1ButtonInfo = buttonInfoList;
                break;
            case 2:
                stage2ButtonInfo = buttonInfoList;
                break;
            case 3:
                stage3ButtonInfo = buttonInfoList;
                break;
            case 4:
                stage4ButtonInfo = buttonInfoList;
                break;
            case 5:
                stage5ButtonInfo = buttonInfoList;
                break;
        }
    }
    void LogPress(int index)
    {
        /*
         * INFO KEY:
         * E.G: 1R1RR
         * POSITION 1, RED BUTTON, READS 1, RED LABEL
         */
        if (stage < 6)
        {
            String logged = "" + (index + 1) + rndButtonColorStrings[index].Substring(0, 1).ToUpper() + labels[index].text + rndLabelColorStrings[index].Substring(0, 1).ToUpper();
            pressedButtons.Add(logged);
            Debug.LogFormat("[Memory Buster #{0}] Correctly pressed: position {1}, button color {2}, label {3}, label color {4}", ModuleId, (index + 1), rndButtonColorStrings[index], labels[index].text, rndLabelColorStrings[index]);
            Debug.LogFormat("[Memory Buster #{0}] Logged: {1}", ModuleId, logged);
            Audio.PlaySoundAtTransform(sounds[stage - 1], display.transform);
        }
        else
        {
            SolveAnim();
            Audio.PlaySoundAtTransform(sounds[5], display.transform);
        }
    }
    void GenerateButtons() {
        List<Material> pool = new List<Material>();
        List<String> poolStr = new List<String>();
        pool.Add(buttonColors[0]);
        pool.Add(buttonColors[1]);
        pool.Add(buttonColors[2]);
        pool.Add(buttonColors[3]);
        poolStr.Add("red");
        poolStr.Add("blue");
        poolStr.Add("yellow");
        poolStr.Add("green");
        for (int i = 0; i < 3; i++)
        {
            int seed = Rnd.Range(0, pool.Count);
            rndButtonColors[i] = pool.ElementAt(seed);
            pool.RemoveAt(seed);
            rndButtonColorStrings[i] = poolStr.ElementAt(seed);
            poolStr.RemoveAt(seed);
        }
        rndButtonColors[3] = pool.ElementAt(0);
        pool.RemoveAt(0);
        rndButtonColorStrings[3] = poolStr.ElementAt(0);
        poolStr.RemoveAt(0);
        Debug.LogFormat("[Memory Buster #{0}] Stage {1} button colors are: {2}, {3}, {4}, {5}", ModuleId, stage, rndButtonColorStrings[0], rndButtonColorStrings[1], rndButtonColorStrings[2], rndButtonColorStrings[3]);
    }
    void GenerateLabelTexts() {
        List<String> pool = new List<String>();
        pool.Add("1");
        pool.Add("2");
        pool.Add("3");
        pool.Add("4");
        for (int i = 0; i < 3; i++) //STORED IN labelTexts
        {
            int seed = Rnd.Range(0, pool.Count);
            labelTexts[i] = pool.ElementAt(seed);
            pool.RemoveAt(seed);
        }
        labelTexts[3] = pool.ElementAt(0);
        pool.RemoveAt(0);
        Debug.LogFormat("[Memory Buster #{0}] Stage {1} label numbers are: {2}, {3}, {4}, {5}", ModuleId, stage, labelTexts[0], labelTexts[1], labelTexts[2], labelTexts[3]);
    }
    void GenerateLabelColors() {
        List<Color> pool = new List<Color>();
        List<String> poolStr = new List<String>();
        pool.Add(labelColors[0]);
        pool.Add(labelColors[1]);
        pool.Add(labelColors[2]);
        pool.Add(labelColors[3]);
        poolStr.Add("red");
        poolStr.Add("blue");
        poolStr.Add("yellow");
        poolStr.Add("green");
        for (int i = 0; i < 3; i++)
        {
            int seed = Rnd.Range(0, pool.Count);
            rndLabelColors[i] = pool.ElementAt(seed);
            pool.RemoveAt(seed);
            rndLabelColorStrings[i] = poolStr.ElementAt(seed);
            poolStr.RemoveAt(seed);
        }
        rndLabelColors[3] = pool.ElementAt(0);
        pool.RemoveAt(0);
        rndLabelColorStrings[3] = poolStr.ElementAt(0);
        poolStr.RemoveAt(0);
        Debug.LogFormat("[Memory Buster #{0}] Stage {1} label colors are: {2}, {3}, {4}, {5}", ModuleId, stage, rndLabelColorStrings[0], rndLabelColorStrings[1], rndLabelColorStrings[2], rndLabelColorStrings[3]);
    }

    void GenerateDisplay() {
        List<Material> pool = new List<Material>();
        pool.Add(displayColors[0]);
        pool.Add(displayColors[1]);
        pool.Add(displayColors[2]);
        pool.Add(displayColors[3]);
        pool.Add(displayColors[4]);
        pool.Add(displayColors[5]);
        int seed = Rnd.Range(0, pool.Count);
        displayColor = pool.ElementAt(seed);
        switch (seed)
        {
            case 0:
                displayColorStr = "red";
                break;
            case 1:
                displayColorStr = "blue";
                break;
            case 2:
                displayColorStr = "yellow";
                break;
            case 3:
                displayColorStr = "green";
                break;
            case 4:
                displayColorStr = "orange";
                break;
            case 5:
                displayColorStr = "purple";
                break;

        }
        displayInfo.Add(displayColorStr);
        Debug.LogFormat("[Memory Buster #{0}] Stage {1} display color is: {2}", ModuleId, stage, displayColorStr);
    }
}
