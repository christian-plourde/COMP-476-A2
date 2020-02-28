using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TagManager : MonoBehaviour
{
    List<Character> characters;
    Character current_it_player;
    Character last_char_to_spot_it_player; //the last person to spot the it player
    public bool navMesh = false;

    public bool NavMesh
    {
        get { return navMesh; }
        set {

            navMesh = value;
            foreach(Character c in characters)
            {
                c.gameObject.GetComponent<NavMeshAgent>().enabled = navMesh;
            }

        }
    }

    public Character LastSpotter
    {
        get { return last_char_to_spot_it_player; }
    }

    public void ResetGame()
    {
        Debug.Log("resetting game");
        //lets fill a list with all of our characters
        characters = new List<Character>();

        foreach (Character c in FindObjectsOfType<Character>())
        {
            characters.Add(c);
            c.Manager = this;
        }

        //then we should assign one of these to be the tag target at random

        int tag_index = Random.Range(0, characters.Count);
        Debug.Log(characters[tag_index].name + " is it.");
        

        for (int i = 0; i < characters.Count; i++)
        {
            if (i == tag_index)
            {
                characters[tag_index].IsIt = true;
                current_it_player = characters[tag_index];
                current_it_player.gameObject.GetComponent<CapsuleCollider>().tag = "ItPlayer";
            }

            else
            {
                characters[i].IsIt = false;
                characters[i].gameObject.GetComponent<CapsuleCollider>().tag = "Untagged";
            }
        }

        NavMesh = navMesh;
    }

    // Start is called before the first frame update
    void Awake()
    {
        ResetGame();
    }

    public Character ItPlayer
    {
        get { return current_it_player; }
    }

    //indicates if any of the characters have spotted the it player
    public bool ItPlayerSpotted()
    {
        foreach(Character c in characters)
        {
            if (c.ItPlayerVisible())
            {
                //if this character can see the it player we record this as the last character to have seen the it player
                last_char_to_spot_it_player = c;
                return true;
            }
                
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
