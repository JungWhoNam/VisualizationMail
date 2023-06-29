using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail
{
    public class VisStateComparator : MonoBehaviour
    {
        public virtual bool IsCameraLocationDiff(Vector3 pos0, Vector3 pos1, Quaternion rot0, Quaternion rot1)
        {
            float distThreshold = 0.0001f;
            if (Vector3.Distance(pos0, pos1) > distThreshold)
            {
                return true;
            }

            float angleThreshold = 1f;
            if (Quaternion.Angle(rot0, rot1) > angleThreshold)
            {
                return true;
            }

            return false;
        }


        // returns null, they are different and there is no valid transition.
        // returns empty, if they are the same.
        // returns non-empty, they are different and there is a valid transition.
        public virtual List<KeyValuePair<string, string>> GetVisDiffs(string fromState, string toState)
        {
            return null;
        }

        public virtual void SetVisTransition(List<KeyValuePair<string, string>> diffs, float amt)
        {

        }

        public virtual void SetCameraTransition(Vector3? pos0, Vector3? pos1, Quaternion? rot0, Quaternion? rot1, float amt)
        {
            if (!pos0.HasValue && !pos1.HasValue)
            {
                return;
            }

            if (!rot0.HasValue && !rot1.HasValue)
            {
                return;
            }

            // assume at least one has a value
            Vector3 posFrom = pos0 ?? pos1.Value;
            Vector3 posTo = pos1 ?? pos0.Value;
            Camera.main.transform.position = Vector3.Lerp(posFrom, posTo, amt);

            // assume at least one has a value
            Quaternion rotFrom = rot0 ?? rot1.Value;
            Quaternion rotTo = rot1 ?? rot0.Value;
            Camera.main.transform.rotation = Quaternion.Lerp(rotFrom, rotTo, amt);
        }

    }
}