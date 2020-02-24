using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagManager : MonoBehaviour
{
    List<Character> characters;
    Character current_it_player;

    // Start is called before the first frame update
    void Start()
    {
        //lets fill a list with all of our characters
        characters = new List<Character>();
        foreach(Character c in FindObjectsOfType<Character>())
        {
            characters.Add(c);
            c.Manager = this;
        }

        //then we should assign one of these to be the tag target at random
        int tag_index = Random.Range(0, characters.Count);

        characters[tag_index].IsIt = true;
        current_it_player = characters[tag_index];
    }

    public Character ItPlayer
    {
        get { return current_it_player; }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
