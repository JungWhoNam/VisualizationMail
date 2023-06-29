using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VMail;

public class DummyVisStateComparator : VisStateComparator
{
    public DummyVis vis;

    // three possible cases
    // 1) returns null, they are different and there is no valid transition.
    // 2) returns empty, if they are the same.
    // 3) returns non-empty, they are different and there is a valid transition.
    // returning list wil be used for "SetTransition(List<string> diff, float amt)" for the case 3!
    public override List<KeyValuePair<string, string>> GetVisDiffs(string fromState, string toState)
    {
        DummyVis.DummyVisState from = JsonUtility.FromJson<DummyVis.DummyVisState>(fromState);
        DummyVis.DummyVisState to = JsonUtility.FromJson<DummyVis.DummyVisState>(toState);

        if (from == null || to == null)
        {
            return null;
        }

        if (from.objectIdx != to.objectIdx)
        {
            return null;
        }

        List<KeyValuePair<string, string>> diffs = new List<KeyValuePair<string, string>>();

        // color is different
        if (!from.color.ToString().Equals(to.color.ToString()))
        {
            diffs.Add(new KeyValuePair<string, string>("color", from.color.ToString() + ";" + to.color.ToString()));
        }

        // scale is different
        if (!from.scale.ToString().Equals(to.scale.ToString()))
        {
            diffs.Add(new KeyValuePair<string, string>("scale", from.scale.ToString() + ";" + to.scale.ToString()));
        }

        return diffs;
    }

    public override void SetVisTransition(List<KeyValuePair<string, string>> diffs, float amt)
    {
        foreach (KeyValuePair<string, string> diff in diffs)
        {
            if (diff.Key.Equals("color"))
            {
                string[] fromTo = diff.Value.Split(';');
                Color from = this.StringToRGBAColor(fromTo[0]);
                Color to = this.StringToRGBAColor(fromTo[1]);

                Color color = Color.Lerp(from, to, amt);
                this.vis.SetColor(color);
            }
            else if (diff.Key.Equals("scale"))
            {
                string[] fromTo = diff.Value.Split(';');
                Vector3 from = this.StringToVector3(fromTo[0]);
                Vector3 to = this.StringToVector3(fromTo[1]);

                Vector3 scale = Vector3.Lerp(from, to, amt);
                this.vis.SetScale(scale);
            }
        }
    }

    // https://answers.unity.com/questions/1134997/string-to-vector3.html
    private Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    private Color StringToRGBAColor(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("RGBA(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(5, sVector.Length - 6);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Color c = new Color(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));

        return c;
    }

}