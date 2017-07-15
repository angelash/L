using UnityEngine;
using System.Collections;

public class FPSUIMgr : MonoBehaviour {

    private int frames;
    private float timeNow;
    private float lastInterval;
    private float realtimeFps;
    private float averageFps;
    private float totalFps;
    private int fpsCount;
    private float updateInterval = 0.5f;
    private const int MAX_STAT_COUNT = 6;

    private UILabel m_lbl;

    void Awake()
    {
        m_lbl = transform.FindChild("Panel/Label").GetComponent<UILabel>();
    }

	// Use this for initialization
	void Start () {
        lastInterval = Time.realtimeSinceStartup;
	}
	
	// Update is called once per frame
	void Update () {
        ++frames;
        timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            realtimeFps = frames / (timeNow - lastInterval);
            frames = 0;
            lastInterval = timeNow;
            //UpdateAverageFps();
            string content = string.Format("FPS: {0}", (int)realtimeFps);
            m_lbl.text = content;
        }
	}

    /*
    void OnGUI()
    {
        string content = string.Format(" FPS: {0}", (int)realtimeFps);
        GUI.backgroundColor = Color.grey;
        GUI.contentColor = Color.white;
        GUI.skin.textArea.fontSize = 12;
        GUI.TextArea(new Rect(5, 73, 70, 20), content, 90);
    }
     * */

    private void UpdateAverageFps()
    {
        totalFps += realtimeFps;
        fpsCount++;
        if (fpsCount >= MAX_STAT_COUNT)
        {
            averageFps = totalFps / fpsCount;
            totalFps = 0;
            fpsCount = 0;
        }
    }
}
