using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SingletonFoEveryton : MonoBehaviour
{
    #region Singleton Setup
    private static SingletonFoEveryton _instance;
    public static SingletonFoEveryton Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this);
        else
            _instance = this;
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        var hmdDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);

        foreach (var device in hmdDevices)
        {
            //device.subsystem.TryRecenter();

            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
