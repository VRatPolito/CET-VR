using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{

    [SerializeField]
    private GameObject[] timers;
    [SerializeField]
    bool _showFromTheBeginning = false;

    private void Awake()
    {
        if (!_showFromTheBeginning)
            hideTimers();
    }

    public void hideTimers()
    {
        foreach (GameObject t in timers)
        {
            t.gameObject.transform.Find("RoundTextTMP").gameObject.GetComponent<TextMeshPro>().enabled = false;
            t.gameObject.transform.Find("TimerTextTMP").gameObject.GetComponent<TextMeshPro>().enabled = false;
        }
    }
    public void showTimers()
    {
        foreach (GameObject t in timers)
        {
            t.gameObject.transform.Find("RoundTextTMP").gameObject.GetComponent<TextMeshPro>().enabled = true;
            t.gameObject.transform.Find("TimerTextTMP").gameObject.GetComponent<TextMeshPro>().enabled = true;
        }
    }
    public void resetTimers()
    {
        foreach(GameObject t in timers)
        {
            t.gameObject.transform.Find("RoundTextTMP").gameObject.GetComponent<TextMeshPro>().text = "Ready to";
            t.gameObject.transform.Find("TimerTextTMP").gameObject.GetComponent<TextMeshPro>().text = "START";
        }
    }
    public void setTimers(string firstLine, string secondLine)
    {
        foreach (GameObject t in timers)
        {
            t.gameObject.transform.Find("RoundTextTMP").gameObject.GetComponent<TextMeshPro>().text = firstLine;
            t.gameObject.transform.Find("TimerTextTMP").gameObject.GetComponent<TextMeshPro>().text = secondLine;

        }
    }

}
