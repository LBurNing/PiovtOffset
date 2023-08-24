using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupUI : MonoBehaviour
{
    private Button openBtn;
    private Button addFrameBtn;
    private Button deleteGrpup;
    private Text progress;
    private InputField groupName;
    private InputField groupPath;
    private GameObject content;
    private GameObject templete;
    private GameObject frameGroup;
    private ScrollRect frameScrollRect;

    private void Awake()
    {
        openBtn = transform.Find("btn/open").GetComponent<Button>();
        deleteGrpup = transform.Find("btn/Delete").GetComponent<Button>();
        frameGroup = transform.Find("FrameGroup").gameObject;
        addFrameBtn = transform.Find("btn/addFrame").GetComponent<Button>();
        groupName = transform.Find("btn/groupName").GetComponent<InputField>();
        groupPath = transform.Find("btn/path").GetComponent<InputField>();
        templete = transform.Find("Frame").gameObject;
        content = transform.Find("FrameGroup/Viewport/Content").gameObject;
        frameScrollRect = transform.Find("FrameGroup").GetComponent<ScrollRect>();
        progress = transform.Find("btn/progress").GetComponent<Text>();
    }

    void Start()
    {
        openBtn.onClick.AddListener(OnOpen);
        addFrameBtn.onClick.AddListener(OnAddFrame);
        deleteGrpup.onClick.AddListener(OnDeleteFrame);
        groupName.onValueChanged.AddListener(OnGroupNameChanged);
    }

    public void Find()
    {

    }

    public void InitFrames(List<Frame> frames)
    {
        foreach(Frame frame in frames)
        {
            CreateFrameUI frameUI = CreateFrameUI();
            frameUI.Frame = frame;
        }
    }

    private void OnGroupNameChanged(string value)
    {
    }

    private void OnDeleteFrame()
    {
        Destroy(gameObject);
    }

    private void OnOpen()
    {
        frameGroup.SetActive(!frameGroup.activeInHierarchy);
        openBtn.transform.rotation = Quaternion.Euler(0, 0, openBtn.transform.rotation.eulerAngles.z == 90 ? 0 : 90);
        ForceRebuildLayoutImmediate();
    }

    private void OnAddFrame()
    {
        CreateFrameUI();
        ForceRebuildLayoutImmediate();
    }

    private CreateFrameUI CreateFrameUI()
    {
        GameObject go = Instantiate(templete);
        go.SetActive(true);
        go.transform.SetParent(content.transform, false);
        CreateFrameUI frameUI = go.GetComponent<CreateFrameUI>();
        frameUI.deleteCallBack = UpdateHeight;

        UpdateHeight();
        return frameUI;
    }

    private void ForceRebuildLayoutImmediate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
        frameScrollRect.verticalNormalizedPosition = 0;
    }

    public void ShowProgress(int cur, int total, string error)
    {
        if (progress == null)
            return;

        if (error != null)
        {
            progress.text = string.Format("<color=#FF0000> {0}</color>", error);
        }
        else
        {
            progress.text = string.Format(" {0}/{1}", cur, total);
        }
    }

    public void UpdateHeight()
    {
        int height = content.transform.childCount * 30;
        height = height >= 170 ? 170 : height;
        RectTransform rt = frameScrollRect.transform as RectTransform;
        rt.sizeDelta = new Vector2(460, height);
    }

    public string GroupName
    {
        get
        {
            return groupName.text;
        }

        set
        {
            groupName.text = value;
        }
    }

    public string GroupPath
    {
        get
        {
            return groupPath.text;
        }

        set
        {
            groupPath.text = value;
        }
    }

    public FrameSet FrameSet
    {
        get
        {
            FrameSet frame = new FrameSet();
            for (int i = 0; i < content.transform.childCount; i++)
            {
                CreateFrameUI frameUI = content.transform.GetChild(i).GetComponent<CreateFrameUI>();
                frame.Name2Frames.Add(frameUI.Name, frameUI.Frame);
            }

            return frame;
        }
    }

    public List<CreateFrameUI> frameUIs
    {
        get
        {
            List<CreateFrameUI> frames = new List<CreateFrameUI>();
            for (int i = 0; i < content.transform.childCount; i++)
            {
                CreateFrameUI frameUI = content.transform.GetChild(i).GetComponent<CreateFrameUI>();
                frames.Add(frameUI);
            }

            return frames;
        }
    }
}
