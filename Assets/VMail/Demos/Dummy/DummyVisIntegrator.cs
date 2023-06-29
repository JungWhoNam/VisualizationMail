using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VMail;

public class DummyVisIntegrator : VisIntegrator
{
    public DummyVis vis;

    public override void SetVisState(string visState)
    {
        DummyVis.DummyVisState state = JsonUtility.FromJson<DummyVis.DummyVisState>(visState);

        this.vis.SetVisState(state);
    }

    public override string GetVisState()
    {
        DummyVis.DummyVisState state = this.vis.GetVisState();

        return JsonUtility.ToJson(state);
    }

}