using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class KeypadModule : MonoBehaviour
{
    public KMSelectable[] buttons;
    public TextMesh EntryText;
    string correctEntry;
    bool isActivated = false;
    KMBombInfo Info;

    static Dictionary<string, int> evenVowelDict = new Dictionary<string, int>()
    {
        {"A", 9}, {"B", 0}, {"C", 1}, {"D", 1}, {"E", 5}, {"F", 5}, {"G", 7}, {"H", 9}, {"I", 3}, {"J", 8}, {"K", 2}, {"L", 3}, {"M", 3}, {"N", 6}, {"O", 8}, {"P", 8}, {"Q", 0}, {"R", 7}, {"S", 5}, {"T", 2}, {"U", 9}, {"V", 6}, {"W", 2}, {"X", 3}, {"Y", 8}, {"Z", 8}
    };
    static Dictionary<string, int> oddVowelDict = new Dictionary<string, int>()
    {
        {"A", 6}, {"B", 3}, {"C", 8}, {"D", 6}, {"E", 0}, {"F", 6}, {"G", 7}, {"H", 3}, {"I", 4}, {"J", 5}, {"K", 0}, {"L", 8}, {"M", 1}, {"N", 0}, {"O", 5}, {"P", 9}, {"Q", 5}, {"R", 3}, {"S", 2}, {"T", 3}, {"U", 5}, {"V", 0}, {"W", 8}, {"X", 5}, {"Y", 3}, {"Z", 1}
    };
    static Dictionary<string, int> evenNoVowelDict = new Dictionary<string, int>()
    {
        {"A", 9}, {"B", 3}, {"C", 7}, {"D", 4}, {"E", 6}, {"F", 2}, {"G", 1}, {"H", 9}, {"I", 4}, {"J", 5}, {"K", 5}, {"L", 8}, {"M", 4}, {"N", 8}, {"O", 3}, {"P", 8}, {"Q", 2}, {"R", 4}, {"S", 7}, {"T", 9}, {"U", 6}, {"V", 5}, {"W", 6}, {"X", 5}, {"Y", 2}, {"Z", 9}
    };
    static Dictionary<string, int> oddNoVowelDict = new Dictionary<string, int>()
    {
        {"A", 1}, {"B", 6}, {"C", 0}, {"D", 0}, {"E", 7}, {"F", 3}, {"G", 9}, {"H", 5}, {"I", 2}, {"J", 8}, {"K", 3}, {"L", 4}, {"M", 1}, {"N", 4}, {"O", 8}, {"P", 2}, {"Q", 4}, {"R", 7}, {"S", 2}, {"T", 6}, {"U", 1}, {"V", 8}, {"W", 7}, {"X", 3}, {"Y", 9}, {"Z", 7}
    };
    string serialNumber;
    string ports;
    string batteries;
    string indicators;
    string userEntry = "";

    void Start()
    {
        Info = GetComponent<KMBombInfo>();
        Init();

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
        
    }

    void Init()
    {
        EntryText.text = "";
   
        for (int i = 0; i < buttons.Length; i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j); return false; };
        }       
    }

    void ActivateModule()
    {
        isActivated = true;
        correctEntry = RightAnswerGenerator();
    }

    void OnPress(int pressedButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (!isActivated)
        {
            Debug.Log("Pressed button before module has been activated!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            Debug.Log("Pressed " + pressedButton + " button");
            if (pressedButton < 10)
            {
                if (userEntry.Length < 6) { userEntry = userEntry + pressedButton; }
                EntryText.text = userEntry;
            }
            else if (pressedButton == 10) 
            {
                userEntry = "";
                EntryText.text = userEntry;
            } else
            {
                if (userEntry.Equals(correctEntry))
                {
                    GetComponent<KMBombModule>().HandlePass();
                } else
                {
                    GetComponent<KMBombModule>().HandleStrike();
                }
            }
        }
    }
    string GetSerial() // do not attempt to call this until the bomb has been activated!!!
    {
        Info = GetComponent<KMBombInfo>();
        string Serial = "";
        List<string> Response = Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Response[0]);
        Serial = dict["serial"];         
        return Serial;
    }

    int SerialLastDigit()
    {

        string Serial = GetSerial();

        return int.Parse(Serial.Substring(Serial.Length - 1));

    }

    string SerialCharacterGetter(int index)
    {
        string Serial = GetSerial();
        return string.Copy(Serial.Substring(index, 1));
        
    }

    string RightAnswerGenerator()
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string RightAnswer = "";
        string Serial = GetSerial();
        int lastSerial = SerialLastDigit();
        int litIndicatorsCount = IndicatorsCount(true);
        int batteryCount = BatteryCount();
        int dviCount = PortsCount("DVI");
        int parallelCount = PortsCount("Parallel");
        int serialCount = PortsCount("Serial");
        for (int i = 0; i < Serial.Length; i++)
        {
            if ((lastSerial==2 || lastSerial==4 || lastSerial==6 || lastSerial==8 || lastSerial==0) && (Serial.Contains("A") || Serial.Contains("E") || Serial.Contains("I") || Serial.Contains("O") || Serial.Contains("U")))
            {
                string thisCharacter = Serial.Substring(i, 1);
                if (alphabet.Contains(thisCharacter)) { RightAnswer = RightAnswer + evenVowelDict[thisCharacter]; }
                else if (thisCharacter.Equals("0")) { RightAnswer = RightAnswer + batteryCount; }
                else if (thisCharacter.Equals("1")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("2")) { RightAnswer = RightAnswer + serialCount; }
                else if (thisCharacter.Equals("3")) { RightAnswer = RightAnswer + batteryCount; }
                else if (thisCharacter.Equals("4")) { RightAnswer = RightAnswer + dviCount; }
                else if (thisCharacter.Equals("5")) { RightAnswer = RightAnswer + litIndicatorsCount; }
                else if (thisCharacter.Equals("6")) { RightAnswer = RightAnswer + dviCount; }
                else if (thisCharacter.Equals("7")) { RightAnswer = RightAnswer + serialCount; }
                else if (thisCharacter.Equals("8")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("9")) { RightAnswer = RightAnswer + litIndicatorsCount; }
            }
            else if ((lastSerial == 1 || lastSerial == 3 || lastSerial == 5 || lastSerial == 7 || lastSerial == 9) && (Serial.Contains("A") || Serial.Contains("E") || Serial.Contains("I") || Serial.Contains("O") || Serial.Contains("U")))
            {
                string thisCharacter = Serial.Substring(i, 1);
                if (alphabet.Contains(thisCharacter)) { RightAnswer = RightAnswer + oddVowelDict[thisCharacter]; }
                else if (thisCharacter.Equals("0")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("1")) { RightAnswer = RightAnswer + batteryCount; }
                else if (thisCharacter.Equals("2")) { RightAnswer = RightAnswer + litIndicatorsCount; }
                else if (thisCharacter.Equals("3")) { RightAnswer = RightAnswer + litIndicatorsCount; }
                else if (thisCharacter.Equals("4")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("5")) { RightAnswer = RightAnswer + dviCount; }
                else if (thisCharacter.Equals("6")) { RightAnswer = RightAnswer + serialCount; }
                else if (thisCharacter.Equals("7")) { RightAnswer = RightAnswer + batteryCount; }
                else if (thisCharacter.Equals("8")) { RightAnswer = RightAnswer + serialCount; }
                else if (thisCharacter.Equals("9")) { RightAnswer = RightAnswer + dviCount; }
            }
            else if ((lastSerial == 2 || lastSerial == 4 || lastSerial == 6 || lastSerial == 8 || lastSerial == 0) && (!Serial.Contains("A") && !Serial.Contains("E") && !Serial.Contains("I") && !Serial.Contains("O") && !Serial.Contains("U")))
            {
                string thisCharacter = Serial.Substring(i, 1);
                if (alphabet.Contains(thisCharacter)) { RightAnswer = RightAnswer + evenNoVowelDict[thisCharacter]; }
                else if (thisCharacter.Equals("0")) { RightAnswer = RightAnswer + litIndicatorsCount; }
                else if (thisCharacter.Equals("1")) { RightAnswer = RightAnswer + serialCount; }
                else if (thisCharacter.Equals("2")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("3")) { RightAnswer = RightAnswer + dviCount; }
                else if (thisCharacter.Equals("4")) { RightAnswer = RightAnswer + litIndicatorsCount; }
                else if (thisCharacter.Equals("5")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("6")) { RightAnswer = RightAnswer + batteryCount; }
                else if (thisCharacter.Equals("7")) { RightAnswer = RightAnswer + dviCount; }
                else if (thisCharacter.Equals("8")) { RightAnswer = RightAnswer + batteryCount; }
                else if (thisCharacter.Equals("9")) { RightAnswer = RightAnswer + serialCount; }
            }
            else
            {
                string thisCharacter = Serial.Substring(i, 1);
                if (alphabet.Contains(thisCharacter)) { RightAnswer = RightAnswer + oddNoVowelDict[thisCharacter]; }
                else if (thisCharacter.Equals("0")) { RightAnswer = RightAnswer + serialCount; }
                else if (thisCharacter.Equals("1")) { RightAnswer = RightAnswer + litIndicatorsCount; }
                else if (thisCharacter.Equals("2")) { RightAnswer = RightAnswer + dviCount; }
                else if (thisCharacter.Equals("3")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("4")) { RightAnswer = RightAnswer + serialCount; }
                else if (thisCharacter.Equals("5")) { RightAnswer = RightAnswer + batteryCount; }
                else if (thisCharacter.Equals("6")) { RightAnswer = RightAnswer + litIndicatorsCount; }
                else if (thisCharacter.Equals("7")) { RightAnswer = RightAnswer + parallelCount; }
                else if (thisCharacter.Equals("8")) { RightAnswer = RightAnswer + dviCount; }
                else if (thisCharacter.Equals("9")) { RightAnswer = RightAnswer + batteryCount; }
            }
        }
        return RightAnswer;
    }

    int BatteryCount()
    {
        List<string> Response = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
        int count = 0;
        foreach (string Value in Response)
        {
            Dictionary<string, int> Batteries = JsonConvert.DeserializeObject<Dictionary<string, int>>(Value);
            count += Batteries["numbatteries"];
        }
        return count;
    }

    int IndicatorsCount(bool Lit) // checks how many indicators in the given state exist
    {
        List<string> Response = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null);
        int count = 0;
        foreach (string Value in Response)
        {
            Dictionary<string, string> Ind = JsonConvert.DeserializeObject<Dictionary<string, string>>(Value);
            bool On = Ind["on"] == "True";
            if (On)
            {
                count +=1;
            }
        }
        return count;
    }

    int PortsCount(string portTypes) // counts the number of ports of the given type
    {
        int count = 0;
        List<string> Response = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null);
        foreach (string Value in Response)
        {
            Dictionary<string, List<string>> Ind = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(Value);
            foreach (string Name in Ind["presentPorts"])
            {
                if (Name.Contains(portTypes))
                    count += 1;
            }
        }
        return count;
    }
}
