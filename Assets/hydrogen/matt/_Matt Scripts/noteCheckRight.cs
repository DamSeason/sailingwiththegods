using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class noteCheckRight : MonoBehaviour
{
    public bool canBePressed;

    //public KeyCode keyToPress;
    private Button btn;

    // Start is called before the first frame update
    void Start()
    {
        btn = GameObject.FindWithTag("RightActivator").GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {

       btn.onClick.AddListener(() => correctNoteHit());

        if (Input.GetKeyDown("right"))
        {
            correctNoteHit();
        }


}


	//When note is hit correctly 
    public void correctNoteHit()
    {
		if(canBePressed)
           {
            gameObject.SetActive(false);
            canBePressed = false;
            GameManager.instance.NoteHit();
            }
    }

	private void OnTriggerEnter2D (Collider2D other)
    {
		if(other.tag == "RightActivator")
        {
            canBePressed = true;
        }
    } 

    private void OnTriggerExit2D(Collider2D other)
    {
        if (this.gameObject.activeSelf)
        {
            if (other.tag == "RightActivator")
            {
                canBePressed = false;

                GameManager.instance.NoteMissed();
            }
        }
    }
}
