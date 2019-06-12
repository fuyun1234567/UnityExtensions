using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Yielders
{
    static WaitForEndOfFrame _waitEndOfFrame;

    public static WaitForEndOfFrame WaitEndOfFrame
    {
        get
        {
            lock (_waitEndOfFrame)
            {
                if (_waitEndOfFrame == null)
                {
                    _waitEndOfFrame = new WaitForEndOfFrame();
                }
            }
            return _waitEndOfFrame;
        }
    }
    static WaitForFixedUpdate _waitFixedUpdate;

    public static WaitForFixedUpdate WaitFixedUpdate
    {
        get
        {
            lock (_waitFixedUpdate)
            {
                if (_waitFixedUpdate == null)
                {
                    _waitFixedUpdate = new WaitForFixedUpdate();
                }
            }
            return _waitFixedUpdate;
        }
    }

    static Dictionary<float,WaitForSeconds> _waitSecondsCollection=new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds WaitSeconds(float seconds)
    {
        lock (_waitSecondsCollection)
        {
            if (_waitSecondsCollection.ContainsKey(seconds))
            {
                return _waitSecondsCollection[seconds];
            }
            else
            {
                WaitForSeconds w = new WaitForSeconds(seconds);
                _waitSecondsCollection.Add(seconds, w);
                return w;
            }
        }
    }
}
