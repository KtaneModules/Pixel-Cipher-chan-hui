using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class pixelcipherScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo bomb;

	private int[,] textField = new int[15,25]{
		{3,8,4,7,2,		5,0,1,9,6,	5,7,1,1,9,	3,4,0,6,2,	8,3,4,9,0},
		{8,9,4,6,6,		1,5,0,1,3,	4,0,9,2,0,	8,2,7,1,6,	3,5,7,4,9},
		{1,0,7,7,2,		5,6,9,0,8,	4,4,1,5,6,	3,1,9,5,8,	3,2,0,9,4},
		{7,1,2,4,4,		4,3,1,7,5,	5,8,8,2,2,	7,5,8,0,6,	6,9,3,0,9},
		{6,5,2,1,7,		7,8,5,1,0,	4,2,6,0,4,	3,3,7,3,1,	0,9,5,9,8},

		{7,9,1,8,7,		0,4,8,1,8,	3,6,6,1,4,	5,6,3,9,2,	5,9,2,0,3},
		{9,7,8,0,1,		8,2,2,5,5,	9,1,1,7,9,	4,3,6,4,6,	2,7,3,0,5},
		{1,6,2,7,0,		8,7,8,2,3,	5,6,9,5,3,	1,8,1,7,0,	6,9,4,5,4},
		{5,7,6,1,6,		9,2,9,1,8,	7,2,8,6,2,	4,3,7,4,0,	5,4,3,5,0},
		{9,5,0,7,1,		4,8,9,3,4,	2,3,6,7,2,	0,1,6,5,9,	8,6,7,4,5},

		{9,1,2,5,1,		0,7,3,8,2,	9,4,0,7,3,	5,5,9,4,7,	8,6,1,6,4},
		{0,4,1,1,3,		5,3,7,2,0,	5,6,9,8,7,	2,6,9,4,7,	4,5,8,0,1},
		{1,2,7,2,1,		8,4,9,6,4,	5,8,6,8,7,	5,0,9,3,1,	3,9,7,4,0},
		{3,9,2,3,3,		8,4,6,1,7,	1,5,1,8,6,	0,0,7,0,6,	9,4,5,2,4},
		{4,9,4,6,2,		5,9,1,7,4,	3,5,2,3,8,	8,9,6,0,1,	5,1,7,0,3}};
	private int pickedField;

	private String[] pixelKeyword = new String[]{"HEART","HAPPY","HOUSE","ARROW","ARMOR","ACORN","CROSS","CHORD","CLOCK"
	,"DONUT","DELTA","DUCKY","EQUAL","EMOJI","EDGES","LIBRA","LUCKY","LUNAR","MEDAL","MOVIE","MUSIC","PANDA","PEARL","PIANO","PIXEL"};
	private int pickedKeyword;
	private String displayedString;
	private List<int> pressingSequence = new List<int>();
	private int stage = 0;

	public GameObject[] textDisplay;
	public KMSelectable[] buttons;

	//logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool fieldPicked;

	private bool nowOnStrike;
	private bool moduleSolved;

	public string TwitchHelpMessage = "Enter the digit with !{0} press 9 or !{0} submit 9. Enter the sequence with !{0} press 980346... or !{0} submit 980346... You may use spaces and commas in the digit sequence.";


	void Awake(){
		moduleId = moduleIdCounter++;
		foreach (KMSelectable button in buttons){
			KMSelectable pressedButton = button;
			for(int i=0;i<buttons.Length;i++){
				if (pressedButton == buttons[i]){
					button.OnInteract += delegate () { buttonPress(pressedButton, i); return false; };
					break;}}
		}
	}

	void Start () {
		if(!fieldPicked){
			pickField();
			fieldPicked = true;
		}
		pickKeyWord();
		displayKey();
		determinePress();
	}

	void pickField(){
		var SNarr = bomb.GetSerialNumber();
		int SN = SNarr[5];
		SN -= '0';

		if(bomb.GetBatteryCount() <= 1){
			pickedField = 0;
		}
		else if(bomb.GetBatteryCount() >= 2 && bomb.GetBatteryCount() <= 3){
			pickedField = 5;
		}
		else if(bomb.GetBatteryCount() >= 4){
			pickedField = 10;
		}

		if(SN==0||SN==5){
			pickedField += 0;
		}
		else if(SN==1||SN==6){
			pickedField += 1;
		}
		else if(SN==2||SN==7){
			pickedField += 2;
		}
		else if(SN==3||SN==8){
			pickedField += 3;
		}
		else if(SN==4||SN==9){
			pickedField += 4;
		}
		Debug.LogFormat("[Pixel Cipher #{0}] Correct Field is Field {1} (Batteries = {2} / Serial = {3}).", moduleId, pickedField+1, bomb.GetBatteryCount(), SN);
	}

	void pickKeyWord(){
		pickedKeyword = UnityEngine.Random.Range(0,pixelKeyword.Length);

		foreach(char c in pixelKeyword[pickedKeyword]){
			List<int> correctCoods = new List<int>();
			int encryNum = c-'A'+1;

			if (encryNum<=21){
				encryNum += 26*UnityEngine.Random.Range(0,4);
			}

			int tens = encryNum/10;
			int unit = encryNum%10;

			for(int i=0;i<25;i++){
				if(textField[pickedField,i] == tens){
					correctCoods.Add(i+1);
				}
			}
			correctCoods.Shuffle();
			displayedString += correctCoods[0].ToString()+" ";

			correctCoods.Clear();

			for(int i=0;i<25;i++){
				if(textField[pickedField,i] == unit){
					correctCoods.Add(i+1);
				}
			}
			correctCoods.Shuffle();
			displayedString += correctCoods[0].ToString()+" ";
		}
		displayedString = displayedString.Remove(displayedString.Length-1,1);

		Debug.LogFormat("[Pixel Cipher #{0}] Displayed digits are {1}.", moduleId, displayedString);
	}

	void displayKey(){
		foreach(GameObject obj in textDisplay){
			obj.GetComponent<TextMesh>().text = "";
			string[] arrangeSt = displayedString.Split(' ');
			for(int i=0;i<5;i++){
				obj.GetComponent<TextMesh>().text += arrangeSt[i]+" ";
			}
			obj.GetComponent<TextMesh>().text = obj.GetComponent<TextMesh>().text.Remove(obj.GetComponent<TextMesh>().text.Length-1,1);
			obj.GetComponent<TextMesh>().text += "\n";
			for(int i=5;i<10;i++){
				obj.GetComponent<TextMesh>().text += arrangeSt[i]+" ";
			}
			obj.GetComponent<TextMesh>().text =	obj.GetComponent<TextMesh>().text.Remove(obj.GetComponent<TextMesh>().text.Length-1,1);

			//Debug.LogFormat("[Pixel Cipher #{0}] {1}.", moduleId, finSt);
		}
	}

	void determinePress(){
		if(pickedKeyword == 0){
			int[] keywordPress = new int[]{1,3,5,6,7,8,9,10,11,12,13,14,16,17,18,22};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 1){
			int[] keywordPress = new int[]{1,3,6,8,15,19,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 2){
			int[] keywordPress = new int[]{2,6,7,8,10,11,12,13,14,16,18,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 3){
			int[] keywordPress = new int[]{2,3,4,8,9,12,14,15,16,21};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 4){
			int[] keywordPress = new int[]{0,1,3,4,5,6,7,8,9,11,12,13,16,17,18,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 5){
			int[] keywordPress = new int[]{2,5,6,7,8,9,11,12,13,16,17,18,22};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 6){
			int[] keywordPress = new int[]{2,7,10,11,12,13,14,17,22};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 7){
			int[] keywordPress = new int[]{2,7,8,12,14,15,16,17,20,21,22};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 8){
			int[] keywordPress = new int[]{1,2,3,5,7,9,10,12,13,14,15,19,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 9){
			int[] keywordPress = new int[]{1,2,3,5,6,7,8,9,10,11,13,14,15,16,17,18,19,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 10){
			int[] keywordPress = new int[]{2,6,8,11,13,15,19,20,21,22,23,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 11){
			int[] keywordPress = new int[]{1,2,6,8,10,11,12,13,16,17,18,19,21,22,23,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 12){
			int[] keywordPress = new int[]{6,7,8,16,17,18};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 13){
			int[] keywordPress = new int[]{0,1,5,7,9,10,12,15,17,20,21,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 14){
			int[] keywordPress = new int[]{0,1,3,4,5,9,15,19,20,21,23,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 15){
			int[] keywordPress = new int[]{1,2,3,6,8,10,11,13,14,20,21,22,23,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 16){
			int[] keywordPress = new int[]{1,3,5,6,8,9,15,16,18,19,21,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 17){
			int[] keywordPress = new int[]{3,8,9,12,13,14,15,16,17,18,19,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 18){
			int[] keywordPress = new int[]{0,4,6,8,12,16,17,18,22};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 19){
			int[] keywordPress = new int[]{3,4,6,7,10,15,16,17,18,19,20,21,22,23,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 20){
			int[] keywordPress = new int[]{1,2,3,4,6,9,11,14,15,16,18,19,20,21,23,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 21){
			int[] keywordPress = new int[]{6,8,10,11,13,14,17,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 22){
			int[] keywordPress = new int[]{1,2,3,5,8,9,10,12,13,14,15,16,17,18,19,21,22,23};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 23){
			int[] keywordPress = new int[]{1,3,6,8,11,13};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		else if(pickedKeyword == 24){
			int[] keywordPress = new int[]{0,2,4,6,8,10,12,14,16,18,20,22,24};
			for(int i=0;i<keywordPress.Length;i++){
				pressingSequence.Add(textField[pickedField,keywordPress[i]]);
			}
		}
		Debug.LogFormat("[Pixel Cipher #{0}] Selected keyword is {1}.", moduleId, pixelKeyword[pickedKeyword]);


		String pressingSeq = "";
		for(int i=0;i<pressingSequence.Count;i++){
			pressingSeq += pressingSequence[i].ToString() + " ";
		}
		pressingSeq = pressingSeq.Remove(pressingSeq.Length-1,1);
		Debug.LogFormat("[Pixel Cipher #{0}] Correct buttons are {1}.", moduleId, pressingSeq);
	}

	void buttonPress(KMSelectable button, int indv){

		if(moduleSolved||nowOnStrike){
			return;
		}

		button.AddInteractionPunch(.5f);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("[Pixel Cipher #{0}] You pressed button {1}.", moduleId, indv);

		if(indv==pressingSequence[stage]){
			stage++;
			if(stage==pressingSequence.Count()){
				moduleSolved = true;
				GetComponent<KMBombModule>().HandlePass();
				audio.PlaySoundAtTransform("solve",transform);
				Debug.LogFormat("[Pixel Cipher #{0}] Inputted sequence was correct, Module solved.", moduleId);
				StartCoroutine(Solved());
			}
		}
		else{
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Pixel Cipher #{0}] Strike! Correct button was {1}.", moduleId,pressingSequence[stage]);
			stage=0;
			pressingSequence.Clear();
			displayedString="";
			StartCoroutine(Strike(indv));
			return;
		}
	}

	IEnumerator Strike(int indv){
		nowOnStrike = true;

		List<String> StrikeMessage = new List<String>(new String[] {"DISAPPOINTING","HELL NAH","THIS IS NOT EVEN\n A HARD MODULE","SERIOUSLY?","GIT\nGUD","BRUH MOMENT"});
		StrikeMessage.Shuffle();
		foreach(GameObject obj in textDisplay){
			obj.GetComponent<TextMesh>().text = StrikeMessage[0];
		}
		yield return new WaitForSeconds(2f);

		int flash = 0;
		while(flash < 3){
				foreach(GameObject obj in textDisplay){
					obj.GetComponent<TextMesh>().text = "RESETTING\nMODULE";
				}
				yield return new WaitForSeconds(0.1f);
				foreach(GameObject obj in textDisplay){
					obj.GetComponent<TextMesh>().text = "RESETTING\nMODULE.";
				}
				yield return new WaitForSeconds(0.1f);
				foreach(GameObject obj in textDisplay){
					obj.GetComponent<TextMesh>().text = "RESETTING\nMODULE..";
				}
				yield return new WaitForSeconds(0.1f);
				foreach(GameObject obj in textDisplay){
					obj.GetComponent<TextMesh>().text = "RESETTING\nMODULE...";
				}
				yield return new WaitForSeconds(0.1f);

				flash++;
		}

		nowOnStrike = false;
		Debug.LogFormat("[Pixel Cipher #{0}] Resetting module...", moduleId);
		Start();
	}

	IEnumerator Solved(){
		List<String> SolveMessage = new List<String>(new String[] {"WE DID IT REDDIT!","GG","NICELY DONE","EZ","POGCHAMP","POGGERS!"});
		SolveMessage.Shuffle();
		foreach(GameObject obj in textDisplay){
			obj.GetComponent<TextMesh>().text = SolveMessage[0];
		}
		yield return new WaitForSeconds(0.1f);
	}

	public List <KMSelectable> ProcessTwitchCommand(string command){

		string[] cutInBlank = command.Split(new char[] {' '});
		List <KMSelectable> whichToPress = new List <KMSelectable>();

		if (cutInBlank[0].Equals("press", StringComparison.InvariantCultureIgnoreCase)|| cutInBlank[0].Equals("submit", StringComparison.InvariantCultureIgnoreCase)){
			for(int i=1;i<cutInBlank.Length;i++){
				char[] digits = cutInBlank[i].ToCharArray();
				for(int k=0;k<digits.Length;k++){
										for(int j=0;j<10;j++){
											if(digits[k] == j.ToString().ToCharArray()[0]){
												whichToPress.Add(buttons[j]);
											}
										}
				}
			}
			return whichToPress;
    }


    return null;
	}

}
