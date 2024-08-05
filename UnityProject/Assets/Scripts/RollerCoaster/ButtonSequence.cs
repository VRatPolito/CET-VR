using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ButtonSequence : MonoBehaviour
{

    [SerializeField]
    List<GameObject> sequenceSprite;

    [SerializeField]
    int sequenceIndex = 0;

    [SerializeField]
    GameObject speedManager;

    private string nextButton;

    public UnityEvent OnErrorCommitted = new UnityEvent();
    public UnityEvent OnSignSurpassed = new UnityEvent();

    public int correctPressures = 0;
    private bool correctPressure = false;
    private bool wrongPressure = false;

    [SerializeField]
    AudioSource answerBuzzerSource; //source that emits the sounds for responses
        [SerializeField]
    AudioClip wrongAnswer, correctAnswer; //sounds

    private void Awake()
    {
        //the individual elements appear one at a time according to the progression index
        foreach (GameObject g in sequenceSprite)
        {
            updateSign(g, false, Color.black);
        }
    }

    internal void Reset()
    {
        foreach (GameObject g in sequenceSprite)
        {
            changeColor(g, Color.black);

            g.gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = "";
            g.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            g.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;

        }

        sequenceIndex = 0;
        correctPressures = 0;
        correctPressure = false;
        wrongPressure = false;
    }

    public void activateFirstSign()
    {
        GameObject g = sequenceSprite[sequenceIndex];

        updateSign(g, true, Color.blue);
    }
    /*
     * Hides the the sign until the next occurrence
     * The next signal will be displayed in blue -> it becomes green if pressure is correct, red if pressure is incorrect
     */
    public void activateNextSign(bool stop = false)
    {
        if(sequenceIndex == 0)
            updateSign(sequenceSprite[sequenceSprite.Count - 1], false, Color.black);
        else 
            updateSign(sequenceSprite[sequenceIndex - 1], false, Color.black);

        if (!correctPressure && !wrongPressure)
        {
            //if no button was pressed for the previous sign, then count it as an error

            changeColor(sequenceSprite[sequenceIndex], Color.red);
            answerBuzzerSource.PlayOneShot(wrongAnswer);
            OnErrorCommitted?.Invoke();
        }
        else
            correctPressures++;
        OnSignSurpassed?.Invoke();

        if (!stop) //if we are not at the last sign
        {
            sequenceIndex = (sequenceIndex + 1) % sequenceSprite.Count;

            correctPressure = false;
            wrongPressure = false;
            updateSign(sequenceSprite[sequenceIndex], true, Color.blue);
        }

    }
    /*
     * Change the color of the sign based on whether you press correctly or incorrectly in the sequence
     */
    public void sequenceButtonPressed(string pressedButton)
    {
        //if correct pressure, green color
        if (pressedButton == nextButton)
        {
            if (!wrongPressure)
            {
                if (!correctPressure)
                {
                    //panel
                    changeColor(sequenceSprite[sequenceIndex], Color.green);
                    correctPressure = true;
                    answerBuzzerSource.PlayOneShot(correctAnswer);
                }
            }
        }
        else
        {
            if (!wrongPressure)
            {
                changeColor(sequenceSprite[sequenceIndex], Color.red);
                OnErrorCommitted?.Invoke();
                correctPressure = false;
                wrongPressure = true;
            }
            answerBuzzerSource.PlayOneShot(wrongAnswer);
        }
    }

    string getNextButton()
    {
        string butt = "";

        float range = Random.Range(0, 100);

        if((int)range%2 == 0)
        {
            butt += "L";
        }
        else
        {
            butt += "R";
        }
        return butt;
    }
    
    void changeColor(GameObject g, Color col)
    {
        g.gameObject.transform.parent.gameObject.GetComponent<MeshRenderer>().material.color = col;
        Material mat = g.gameObject.transform.parent.gameObject.GetComponent<MeshRenderer>().material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", col);
    }

    void updateSign(GameObject g, bool isEnabled, Color col)
    {
        if (isEnabled)
        {
            g.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            g.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            nextButton = getNextButton();
            g.gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = nextButton;
            changeColor(g, col);
        }
        else //deactivate the sign
        {
            g.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            g.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            changeColor(g, col);
        }
    }
}
