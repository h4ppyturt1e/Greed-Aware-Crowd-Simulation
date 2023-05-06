using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.IO;

public class FlowMap : MonoBehaviour
{   
    const int H_TO_MIN = 60;
    const int MIN_TO_SEC = 60;
    const int SEC_TO_MS = 100;
    const int SINGLE_TEST_LENGTH = 5 * MIN_TO_SEC;
    const int totalTestLength = SINGLE_TEST_LENGTH * 100;
    const int patience = 420;

    [SerializeField] public int totalNPCs = 0;
    [SerializeField] public int marketNPCs = 0;
    [SerializeField] public int movingNPCs = 0;
    [SerializeField] public int idleNPCs = 0;
    [SerializeField] public TMP_Text textElement = null;

    public bool isDataCollection = false;
    public string textValue;

    private int maxMarketNPCs = 5;
    private string path = string.Format("Assets/Logs/flowLog_Patience_{0}.txt", patience);
    private int curTestTime = totalTestLength;
    public List<int> flowMap = new List<int>(totalTestLength);
    private int testIndex = 0;

    private int lastSecond = 0;
    public float averageMarketNPCs = 0f;

    void Start()
    {   
        flowMap.Add(marketNPCs);
        CreateLogFile();
    }

    // Update is called once per frame
    void Update()
    {   
        // get time and format it
        float simTime = Time.time;
        int min = Mathf.FloorToInt(simTime / H_TO_MIN) % MIN_TO_SEC;
        int sec = Mathf.FloorToInt(simTime) % MIN_TO_SEC;
        int ms = Mathf.FloorToInt(simTime * SEC_TO_MS) % SEC_TO_MS;

        string timeStr = string.Format("{0:00}:{1:00}:{2:00}", min, sec, ms);

        if (flowMap.Count < totalTestLength)
        {   
            if (sec != lastSecond && isDataCollection)
            {
                flowMap.Add(marketNPCs);
                averageMarketNPCs = (float) flowMap.Average();
                curTestTime++;

                string line = string.Format("{0}", averageMarketNPCs) + System.Environment.NewLine;
                if (curTestTime >= SINGLE_TEST_LENGTH)
                {
                    curTestTime = 0;
                    testIndex++;

                    WriteLogLine(line, true);
                } else {
                    WriteLogLine(line);
                }
            }
        }

        string testFinished = flowMap.Count >= totalTestLength ? "TEST FINISHED!" : "";
        

        string full = string.Format("Marketplace full (> {0})", maxMarketNPCs);
        string Warning = isMarketFull() ? full: "";
        

        textValue = string.Format("Simulation time: {5}\n\nTotal NPCs: {0}\nMarket NPCs: {1}\nMoving NPCs: {2}\nIdle NPCs: {3}\nAverage flowrate: {6}\n{7}\n\n{4}", 
        totalNPCs, marketNPCs, movingNPCs, idleNPCs, Warning, timeStr, averageMarketNPCs, testFinished);

        if (textElement != null) textElement.text = textValue;

        lastSecond = sec;
    }

    public bool isMarketFull()
    {
        return marketNPCs > maxMarketNPCs;
    }

    private void CreateLogFile()
    {
        File.WriteAllText(path, "");
    }

    private void WriteLogLine(string line, bool newTest = false)
    {
        if (newTest)
        {
            string header = System.Environment.NewLine + string.Format("Test {0} - Patience: {1}", testIndex, patience) + System.Environment.NewLine;
            File.AppendAllText(path, header);
        }

        File.AppendAllText(path, line);
    }
}


/*
TODO: 
add a timer to see how long the simulation has been running

add an average flow counter
update every second and add to a list
then average the list

do TEN MINUTES-second tests for 
    1. all NPCs at 50 patience
    2. all NPCs at 99 patience
    3. all NPCs at 0 patience
    4. all NPCs at random patience

google sheets that badboi

"It is hypothesized that incorporating greed as a behavioral factor 
can significantly affect crowd dynamics in commercial environments, 
and this approach aims to investigate and quantify this impact."
*/