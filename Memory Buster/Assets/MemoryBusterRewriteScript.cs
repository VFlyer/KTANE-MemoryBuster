using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class MemoryBusterRewriteScript : MonoBehaviour {

	public KMBombModule modSelf;
	public KMAudio mAudio;
	public KMRuleSeedable ruleseed;
	public KMSelectable[] buttonSelectables;
	public TextMesh[] labels, cbBtnText;
	public TextMesh cbDisplayTxt;
	public MeshRenderer display;
	public MeshRenderer[] stageLights, buttons;
	public KMColorblindMode colorblindMode;
	public Material[] displayMats, btnMats, statusMats;
	public Material offMat;
	public Color[] txtColors, cbTextBtnColors;
	enum MemoryType
    {
		None,
		Position,
		Label,
		BtnColor,
		TxtColor,
		Stage,
		Action
    }

	List<int[]> storedIdxLabels, storedIdxColorBtns, storedIdxColorLabels;
	List<int> storedIdxExpected, storedIdxDisplays;
	int completedStages = 0;
	int moduleID;
	static int modIDCnt;

	bool ModuleSolved, animating = true, colorblindDetected = false;

	static readonly string[] sounds = { "stage 1", "stage 2", "stage 3", "stage 4", "stage 5", "solve" },
		colorBtnsTxts = { "red", "yellow", "green", "blue" },
		colorDisplays = { "red", "orange", "yellow", "green", "blue", "purple" },
		posAbbrev = { "1st", "2nd", "3rd", "4th", "5th", "6th" };
	string[][] allRules;
	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }
	void QuickLogDebug(string toLog, params object[] args)
    {
		Debug.LogFormat("<{0} #{1}> {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }
	void HandleRuleSeed()
    {
		var rsRNG = ruleseed != null ? ruleseed.GetRNG() : new MonoRandom(1);
		if (rsRNG.Seed == 1)
		{
			// Encode all rules for rule seed 1. Items are 1-indexed (labels, positions, stage, colors).
			/* Notes:
			* - Order for display colors should go Red, Orange, Yellow, Green, Blue, Purple
			* - Order for colors on buttons and texts should go Red, Yellow, Green, Blue
			* - P: Position
			* - L: Label (#)
			* - B: Button Color
			* - T: Text Color
			* - A: Action
			* - S: Stage
			*/
			allRules = new[] {
				new[] { "P1", "L4", "B2", "T2", "P3", "L4", },
				new[] { "LS1", "BL4S1", "P1", "B3", "PS1", "PT3S1", },
				new[] { "TS1", "BL4S1", "BS2", "AS1", "AS2", "AS2", },
				new[] { "LT3S3", "PB3S3", "BS3", "P3", "TS3", "L3", },
				new[] { "AS4", "AS2", "L4", "P2", "PS4", "LS2", },
				new[] { "AS5", "AS3", "AS1", "AS1", "AS5", "AS3", },
			};
		}
		else
		{
			var allPossibleFormats = new string[][] {
			new[] { "PLBT", "1234" },
			new[] { "PLBT", "S", "12345" },
			new[] { "A", "S", "12345" },
			new[] { "P", "LBT", "1234", "S", "12345" },
			new[] { "L", "PBT", "1234", "S", "12345" },
			new[] { "B", "PLT", "1234", "S", "12345" },
			new[] { "T", "PLB", "1234", "S", "12345" },
			};
			var idxesPerStageAllowedFormats = new int[][] {
				new[] { 0 },
				new[] { 0, 1, 3, 4, 5, 6 },
				new[] { 0, 1, 2, 3, 4, 5, 6 },
				new[] { 0, 1, 2, 3, 4, 5, 6 },
				new[] { 0, 1, 2, 3, 4, 5, 6 },
				new[] { 2 },
			};
			allRules = new string[6][];
			for (var x = 0; x < idxesPerStageAllowedFormats.Length; x++)
            {
				var newRuleCurStage = new string[6];
				var curRestrictionFormats = idxesPerStageAllowedFormats[x];
				for (var y = 0; y < 6; y++)
                {
					var newStr = "";
					var pickedInstFormat = allPossibleFormats[curRestrictionFormats[rsRNG.Next(0, curRestrictionFormats.Length)]];
					var stageDetected = false;
					foreach (var str in pickedInstFormat)
                    {
						var allowedStr = str;
						if (stageDetected)
							allowedStr = allowedStr.Substring(0, x);
						var pickedChr = allowedStr[rsRNG.Next(0, allowedStr.Length)];
						if (pickedChr == 'S')
							stageDetected = true;
						newStr += pickedChr;
                    }
					newRuleCurStage[y] = newStr;
                }
				allRules[x] = newRuleCurStage;
            }
		}
		if (ruleseed == null)
			QuickLog("Using default rules bundled in the manual.");
		else
			QuickLog("Using rule seed {0}. See filtered log for the encoded instructions.", rsRNG.Seed);
		QuickLogDebug("Encoded rules in the order R, O, Y, G, B, P.");
		for (var x = 0; x < allRules.Length; x++)
			QuickLogDebug("Stage {0}: [{1}]", x + 1, allRules[x].Join("], ["));
    }
	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		HandleRuleSeed();
		storedIdxColorBtns = new List<int[]>();
		storedIdxColorLabels = new List<int[]>();
		storedIdxLabels = new List<int[]>();
		storedIdxDisplays = new List<int>();
		storedIdxExpected = new List<int>();
		try
        {
			colorblindDetected = colorblindMode.ColorblindModeActive;
        }
		catch
        {
			colorblindDetected = false;
        }
		HandleNewStage();
        for (var x = 0; x < buttonSelectables.Length; x++)
        {
			var y = x;
			buttonSelectables[x].OnInteract += delegate {
				HandleIdxPress(y);
				return false;
			};
        }
		modSelf.OnActivate += delegate { StartCoroutine(AnimateStageChange()); };
		for (var x = 3; x >= 0; x--)
		{
			labels[x].text = "";
			labels[x].color = Color.white;
			cbBtnText[x].text = "";
			buttons[x].material = btnMats.Last();
		}
	}
	IEnumerator AnimateStageChange(int stageIdx = 0, bool isSolving = false, float delay = 0.05f)
    {
		animating = true;
		display.material = displayMats.Last();
		cbDisplayTxt.text = "";
        for (var x = 0; x < stageLights.Length; x++)
			stageLights[x].material = x <= completedStages ? statusMats[1] : statusMats[0];

        for (var x = 3; x >= 0; x--)
        {
			labels[x].text = isSolving ? "NICE"[x].ToString() : "?";
			labels[x].color = Color.white;
			labels[x].characterSize = 1;
			cbBtnText[x].text = "";
			buttons[x].material = btnMats.Last();
			if (delay > 0f)
				yield return new WaitForSeconds(delay);
        }
		if (!isSolving)
        {
			display.material = displayMats[storedIdxDisplays[stageIdx]];
			cbDisplayTxt.text = colorblindDetected ? colorDisplays[storedIdxDisplays[stageIdx]] : "";
			for (var x = 0; x < 4; x++)
			{
				labels[x].text = "1234"[storedIdxLabels[stageIdx][x]].ToString() + (colorblindDetected ? "RYGB"[storedIdxColorLabels[completedStages][x]].ToString() : "");
				labels[x].color = txtColors[storedIdxColorLabels[stageIdx][x]];
				cbBtnText[x].text = colorblindDetected ? "RYGB"[storedIdxColorBtns[completedStages][x]].ToString() : "";
				cbBtnText[x].color = cbTextBtnColors[storedIdxColorBtns[completedStages][x]];
				buttons[x].material = btnMats[storedIdxColorBtns[stageIdx][x]];
				if (delay > 0f)
					yield return new WaitForSeconds(delay);
			}
		}
		animating = false;
    }

	void HandleIdxPress(int idxPressed)
    {
		if (ModuleSolved || animating)
			return;
		buttonSelectables[idxPressed].AddInteractionPunch();
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttonSelectables[idxPressed].transform);
		if (idxPressed == storedIdxExpected[completedStages])
        {
			QuickLog("Correctly pressed: {0} position, label {1} {2}, {3} button", posAbbrev[idxPressed],
				colorBtnsTxts[storedIdxColorLabels.Last()[idxPressed]],
				storedIdxLabels.Last()[idxPressed] + 1,
				colorBtnsTxts[storedIdxColorBtns.Last()[idxPressed]]);
			mAudio.PlaySoundAtTransform(sounds[completedStages], transform);
			completedStages++;
			if (completedStages >= 6)
			{
				modSelf.HandlePass();
				ModuleSolved = true;
			}
			else
			{
				HandleNewStage();
			}
        }
		else
        {
			QuickLog("Incorrectly pressed: {0} position, label {1} {2}, {3} button", posAbbrev[idxPressed],
				colorBtnsTxts[storedIdxColorLabels.Last()[idxPressed]],
				storedIdxLabels.Last()[idxPressed] + 1,
				colorBtnsTxts[storedIdxColorBtns.Last()[idxPressed]]);
			modSelf.HandleStrike();
			ResetModule();
        }
		animating = true;
		StartCoroutine(AnimateStageChange(completedStages, ModuleSolved));
	}
	void ResetModule()
    {
		storedIdxColorBtns.Clear();
		storedIdxColorLabels.Clear();
		storedIdxLabels.Clear();
		storedIdxDisplays.Clear();
		storedIdxExpected.Clear();
		completedStages = 0;
		HandleNewStage();
	}
	void HandleNewStage()
    {
		var pickedIdxColor = Random.Range(0, 6);
		var shuffledIdxLabels = Enumerable.Range(0, 4).ToArray().Shuffle();
		var shuffledIdxTxtColors = Enumerable.Range(0, 4).ToArray().Shuffle();
		var shuffledIdxBtnColors = Enumerable.Range(0, 4).ToArray().Shuffle();
		QuickLog("Stage {0}:", completedStages + 1);
		QuickLog("Button colors from left to right: {0}", shuffledIdxBtnColors.Select(a => colorBtnsTxts[a]).Join(", "));
		QuickLog("Text colors from left to right: {0}", shuffledIdxTxtColors.Select(a => colorBtnsTxts[a]).Join(", "));
		QuickLog("Labels from left to right: {0}", shuffledIdxLabels.Select(a => a + 1).Join(", "));
		QuickLog("Current displayed color: {0}", colorDisplays[pickedIdxColor]);
		storedIdxLabels.Add(shuffledIdxLabels);
		storedIdxColorLabels.Add(shuffledIdxTxtColors);
		storedIdxColorBtns.Add(shuffledIdxBtnColors);
		storedIdxDisplays.Add(pickedIdxColor);
		// The entire section of decoding the instruction constructed.
		var curRule = allRules[completedStages][pickedIdxColor];
		var iteratedInstructions = new List<string>();
	iterateInst:
		var curTargetStage = completedStages;
		var curTargetIdxPos = -1;
		var lastRule = curRule;
		var curTypeProcessed = new List<MemoryType>();
		var curIdxVal = new List<int>();
		iteratedInstructions.Add(curRule);
		// Section A: Converting the encoded instruction as pairs, note that many instructions are not even in length.
		for (var x = 0; x < curRule.Length; x++)
        {
			var curChr = curRule[x];
			var idxDigit = "12345".IndexOf(curChr);
			if (idxDigit != -1)
				curIdxVal.Insert(0, idxDigit);
			else
				switch (curChr)
                {
					case 'S': curTypeProcessed.Insert(0, MemoryType.Stage); break;
					case 'B': curTypeProcessed.Insert(0, MemoryType.BtnColor); break;
					case 'T': curTypeProcessed.Insert(0, MemoryType.TxtColor); break;
					case 'L': curTypeProcessed.Insert(0, MemoryType.Label); break;
					case 'P': curTypeProcessed.Insert(0, MemoryType.Position); break;
					case 'A': curTypeProcessed.Insert(0, MemoryType.Action); break;
                }
        }
		// Section B: Processing the encoded instruction as pairs. Use the unpaired alongside the other variables to get the target.
		for (var x = 0; x < curTypeProcessed.Count; x++)
        {
			var actionCur = curTypeProcessed[x];
			if (x < curIdxVal.Count)
            {
				var curVal = curIdxVal[x];
				switch (actionCur)
                {
					case MemoryType.Stage:
						curTargetStage = curVal;
						break;
					case MemoryType.Position:
						curTargetIdxPos = curVal;
						break;
					case MemoryType.Label:
						curTargetIdxPos = Enumerable.Range(0, 4).Single(a => storedIdxLabels[curTargetStage][a] == curVal);
						break;
					case MemoryType.BtnColor:
						curTargetIdxPos = Enumerable.Range(0, 4).Single(a => storedIdxColorBtns[curTargetStage][a] == curVal);
						break;
					case MemoryType.TxtColor:
						curTargetIdxPos = Enumerable.Range(0, 4).Single(a => storedIdxColorLabels[curTargetStage][a] == curVal);
						break;
				}
            }
			else
            {
				if (curTargetIdxPos == -1)
					curTargetIdxPos = storedIdxExpected[curTargetStage];
				switch (actionCur)
				{
					case MemoryType.Stage:
					case MemoryType.Position:
						break;
					case MemoryType.Label:
						{
							var labelObtained = storedIdxLabels[curTargetStage][curTargetIdxPos];
							curTargetIdxPos = Enumerable.Range(0, 4).Single(a => storedIdxLabels.Last()[a] == labelObtained);
						}
						break;
					case MemoryType.BtnColor:
						{
							var btnColorObtained = storedIdxColorBtns[curTargetStage][curTargetIdxPos];
							curTargetIdxPos = Enumerable.Range(0, 4).Single(a => storedIdxColorBtns.Last()[a] == btnColorObtained);
						}
						break;
					case MemoryType.TxtColor:
						{
							var txtColorObtained = storedIdxColorLabels[curTargetStage][curTargetIdxPos];
							curTargetIdxPos = Enumerable.Range(0, 4).Single(a => storedIdxColorLabels.Last()[a] == txtColorObtained);
						}
						break;
					case MemoryType.Action:
                        {
							var idxColorObtained = storedIdxDisplays[curTargetStage];
							curRule = allRules[curTargetStage][idxColorObtained];
							break;
                        }
				}
			}
        }
		if (lastRule != curRule) // Since there are some rules that require other instructions, we will need to iterate through that instruction as if it relied on the current stage.
			goto iterateInst;
		QuickLogDebug("Iterated through these instructions: {0}", iteratedInstructions.Join());
		storedIdxExpected.Add(curTargetIdxPos);
		QuickLog("Expecting the following to be pressed: {0} position, label {1} {2}, {3} button", posAbbrev[curTargetIdxPos],
			colorBtnsTxts[shuffledIdxTxtColors[curTargetIdxPos]],
			shuffledIdxLabels[curTargetIdxPos] + 1,
			colorBtnsTxts[shuffledIdxBtnColors[curTargetIdxPos]]);
    }
	void HandleColorblindModeToggle()
    {
		cbDisplayTxt.text = colorblindDetected ? colorDisplays[storedIdxDisplays[completedStages]] : "";
		for (var x = 0; x < 4; x++)
		{
			labels[x].text = "1234"[storedIdxLabels[completedStages][x]].ToString() + (colorblindDetected ? "RYGB"[storedIdxColorLabels[completedStages][x]].ToString() : "");
			labels[x].color = txtColors[storedIdxColorLabels[completedStages][x]];
			cbBtnText[x].text = colorblindDetected ? "RYGB"[storedIdxColorBtns[completedStages][x]].ToString() : "";
			cbBtnText[x].color = cbTextBtnColors[storedIdxColorBtns[completedStages][x]];
		}
	}


#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Press a button in that position, 1 being the left-most button, with \"!{0} <1/2/3/4>\". Toggle colorblind mode with \"!{0} colorblind/colourblind/cb\" ";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
	{
		if (animating || ModuleSolved)
		{
			yield return "sendtochaterror The module is refusing inputs right now. Wait a bit until the module is ready.";
			yield break;
		}
		var intCommand = command.Trim();
		var rgxCBMatch = Regex.Match(intCommand, @"^(colou?rblind|cb)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		var rgxDigitMatch = Regex.Match(intCommand, @"^[1-4]$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (rgxCBMatch.Success)
        {
			yield return null;
			colorblindDetected ^= true;
			HandleColorblindModeToggle();
        }
		else if (rgxDigitMatch.Success)
        {
			var obtainedMatch = rgxDigitMatch.Value.ToLowerInvariant();
			yield return null;
			buttonSelectables[int.Parse(obtainedMatch) - 1].OnInteract();
		}
		else
        {
			yield return string.Format("sendtochaterror Invalid command sent: \"{0}\" Check your command for typos.", intCommand);
			yield break;
		}
	}

	IEnumerator TwitchHandleForcedSolve()
    {
		while (!ModuleSolved)
        {
			while (animating)
				yield return true;
			buttonSelectables[storedIdxExpected[completedStages]].OnInteract();
			yield return new WaitForSeconds(.1f);
			while (animating)
				yield return true;
        }
    }

}
