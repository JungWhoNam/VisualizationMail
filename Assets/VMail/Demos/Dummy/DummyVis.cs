using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyVis : MonoBehaviour
{
    public GameObject[] objects;

    public int currObjectIdx { get; private set; }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.SetObjectIndex(this.currObjectIdx + 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.SetObjectIndex(this.currObjectIdx - 1);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            this.SetColor(Random.ColorHSV());
            this.SetScale(new Vector3(Random.Range(0.5f, 2f), Random.Range(0.5f, 2f), Random.Range(0.5f, 2f)));
        }
    }

    private void SetObjectIndex(int idx)
    {
        if (idx < 0 || idx >= this.objects.Length)
        {
            return;
        }

        for (int i = 0; i < this.objects.Length; i++)
        {
            this.objects[i].SetActive(i == idx);
        }

        this.currObjectIdx = idx;
    }

    public DummyVisState GetVisState()
    {
        DummyVisState state = new DummyVisState();
        state.objectIdx = this.currObjectIdx;
        state.color = this.objects[this.currObjectIdx].GetComponent<MeshRenderer>().material.color;
        state.scale = this.objects[this.currObjectIdx].transform.localScale;

        return state;
    }

    public void SetVisState(DummyVisState state)
    {
        this.SetObjectIndex(state.objectIdx);
        this.SetColor(state.color);
        this.SetScale(state.scale);
    }

    public void SetColor(Color c)
    {
        this.objects[this.currObjectIdx].GetComponent<MeshRenderer>().material.color = c;
    }

    public void SetScale(Vector3 scale)
    {
        this.objects[this.currObjectIdx].transform.localScale = scale;
    }


    public class DummyVisState
    {
        public int objectIdx;
        public Color color;
        public Vector3 scale;
    }

}